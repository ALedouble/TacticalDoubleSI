using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "AbilityEffect", menuName = "ScriptableObjects/AbilityEffect", order = 2)]
/// <summary>
/// For each type of ability (damage or heal or push etc..)
/// </summary>
public class AbilityEffect : ScriptableObject
{
    public float duration;

    public virtual void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
    }

    public virtual void ApplyEffect(EntityBehaviour entity, Ability ability, TileData castTile, Action<EntityBehaviour> effect)
    {
        List<Vector2Int> effectTiles = ability.effectArea.GetWorldSpaceRotated(entity.GetPosition(), castTile.position);
        for (int i = 0; i < effectTiles.Count; i++)
        {
            List<EntityBehaviour> entities = MapManager.GetTile(effectTiles[i]).entities;

            for (int j = 0; j < entities.Count; j++)
            {
                effect.Invoke(entities[j]);
            }
        }
    }
}
