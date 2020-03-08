using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatUtils
{
    static AbilityEffect currentEffect;
    static DamageEffect damageEffect;
    /*
    public static AbilityEffect GetEffect(EntityBehaviour entity)
{
            for (int y = 0; y < entity.data.abilities[0].numberOfEffects.Count; y++) {
                currentEffect = entity.data.abilities[].numberOfEffects[y];

                if (currentEffect.GetType() == typeof(DamageEffect))
                {
                    damageEffect = (DamageEffect)currentEffect;
                    SetDamage(entity);
                }

                if (currentEffect.GetType() == typeof(PushEffect))
                {
                    damageEffect = (DamageEffect)currentEffect;
                    Vector2Int pushVector = CombatUtils.PushEffect(enemyPosition, currentTile.position);
                    Push(currentEnemy, pushVector);
                }


            }
         }
        return currentEffect;
    }

    public static float SetDamage(EntityBehaviour entity) {
        float damageValue = 0;
        for(int i = 0; i < entity.GetAbilities().Count; i++)
        {
            float damage = ((entity.data.power * damageEffect.damageMultiplicator) - 1);
            damageValue = damage;
        }
        Debug.Log(damageValue);

        return damageValue;
    }


    public static Vector2Int PushEffect(Vector2Int enemyPosition, Vector2Int myPosition)
    {
        Vector2Int pushVector = enemyPosition - myPosition;
        return myPosition + (pushVector* 2);
    }

    public static Sequence Push(EntityBehaviour entity, Vector2Int pushVector)
    {
        
        Sequence pushSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;
        for(int x = 0; x < MapManager.GetSize(); x++)
        {
            for (int y = 0; y < MapManager.GetSize(); y++)
            {
                if (MapManager.GetTile(pushVector.x ,pushVector.y).tileType == TileType.Normal)
                {
                    pushSequence.Append(entity.transform.DOMove(new Vector3(pushVector.x, 0, pushVector.y), 0.5f));
                }
            }
        }
        return pushSequence;
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

    */
}
