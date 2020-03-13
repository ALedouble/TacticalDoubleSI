using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TemporaryObstacleBrain", menuName = "ScriptableObjects/TemporaryObstacleBrain", order = 918)]
public class TemporaryObstacleBrain : Brain
{
    private float turnNumber = -1;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        Debug.Log(turnNumber);
        turnNumber++;

        if (turnNumber ==  0)
        {
            RoundManager.Instance.EndTurn();
            MapManager.DeleteEntity(entityBehaviour);
            Destroy(entityBehaviour.gameObject);
            turnNumber = -1;
        } 
    }
}
