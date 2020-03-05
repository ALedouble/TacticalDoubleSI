using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrabEffect", menuName = "ScriptableObjects/GrabEffect", order = 102)]
public class GrabEffect : AbilityEffect
{
    [Tooltip("Le nombre de case que le grab va effectuer")]
    public float grabValue;

    [Tooltip("Zone ou la capcité pourra être lancé, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea castArea;

    [Tooltip("Zone d'effet de la capacité, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea effectArea;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;
}
