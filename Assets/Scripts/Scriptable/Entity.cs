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
    public SoundReference walkSFX;
    public SoundReference hitBySomeoneSFX;
    public SoundReference deathSFX;
    public Vector2 pivot = new Vector2(0.5f, 0.5f);

    [Space]

    public float maxHealth;
    public float maxActionPoints;
    public float armor;
    public List<Ability> abilities;
    public List<int> abilityLevels;
    public Brain brain;
    public Alignement alignement;
    public EntityTag entityTag;
    public float power = 1;

    public float xpPoint = 1;

    [Space]

    public int totemValue = -1;
    public bool isNotDestructible = false;
    public bool isFx = false;
    public GameObject fxEntity;
    public Color color;

    public int GetAbilityNumber(Ability ability)
    {
        return abilities.FindIndex(x => x == ability);
    }
}
