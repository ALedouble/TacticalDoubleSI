using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrabEffect", menuName = "ScriptableObjects/GrabEffect", order = 102)]
public class GrabEffect : AbilityEffect
{
    [Tooltip("Le nombre de case que le grab va effectuer")]
    public float grabValue;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;
}
