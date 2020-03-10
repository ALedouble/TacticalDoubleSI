﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum AnimationType{ Jump, Thrust, Movement, Grab, Push};


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

    [Tooltip("Le multiplicateur à chaque LevelUp")]
    public float multiplicator;

    [Tooltip("L'expérience à chaque fois qu'on utilise l'effet")]
    public float experience;

    [Tooltip("Le sprite de l'UI'")]
    public Sprite displaySprite;

    [Tooltip("VFX quand on lance l'attaque")]
    public GameObject vfxCast;

    [Tooltip("SFX quand on lance l'attaque")]
    public AudioSource sfxCast;

    [Tooltip("Est-ce possible de se le lancer dessus")]
    public bool canCastOnEntityPosition;



    [Tooltip("Cooldown (si 0, retourne null)")]
    public int cooldown;

    [Tooltip("Les effets que la capacité va appeller")]
    public List<AbilityEffect> abilityEffect;

    [Tooltip("Zone ou la capcité pourra être lancé, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea castArea;

    [Tooltip("Zone d'effet de la capacité, à définir par sélection des tiles (gris foncé = notre position)")]
    public TileArea effectArea;


    public Tween GetStartTween(Transform transform, Vector2Int castableTiles)
    {
        switch (animationType)
        {
            case AnimationType.Jump:
                Ease jumpStartEase = Ease.InExpo;
                return transform.DOMove(new Vector3(transform.position.x, 0f, transform.position.z), 0.25f);
                break;
            case AnimationType.Thrust:
                return transform.DOMove(new Vector3(transform.position.x + (castableTiles.x * 0.1f), 0, transform.position.z), .25f);
                break;
            case AnimationType.Movement:
                return transform.DOMove(new Vector3(transform.position.x + 0.5f, 0, transform.position.z), .25f);
                break;
            case AnimationType.Grab:
                return transform.DOMove(new Vector3(transform.position.x + 0.5f, 0, transform.position.z), .25f);
                break;
            case AnimationType.Push:
                return transform.DOMove(new Vector3(transform.position.x + 0.5f, 0, transform.position.z), .25f);
            default:
                break;
        }

        return null;
    }

    public Tween GetEndTween(Transform transform)
    {
        switch (animationType)
        {
            case AnimationType.Jump:
                Ease jumpEndEase = Ease.OutExpo;
                return transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f);
                break;
            case AnimationType.Thrust:
                return transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f);
                break;
            case AnimationType.Movement:
                return transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f);
                break;
            case AnimationType.Grab:
                return transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f);
                break;
            case AnimationType.Push:
                return transform.DOMove(new Vector3(transform.position.x, 0, transform.position.z), .25f);
            default:
                break;
        }

        return null;
    }
}
