using System.Collections;
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

    [HideInInspector]
    Vector3 posBeforeAnim;


    public void MovementAnimations(EntityBehaviour entity, TileData targetTile)
    {
        posBeforeAnim = new Vector3(entity.GetPosition().x, 0, entity.GetPosition().y);

        switch (animationType)
        {
            case AnimationType.Jump:
                entity.transform.position = new Vector3(targetTile.position.x, 0, targetTile.position.y);
                break;
            case AnimationType.Thrust:
                entity.transform.position = new Vector3(targetTile.position.x, 0, targetTile.position.y);
                break;
            case AnimationType.Movement:
                break;
            case AnimationType.Grab:
                break;
            case AnimationType.Push:
                break;
            default:
                break;
        }
    }

    public void ReturnPositions(EntityBehaviour entity, TileData targetTile)
    {
        switch (animationType)
        {
            case AnimationType.Jump:
                break;
            case AnimationType.Thrust:
                entity.transform.position = posBeforeAnim;
                break;
            case AnimationType.Movement:
                break;
            case AnimationType.Grab:
                break;
            case AnimationType.Push:
                break;
            default:
                break;
        }
    }
}
