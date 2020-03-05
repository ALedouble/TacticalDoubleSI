using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds all of the data regarding an Entity
/// </summary>



[CreateAssetMenu(fileName = "Entity", menuName = "ScriptableObjects/Entity", order = 1)]
public class Entity : ScriptableObject
{
    public string displayName;
    public int maxHealth;
    public List<Ability> abilities;
    public Brain brain;
    public int alignement;
}
