using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "PushEffect", menuName = "ScriptableObjects/PushEffect", order = 101)]
public class PushEffect : AbilityEffect
{
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) => {
            Push(x, CombatUtils.ComputeProjection(x.GetPosition(), entity.GetPosition()));
        });
    }

    public Sequence Push(EntityBehaviour entity, Vector2Int pushVector)
    {
        Sequence pushSequence = DOTween.Sequence();
        Ease pushEase = Ease.InQuad;
        Debug.Log(pushVector);
        if (MapManager.IsInsideMap(pushVector) && MapManager.GetTile(pushVector).tileType != TileType.Solid) //&& entity.data.alignement != Alignement.Player)
        {
            pushSequence.Append(entity.transform.DOMove(new Vector3(pushVector.x, 0, pushVector.y), 0.5f));
            entity.currentTile = MapManager.MoveEntity(entity, entity.GetPosition(), pushVector);
        }
       
        return pushSequence;
    }
}
