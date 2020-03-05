using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "PlayerBrain", menuName = "ScriptableObjects/PlayerBrain", order = 999)]
public class PlayerBrain : Brain
{
    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        base.OnTurnStart(entityBehaviour);



       // SelectionManager.Instance.OnClick += OnMovement;
        SelectionManager.Instance.OnClick += OnUseAbility;

    }

    void OnMovement(MapRaycastHit hit)
    {
        if (hit.tile == null) return;

        SelectionManager.Instance.OnClick -= OnMovement;

        EntityBehaviour selectedEntity = SelectionManager.Instance.selectedEntity;

        // TODO : Get entity position properly
        ReachableTile reachableTile = IAUtils.FindShortestPath(new Vector2Int((int)selectedEntity.transform.position.x, (int)selectedEntity.transform.position.z), MapManager.GetMap().map, hit.position, 999);

        Sequence moveSequence = selectedEntity.MoveTo(reachableTile);

        moveSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnMovement;
        });
    }

    void OnUseAbility(MapRaycastHit hit)
    {
      /*  Debug.Log("OnUseAbility");
        if (hit.tile == null) return;

        SelectionManager.Instance.OnClick -= OnUseAbility;

        EntityBehaviour selectedEntity = SelectionManager.Instance.selectedEntity;

        //Sequence attackSequence = selectedEntity.UseAbility();

        attackSequence.OnComplete(() =>
        {
            SelectionManager.Instance.OnClick += OnUseAbility;
        });
        */
    }
}
