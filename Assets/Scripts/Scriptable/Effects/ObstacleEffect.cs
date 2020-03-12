using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleEffect", menuName = "ScriptableObjects/ObstacleEffect", order = 106)]
public class ObstacleEffect : AbilityEffect
{
    public Entity spawnedEntity;
    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        List<Vector2Int> effectTiles = ability.effectArea.GetWorldSpaceRotated(entity.GetPosition(), castTile.position);
        for (int i = 0; i < effectTiles.Count; i++)
        {
            if (MapManager.GetTile(effectTiles[i]).IsWalkable)
            {
                RoundManager.Instance.roundEntities.Add(MapManager.SpawnEntity(spawnedEntity, effectTiles[i], -1));
            }
        } 
    }
}
