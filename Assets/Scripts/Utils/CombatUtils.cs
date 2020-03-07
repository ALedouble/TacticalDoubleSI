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
}
