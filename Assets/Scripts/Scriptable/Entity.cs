using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds all of the data regarding an Entity
/// </summary>

public enum Alignement { Enemy, Player, Neutral }
public enum EntityTag { Healer, DPS, Tank, Minion, Totem, Trap, None }

[CreateAssetMenu(fileName = "Entity", menuName = "ScriptableObjects/Entity", order = 1)]
public class Entity : ScriptableObject
{
    public string displayName;
    public Sprite portrait;
    public EntityAnimations animations;

    [Space]

    public float maxHealth;
    public float maxActionPoints;
    public float armor;
    public List<Ability> abilities;
    public Brain brain;
    public Alignement alignement;
    public EntityTag entityTag;
    public float power = 1;

    [Space]

    public int totemValue = -1;

    public int GetAbilityNumber(Ability ability)
    {
        return abilities.FindIndex(x => x == ability);
    }
}
