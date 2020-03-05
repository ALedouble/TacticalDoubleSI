using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/HealEffect", order = 105)]
public class HealEffect : AbilityEffect
{
    [Tooltip("Nombre de pv que la capacité va rendre")]
    public float healValue;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;
}
