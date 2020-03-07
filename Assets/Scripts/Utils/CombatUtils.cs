using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatUtils
{
    public static float damageCalcul(Entity data, float damage)
    {
        damage = ((data.power * 0.5f) - 1);
        return damage;
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

        pushSequence.Append(entity.transform.DOMove(new Vector3(pushVector.x, 0, pushVector.y), 0.5f));
        return pushSequence;
    }
}
