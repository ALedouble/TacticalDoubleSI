using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TemporaryObstacleBrain", menuName = "ScriptableObjects/TemporaryObstacleBrain", order = 918)]
public class TemporaryObstacleBrain : Brain
{
    public float turnNumber;

    public override void OnTurnStart(EntityBehaviour entityBehaviour)
    {
        if(turnNumber > 2)
        {

        } else
        {
            turnNumber++;
        }
    }
}
