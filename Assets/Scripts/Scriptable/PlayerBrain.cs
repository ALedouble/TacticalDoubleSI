using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "PlayerBrain", menuName = "ScriptableObjects/PlayerBrain", order = 999)]
public class PlayerBrain : Brain
{
    EntityBehaviour entityBehaviour;
    public List<Vector2Int> castableTiles;
    List<ReachableTile> reachableTiles;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
       
        base.OnTurnStart(entityBehaviour);
       
        this.entityBehaviour = entityBehaviour;

        SelectionManager.Instance.OnCancel += CancelEverything;
        SelectionManager.Instance.OnCancel += CanSelectAnotherPlayer;
        HUDManager.Instance.OnEndTurnPressed += CancelEverything;


        SelectionManager.Instance.OnClick -= OnMovement;
        SelectionManager.Instance.OnClick += OnMovement;
        HUDManager.Instance.OnAbilityClicked -= OnAbilitySelected;
        HUDManager.Instance.OnAbilityClicked += OnAbilitySelected;

        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);

        reachableTiles = IAUtils.FindAllReachablePlace(entityBehaviour.GetPosition(), entityBehaviour.CurrentActionPoints);
        reachableTiles.RemoveAll(x => x.GetCoordPosition() == entityBehaviour.GetPosition());
        // TEMPORARY -> will replace with real fx 
        MapManager.SetReachableTilesPreview(reachableTiles);

        
    }

    void CanSelectAnotherPlayer()
    {
        SelectionManager.Instance.OnEntitySelect += RoundManager.Instance.StartPlayerTurn;
    }

    void CancelEverything()
    {
        SelectionManager.Instance.OnClick -= OnMovement;
        HUDManager.Instance.OnAbilityClicked -= OnAbilitySelected;
        SelectionManager.Instance.OnHoveredTileChanged -= UpdateAbilityEffectArea;

        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);
        MapManager.SetReachableTilesPreview(null);

        SelectionManager.Instance.OnCancel -= CanSelectAnotherPlayer;
    }

    void OnMovement(MapRaycastHit hit)
    {
        if (hit.tile == null) return;

        bool canReachTile = false;
        for (int i = 0; i < reachableTiles.Count; i++)
        {
            if (reachableTiles[i].GetCoordPosition() == hit.position)
            {
                canReachTile = true;
                break;
            }
        }

        if (!canReachTile) return;

        SelectionManager.Instance.OnClick -= OnMovement;
        HUDManager.Instance.OnAbilityClicked -= OnAbilitySelected;

        ReachableTile reachableTile = IAUtils.FindShortestPath(false, entityBehaviour.GetPosition(), hit.position, true, entityBehaviour.CurrentActionPoints);

        if (reachableTile == null)
        {
            Debug.LogError("Entity " + entityBehaviour.name + " cannot move to " + hit.position, entityBehaviour.gameObject);
            Debug.Break();
        }

        // TEMPORARY
        MapManager.SetReachableTilesPreview(null);
        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);
        FXManager.Instance.pathRenderer.positionCount = 0;

        Sequence moveSequence = entityBehaviour.MoveTo(reachableTile);

        moveSequence.OnComplete(() =>
        {
            OnTurnStart(entityBehaviour);
            SelectionManager.Instance.OnEntitySelect += RoundManager.Instance.StartPlayerTurn;
        });
    }

    int selectedAbilityIndex;

    void OnAbilitySelected(int index)
    {
        if (index > entityBehaviour.data.abilities.Count - 1) return;

        if (entityBehaviour.CurrentActionPoints < entityBehaviour.data.abilities[index].cost) return;

        selectedAbilityIndex = index;

        SelectionManager.Instance.OnClick -= OnMovement;
        MapManager.SetReachableTilesPreview(null);

        SelectionManager.Instance.OnClick -= OnUseAbility;
        SelectionManager.Instance.OnClick += OnUseAbility;
        SelectionManager.Instance.OnHoveredTileChanged += UpdateAbilityEffectArea;
        SelectionManager.Instance.OnEntitySelect -= RoundManager.Instance.StartPlayerTurn;

        castableTiles = entityBehaviour.data.abilities[index].castArea.GetWorldSpace(entityBehaviour.GetPosition());

        if (!entityBehaviour.data.abilities[index].canCastOnEntityPosition)
        {
            castableTiles.RemoveAll(x => !MapManager.GetTile(x).IsWalkable);
        }
        castableTiles.RemoveAll(x => MapManager.GetTile(x.x, x.y).tileType == TileType.Solid);

        MapManager.SetCastableTilesPreview(castableTiles);
    }

    void OnUseAbility(MapRaycastHit hit)
    {
        if (hit.tile == null) return;
        if (!castableTiles.Contains(hit.position)) return;

        // TEMPORARY
        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);

        SelectionManager.Instance.OnHoveredTileChanged -= UpdateAbilityEffectArea;
        HUDManager.Instance.OnAbilityClicked -= OnAbilitySelected;
        SelectionManager.Instance.OnClick -= OnUseAbility;
        SelectionManager.Instance.OnClick -= OnMovement;
        SelectionManager.Instance.OnHoveredTileChanged -= UpdateAbilityEffectArea;

        if (entityBehaviour.GetAbilities(selectedAbilityIndex).Channeling)
        {
            entityBehaviour.channelingRoundsLeft = 1;
            
            entityBehaviour.channelingAbility = entityBehaviour.GetAbilities(selectedAbilityIndex);
            SelectionManager.Instance.OnEntitySelect += RoundManager.Instance.StartPlayerTurn;
            animBlastOut(entityBehaviour, entityBehaviour.channelingAbility);
            return;
        }
         Sequence attackSequence = entityBehaviour.UseAbility(entityBehaviour.GetAbilities(selectedAbilityIndex), hit.tile);

        
        attackSequence.OnComplete(() =>
        {
            OnTurnStart(entityBehaviour);
            SelectionManager.Instance.OnEntitySelect += RoundManager.Instance.StartPlayerTurn;
        });

        List<Vector2Int> effectTiles = entityBehaviour.data.abilities[selectedAbilityIndex].effectArea.GetWorldSpaceRotated(entityBehaviour.GetPosition(), hit.position);

    }

    // Maybe temporary, we need a function to subcribe for updating area effect
    void UpdateAbilityEffectArea(MapRaycastHit hit)
    {
        if (hit.tile == null || !castableTiles.Contains(hit.position))
        {
            MapManager.SetEffectTilesPreview(null);
            return;
        }

        List<Vector2Int> effectTiles = entityBehaviour.data.abilities[selectedAbilityIndex].effectArea.GetWorldSpaceRotated(entityBehaviour.GetPosition(), hit.position);
        effectTiles.RemoveAll(x => MapManager.GetTile(x.x, x.y).tileType == TileType.Solid);

        MapManager.SetEffectTilesPreview(effectTiles);
    }

    Sequence animBlastOut(EntityBehaviour entity, Ability ability)
    {
        Sequence blastOutSequence = DOTween.Sequence();

        EntityAnimation anim = entity.data.animations.channelingStartAnimation;
        blastOutSequence.AppendCallback(() =>
        {
            entity.animator.PlayAnimation(anim);
        });

        blastOutSequence.AppendInterval(3f);

       

        blastOutSequence.AppendCallback(() =>
        {
            anim = entity.data.animations.channelingIdleAnimation;
            entity.animator.PlayAnimation(anim);
        });

            return blastOutSequence;
    }
}
