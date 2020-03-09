using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityProgression", menuName = "ScriptableObjects/EntityProgression")]
public class EntityProgression : ScriptableObject
{
    public float healthIncrement;
    public float actionPointsIncrement;
    public float armorIncrement;

    public List<EntityAnimations> animations = new List<EntityAnimations>();

    public List<AbilityProgression> abilityProgression = new List<AbilityProgression>();
}
