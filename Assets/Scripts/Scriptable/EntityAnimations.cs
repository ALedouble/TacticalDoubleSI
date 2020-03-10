using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityAnimations", menuName = "ScriptableObjects/EntityAnimations")]
public class EntityAnimations : ScriptableObject
{

    [SerializeField]
    public EntityAnimation idleAnimation;
    [SerializeField]
    public EntityAnimation moveAnimation;
    [SerializeField]
    public EntityAnimation hitAnimation;
    [SerializeField]
    public EntityAnimation deathAnimation;

    [SerializeField]
    private List<EntityAnimation> abilityAnimations;

    public EntityAnimation GetAbilityAnimation(int abilityNumber)
    {
        return abilityAnimations[abilityNumber];
    }
}
