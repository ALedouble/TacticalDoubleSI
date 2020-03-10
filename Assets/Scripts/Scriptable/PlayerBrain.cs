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

        SelectionManager.Instance.OnClick += OnMovement;
        HUDManager.Instance.OnAbilityClicked += OnAbilitySelected;

        MapManager.Instance.castableTiles.Clear();

        reachableTiles = IAUtils.FindAllReachablePlace(entityBehaviour.GetPosition(), entityBehaviour.CurrentActionPoints);
        reachableTiles.RemoveAll(x => x.GetCoordPosition() == entityBehaviour.GetPosition());
        // TEMPORARY -> will replace with real fx 
        MapManager.Instance.reachableTiles = reachableTiles;
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

        ReachableTile reachableTile = IAUtils.FindShortestPath(false, entityBehaviour.GetPosition(), hit.position, true, entityBehaviour.CurrentActionPoints);

        if (reachableTile == null)
        {
            Debug.LogError("Entity " + entityBehaviour.name + " cannot move to " + hit.position, entityBehaviour.gameObject);
            Debug.Break();
        }

        // TEMPORARY
        MapManager.Instance.reachableTiles.Clear();
        MapManager.Instance.castableTiles.Clear();
        MapManager.SetEffectTilesPreview(null);

        Sequence moveSequence = entityBehaviour.MoveTo(reachableTile);

        moveSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnMovement;
            HUDManager.Instance.OnAbilityClicked += OnAbilitySelected;
        });
    }

    int selectedAbilityIndex;

    void OnAbilitySelected(int index)
    {
        if (index > entityBehaviour.data.abilities.Count - 1) return;

        selectedAbilityIndex = index;

        SelectionManager.Instance.OnClick -= OnMovement;
        MapManager.Instance.reachableTiles.Clear();

        SelectionManager.Instance.OnClick += OnUseAbility;
        SelectionManager.Instance.OnHoveredTileChanged += UpdateAbilityEffectArea;

        castableTiles = entityBehaviour.data.abilities[index].castArea.GetWorldSpace(entityBehaviour.GetPosition());

        if (!entityBehaviour.data.abilities[index].canCastOnEntityPosition)
        {
            castableTiles.RemoveAll(x => !MapManager.GetTile(x).IsWalkable);
        }
        
 
        MapManager.SetCastableTilesPreview(castableTiles);
    }

    void OnUseAbility(MapRaycastHit hit)
    {
        if (hit.tile == null) return;
        if (!castableTiles.Contains(hit.position)) return;

        // TEMPORARY
        MapManager.SetCastableTilesPreview(null);
        MapManager.SetEffectTilesPreview(null);

        SelectionManager.Instance.OnClick -= OnUseAbility;
        SelectionManager.Instance.OnHoveredTileChanged += UpdateAbilityEffectArea;

        Sequence attackSequence = entityBehaviour.UseAbility(entityBehaviour.GetAbilities(selectedAbilityIndex), hit.tile);

        attackSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnMovement;
            HUDManager.Instance.OnAbilityClicked += OnAbilitySelected;
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

        MapManager.SetEffectTilesPreview(entityBehaviour.data.abilities[selectedAbilityIndex].effectArea.GetWorldSpaceRotated(entityBehaviour.GetPosition(), hit.position));
    }
}
