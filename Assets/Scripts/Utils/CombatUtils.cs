using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatUtils
{

    public static float ComputeDamage(EntityBehaviour entity, Ability ability) {
        float damageValue = 0;
        for(int i = 0; i < entity.GetAbilities().Count; i++)
        {
            float damage = ((entity.data.power * ability.multiplicator) - 1);
            damageValue = damage;
        }

        return damageValue;
    }

    public static float ComputeHeal(EntityBehaviour entity, Ability ability)
    {
        float healValue = 0;
        for (int i = 0; i < entity.GetAbilities().Count; i++)
        {
            float heal = ((entity.data.power * ability.multiplicator));
            healValue = heal;
        }

        return healValue;
    }


    public static Vector2Int ComputeProjection(Vector2Int enemyPosition, Vector2Int myPosition)
    {
        Vector2Int pushVector = enemyPosition - myPosition;
        return myPosition + (pushVector * 2);
    }

    

    public static Sequence Grab(Vector2Int myPosition, EntityBehaviour entity, Vector2Int grabVector)
    {

        Sequence grabSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;
        for (int x = 0; x < MapManager.GetSize(); x++)
        {
            for (int y = 0; y < MapManager.GetSize(); y++)
            {
                if (MapManager.GetTile(grabVector.x, grabVector.y).tileType == TileType.Normal)
                {
                    Debug.Log(grabVector);
                  //  grabSequence.Append(entity.transform.DOMove(new Vector3(-grabVector.x, 0, -grabVector.y), 0.5f));
                }
            }
        }
        return grabSequence;
    }
}
