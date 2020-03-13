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
        HUDManager.Instance.DeselectAbility();

        base.OnTurnStart(entityBehaviour);
       
        this.entityBehaviour = entityBehaviour;

        CancelEverything();
        SelectionManager.Instance.OnEntitySelect -= RoundManager.Instance.StartPlayerTurn;
        SelectionManager.Instance.OnEntitySelect += RoundManager.Instance.StartPlayerTurn;

        SelectionManager.Instance.OnEntitySelect += DeselectThis;

        SelectionManager.Instance.OnCancel += CancelEverything;
        SelectionManager.Instance.OnCancel += CanSelectAnotherPlayer;
        HUDManager.Instance.OnEndTurnPressed += CancelEverything;


        SelectionManager.Instance.OnClick -= OnMovement;
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

    void DeselectThis(EntityBehaviour entity)
    {
        SelectionManager.Instance.OnEntitySelect -= DeselectThis;

        if (entity == entityBehaviour || entity == null)
        {
            return;
        }

        SelectionManager.Instance.OnClick -= OnMovement;
        HUDManager.Instance.OnAbilityClicked -= OnAbilitySelected;
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
        SelectionManager.Instance.OnClick -= OnUseAbility;

        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);
        MapManager.SetReachableTilesPreview(null);

        SelectionManager.Instance.OnCancel -= CanSelectAnotherPlayer;

        HUDManager.Instance.DeselectAbility();

        SelectionManager.Instance.OnEntitySelect -= HUDManager.Instance.UpdateEntityInfo;
        SelectionManager.Instance.OnEntitySelect += HUDManager.Instance.UpdateEntityInfo;
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

        SelectionManager.Instance.OnEntitySelect -= RoundManager.Instance.StartPlayerTurn;
        SelectionManager.Instance.OnClick -= OnMovement;
        SelectionManager.Instance.OnClick -= OnMovement;
        HUDManager.Instance.OnAbilityClicked -= OnAbilitySelected;

        ReachableTile reachableTile = IAUtils.FindShortestPath(false, entityBehaviour.GetPosition(), hit.position, true, entityBehaviour.CurrentActionPoints);

        if (reachableTile == null)
        {
            Debug.Break();
        }

        // TEMPORARY
        MapManager.SetReachableTilesPreview(null);
        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);
        FXManager.Instance.pathRenderer.positionCount = 0;

        Sequence moveSequence = entityBehaviour.MoveTo(reachableTile);

        HUDManager.Instance.HideEndTurnButton();

        moveSequence.OnComplete(() =>
        {
            OnTurnStart(entityBehaviour);
            CanSelectAnotherPlayer();
            HUDManager.Instance.ShowEndTurnButton();
        });
    }

    int selectedAbilityIndex;

    void OnAbilitySelected(int index)
    {
        if (index > entityBehaviour.data.abilities.Count - 1) return;

        if (entityBehaviour.CurrentActionPoints < entityBehaviour.data.abilities[index].cost) return;

        if (index == 3)
        {
            if (PlayerTeamManager.Instance.teamXp < 10)
            {
                return;
            }
        }
        selectedAbilityIndex = index;

        if(index<=2) HUDManager.Instance.SelectAbility(index);

        SelectionManager.Instance.OnEntitySelect -= HUDManager.Instance.UpdateEntityInfo;
        SelectionManager.Instance.OnEntitySelect -= HUDManager.Instance.UpdateEntityInfo;

        SelectionManager.Instance.OnClick -= OnMovement;
        MapManager.SetReachableTilesPreview(null);

        SelectionManager.Instance.OnClick -= OnUseAbility;
        SelectionManager.Instance.OnClick += OnUseAbility;
        SelectionManager.Instance.OnHoveredTileChanged += UpdateAbilityEffectArea;
        SelectionManager.Instance.OnEntitySelect -= RoundManager.Instance.StartPlayerTurn;

        castableTiles = entityBehaviour.data.abilities[index].castArea.GetWorldSpace(entityBehaviour.GetPosition());

        SelectionManager.Instance.OnClick += CancelAbility;

        if (!entityBehaviour.data.abilities[index].canCastOnEntityPosition)
        {
            castableTiles.RemoveAll(x => !MapManager.GetTile(x).IsWalkable);
        }
        castableTiles.RemoveAll(x => MapManager.GetTile(x.x, x.y).tileType == TileType.Solid);

        MapManager.SetCastableTilesPreview(castableTiles);
    }

    void CancelAbility(MapRaycastHit mapHit)
    {
        SelectionManager.Instance.OnClick -= CancelAbility;

        if (castableTiles.Contains(mapHit.position))
        {
            return;
        }

        CancelEverything();
        CanSelectAnotherPlayer();
    }

    void OnUseAbility(MapRaycastHit hit)
    {
        if (hit.tile == null) return;
        if (!castableTiles.Contains(hit.position)) return;


        if (selectedAbilityIndex == 3)
        {
            if (PlayerTeamManager.Instance.teamXp < 10)
            {
                return;
            }
            else
            {
                PlayerTeamManager.Instance.teamXp = 0;
                PlayerTeamManager.Instance.OnXPChanged?.Invoke();
            }
        }

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


        HUDManager.Instance.HideEndTurnButton();

        attackSequence.OnComplete(() =>
        {
            OnTurnStart(entityBehaviour);
            HUDManager.Instance.ShowEndTurnButton();
            SelectionManager.Instance.OnEntitySelect += HUDManager.Instance.UpdateEntityInfo;
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
