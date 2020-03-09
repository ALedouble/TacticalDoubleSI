using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleEffect", menuName = "ScriptableObjects/ObstacleEffect", order = 106)]
public class ObstacleEffect : AbilityEffect
{
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) =>
        {
            SetObstacle(castTile, castTile.position);
        });
    }

    public void SetObstacle(TileData tile, Vector2Int position)
    {
        tile.TileType = TileType.Solid;
    }
}
