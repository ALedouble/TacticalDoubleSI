using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds all of the data regarding an Entity
/// </summary>

public enum Alignement { Enemy, Player, Neutral }
public enum EntityTag { Healer, DPS, Tank, Minion, Totem, Trap }

[CreateAssetMenu(fileName = "Entity", menuName = "ScriptableObjects/Entity", order = 1)]
public class Entity : ScriptableObject
{
    public string displayName;

    [Space]

    public int maxHealth;
    public List<Ability> abilities;
    public Brain brain;
    public Alignement alignement;
    public EntityTag entityTag;

    [Space]

    public int totemValue = -1;
}
