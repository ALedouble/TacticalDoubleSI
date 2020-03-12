using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatUtils
{

    public static float ComputeDamage(EntityBehaviour entity, Ability ability) {
        float damageValue = 0;
        for(int i = 0; i < entity.GetAbilities().Count; i++)
        {
            float damage = ((entity.data.power * ability.multiplicator));
            damageValue = damage;
        }

        return damageValue;
    }

    public static float ComputeHeal(EntityBehaviour entity, Ability ability)
    {
        float healValue = 0;
        for (int i = 0; i < entity.GetAbilities().Count; i++)
        {
            float heal = ((entity.data.power * ability.multiplicator));
            
            healValue = heal;
        }

        return healValue;
    }

}
