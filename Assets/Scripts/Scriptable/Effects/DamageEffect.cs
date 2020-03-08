using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/DamageEffect", order = 100)]
public class DamageEffect : AbilityEffect
{ 
    [Tooltip("Le multiplicateur à chaque LevelUp")]
    public float damageMultiplicator;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;

    public float damageCalcul(Entity data, float damage)
    {
        float totalDamage = ((data.power * damageMultiplicator) - 1);
        return totalDamage;
    }
}
