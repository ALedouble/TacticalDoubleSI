using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorEffect", menuName = "ScriptableObjects/ArmorEffect", order = 103)]
public class ArmorEffect : AbilityEffect
{
    [Tooltip("Le nombre d'armure ajouté")]
    public float armorValue;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;
}
