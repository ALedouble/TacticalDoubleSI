using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "PlayerBrain", menuName = "ScriptableObjects/PlayerBrain", order = 999)]
public class PlayerBrain : Brain
{
    EntityBehaviour entityBehaviour;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        base.OnTurnStart(entityBehaviour);

        this.entityBehaviour = entityBehaviour;

        //SelectionManager.Instance.OnClick += OnMovement;
        SelectionManager.Instance.OnClick += OnUseAbility;
    }

    void OnMovement(MapRaycastHit hit)
    {
        if (hit.tile == null) return;

        SelectionManager.Instance.OnClick -= OnMovement;

        // TODO : Get entity position properly
        ReachableTile reachableTile = IAUtils.FindShortestPath(false, new Vector2Int((int)entityBehaviour.transform.position.x, (int)entityBehaviour.transform.position.z), hit.position, true, 999);

        if (reachableTile == null)
        {
            Debug.LogError("Entity " + entityBehaviour.name + " cannot move to " + hit.position, entityBehaviour.gameObject);
            Debug.Break();
        }

        Sequence moveSequence = entityBehaviour.MoveTo(reachableTile);

        moveSequence.OnComplete(() =>
        {
            //SelectionManager.Instance.OnClick += OnMovement;
            RoundManager.Instance.EndTurn(entityBehaviour);
        });
    }

    void OnUseAbility(MapRaycastHit hit)
    {
        if (hit.tile == null) return;

        SelectionManager.Instance.OnClick -= OnUseAbility;

        Sequence attackSequence = entityBehaviour.UseAbility(entityBehaviour.GetAbilities(0), entityBehaviour.currentTile);

        attackSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnUseAbility;
        });
    }
}
