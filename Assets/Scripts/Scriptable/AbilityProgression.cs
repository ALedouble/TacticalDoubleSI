using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityProgression", menuName = "ScriptableObjects/AbilityProgression")]
public class AbilityProgression : ScriptableObject
{
    public List<Ability> abilities = new List<Ability>();
}
