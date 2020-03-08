using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/DamageEffect", order = 100)]
public class DamageEffect : AbilityEffect
{ 
    [Tooltip("Le multiplicateur à chaque LevelUp")]
    public float damageMultiplicator;

    [Tooltip("SFX quand l'attaque touche")]
    public AudioSource sfxHit;

    [Tooltip("VFX quand l'attaque touche")]
    public GameObject vfxHit;

  /*  public override void Activate(EntityBehaviour entity, Ability ability, TileData castTile)
    {

        //Check en fonction de la castTile,
        //Rotate
        //Check toutes les tiles de la zone d'effet
        //Dégâts
        base.Activate(abilitySequence);
    }
    */
}
