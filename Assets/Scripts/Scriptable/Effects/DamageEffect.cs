﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/DamageEffect", order = 100)]
public class DamageEffect : AbilityEffect
{


    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;

    public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {
        ApplyEffect(entity, ability, castTile, (x) => {
            if (x.data.alignement != entity.data.alignement)
            {
                x.CurrentHealth -= SetDamage(entity, ability) - x.CurrentArmor;
            }
        });

        
    }

    public float SetDamage(EntityBehaviour entity, Ability ability)
    {
        return CombatUtils.ComputeDamage(entity, ability);
    }
}
