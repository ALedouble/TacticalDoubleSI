using System.Collections;
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
}
