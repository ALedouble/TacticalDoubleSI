using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/HealEffect", order = 105)]
public class HealEffect : AbilityEffect
{
    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;

    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) => {
            if(x.data.alignement == entity.data.alignement)
            {
                x.CurrentHealth += SetHeal(entity, ability);
            }
        });
    }

    public float SetHeal(EntityBehaviour entity, Ability ability)
    {
        return CombatUtils.ComputeHeal(entity, ability);
    }
}
