using UnityEngine;
using System;

/// <summary>
/// Handles Entity <-> World interaction
/// </summary>

[System.Serializable]
public class EntityBehaviour : MonoBehaviour
{
    public Entity data;

    int currentHealth;
    public int CurrentHealth { get => currentHealth; }

    int currentActionPoints;
    public int CurrentActionPoints { get => currentActionPoints; }

    int currentArmor;
    public int CurrentArmor { get => currentArmor; }
}
