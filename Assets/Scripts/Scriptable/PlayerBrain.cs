using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "PlayerBrain", menuName = "ScriptableObjects/PlayerBrain", order = 999)]
public class PlayerBrain : Brain
{
    EntityBehaviour entityBehaviour;

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

        ReachableTile reachableTile = IAUtils.FindShortestPath(entityBehaviour.GetPosition(), hit.position, true, entityBehaviour.CurrentActionPoints);

        if (reachableTile == null)
        {
            Debug.LogError("Entity " + entityBehaviour.name + " cannot move to " + hit.position, entityBehaviour.gameObject);
            Debug.Break();
        }

        // TEMPORARY
        MapManager.Instance.reachableTiles.Clear();

        Sequence moveSequence = entityBehaviour.MoveTo(reachableTile);

        moveSequence.OnComplete(() =>
        {
            //SelectionManager.Instance.OnClick += OnMovement;
            RoundManager.Instance.EndTurn(entityBehaviour);
        });
    }

    public List<Vector2Int> castableTiles;

    void OnAbilitySelected(int index)
    {
        if (index > entityBehaviour.data.abilities.Count - 1) return;

        SelectionManager.Instance.OnClick -= OnMovement;
        MapManager.Instance.reachableTiles.Clear();

        SelectionManager.Instance.OnClick += OnUseAbility;

        castableTiles = new List<Vector2Int>(entityBehaviour.GetTileForCast(entityBehaviour.data.abilities[index].castArea));

        // TEMPORARY : GetTile(s)ForCast devrait return des cases en WORLD SPACE
        for (int i = 0; i < castableTiles.Count; i++)
        {
            castableTiles[i] += entityBehaviour.GetPosition();
        }
    }

    void OnUseAbility(MapRaycastHit hit)
    {
        if (hit.tile == null) return;
        if (!castableTiles.Contains(hit.position)) return;

        SelectionManager.Instance.OnClick -= OnUseAbility;

        Sequence attackSequence = entityBehaviour.UseAbility(entityBehaviour.GetAbilities(0), entityBehaviour.currentTile);

        attackSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnUseAbility;
        });
    }
}
