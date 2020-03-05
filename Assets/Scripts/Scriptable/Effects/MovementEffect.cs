using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementEffect", menuName = "ScriptableObjects/MovementEffect", order = 104)]
public class MovementEffect : AbilityEffect
{
    [Tooltip("Nombre de tour avant que l'obstacle soit détruit")]
    public float timeBeforeDestruction;

    [Tooltip("Si oui, ma ligne de vue sera bloqué")]
    public bool blockMyself;

    [Tooltip("Zone ou la capcité pourra être lancé, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea castArea;

    [Tooltip("Zone d'effet de la capacité, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea effectArea;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;
}
