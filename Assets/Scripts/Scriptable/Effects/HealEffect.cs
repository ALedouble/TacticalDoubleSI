using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/HealEffect", order = 105)]
public class HealEffect : AbilityEffect
{
    [Tooltip("Nombre de pv que la capacité va rendre")]
    public float healValue;

    [Tooltip("Zone ou la capcité pourra être lancé, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea castArea;

    [Tooltip("Zone d'effet de la capacité, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea effectArea;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;
}
