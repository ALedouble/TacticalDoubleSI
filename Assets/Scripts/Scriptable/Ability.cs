using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType{ Jump, Thrust, Movement, };


[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
/// <summary>
/// Holds all of the data regarding an Ability
/// </summary>
public class Ability : ScriptableObject
{
    public AnimationType animationType;

    [Tooltip("Le nom de la capacité")]
    public string displayName;

    [Tooltip("Description de la capacité")]
    public string description;

    [Tooltip("Coût de la capacité")]
    public int cost;

    [Tooltip("Le sprite de l'UI'")]
    public Sprite displaySprite;

    [Tooltip("VFX quand on lance l'attaque")]
    public GameObject vfxCast;

    [Tooltip("SFX quand on lance l'attaque")]
    public AudioSource sfxCast;

    [Tooltip("Cooldown (si 0, retourne null)")]
    public int cooldown;

    [Tooltip("Les effets que la capacité va appeller")]
    public List<AbilityEffect> numberOfEffects;

    [Tooltip("Zone ou la capcité pourra être lancé, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea castArea;

    [Tooltip("Zone d'effet de la capacité, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea effectArea;
}
