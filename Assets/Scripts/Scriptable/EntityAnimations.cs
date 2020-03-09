using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityAnimations", menuName = "ScriptableObjects/EntityAnimations")]
public class EntityAnimations : ScriptableObject
{
    [SerializeField]
    private List<EntityAnimation> animations;

    public EntityAnimation GetAnimation(int i)
    {
        return animations[i];
    }
}
