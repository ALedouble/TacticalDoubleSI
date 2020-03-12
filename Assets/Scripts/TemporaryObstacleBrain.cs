using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TemporaryObstacleBrain", menuName = "ScriptableObjects/TemporaryObstacleBrain", order = 918)]
public class TemporaryObstacleBrain : Brain
{
    public float turnNumber;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        Debug.Log(turnNumber);
        if(turnNumber == 0)
        {
            MapManager.GetListOfEntity().Remove(entityBehaviour);
            MapManager.DeleteEntity(entityBehaviour);
            Destroy(entityBehaviour.gameObject);
            RoundManager.Instance.StartRound();
        } else
        {
            turnNumber++;
            Debug.Log("hello");
        }
    }
}
