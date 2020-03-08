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
        MapManager.Instance.effectTiles.Clear();

        Sequence moveSequence = entityBehaviour.MoveTo(reachableTile);

        moveSequence.OnComplete(() =>
        {
            //SelectionManager.Instance.OnClick += OnMovement;
            RoundManager.Instance.EndTurn(entityBehaviour);
        });
    }

    

    void OnAbilitySelected(int index)
    {
        if (index > entityBehaviour.data.abilities.Count - 1) return;

        SelectionManager.Instance.OnClick -= OnMovement;
        MapManager.Instance.reachableTiles.Clear();

        SelectionManager.Instance.OnClick += OnUseAbility;

        castableTiles = entityBehaviour.data.abilities[index].castArea.GetWorldSpace(entityBehaviour.GetPosition());

        MapManager.Instance.castableTiles = castableTiles; //TEMPORARY : For DrawColor in DebugMapVizualizer


    }

    void OnUseAbility(MapRaycastHit hit)
    {
        if (hit.tile == null) return;
        if (!castableTiles.Contains(hit.position)) return;

        MapManager.Instance.castableTiles.Clear();

        SelectionManager.Instance.OnClick -= OnUseAbility;

        Sequence attackSequence = entityBehaviour.UseAbility(entityBehaviour.GetAbilities(0), hit.tile);

        attackSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnUseAbility;
        });

        List<Vector2Int> effectTiles = entityBehaviour.data.abilities[0].effectArea.GetWorldSpaceRotated(entityBehaviour.GetPosition(), hit.position);

        MapManager.Instance.effectTiles = effectTiles; //TEMPORARY : For DrawColor in DebugMapVizualizer
    }
}
