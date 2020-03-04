using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
/// <summary>
/// Holds all of the data regarding an Ability
/// </summary>
public class Ability : ScriptableObject
{
    [Tooltip("Le nom de la capacité")]
    public string displayName;

    [Tooltip("Description de la capacité")]
    public string description;

    [Tooltip("Coût de la capacité")]
    public int cost;

    [Tooltip("Le nom de la capacité")]
    public float damage;

    [Tooltip("Le nom de la capacité")]
    public Sprite displaySprite;

    [Tooltip("Le nom de la capacité")]
    public GameObject vfxCast;

    [Tooltip("Le nom de la capacité")]
    public GameObject vfxHit;

    [Tooltip("Le nom de la capacité")]
    public AudioSource sfxCast;

    [Tooltip("Le nom de la capacité")]
    public AudioSource sfxHit;

    [Tooltip("Le nom de la capacité")]
    public int cooldown;

    [Tooltip("Le nom de la capacité")]
    public TileArea castArea;

    [Tooltip("Le nom de la capacité")]
    public TileArea effectArea;
}
