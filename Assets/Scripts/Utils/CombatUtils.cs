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
        for(int x = 0; x < MapManager.GetSize(); x++)
        {
            for (int y = 0; y < MapManager.GetSize(); y++)
            {
                Debug.Log(MapManager.GetTile(x,y));
                Debug.Log(pushVector);
                if (MapManager.GetTile(pushVector.x ,pushVector.y).tileType == TileType.Normal)
                {
                    Debug.Log(entity.currentTile.position);
                    pushSequence.Append(entity.transform.DOMove(new Vector3(pushVector.x, 0, pushVector.y), 0.5f));
                    Debug.Log(entity.currentTile.position);
                    
                }
            }
        }
        return pushSequence;
    }
}
