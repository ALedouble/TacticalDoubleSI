using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Editor class for feature toggling (Only one asset of this type should be in the project at any time)
/// </summary>
[CreateAssetMenu(fileName = "DevelopmentSettings", menuName = "ScriptableObjects/Singleton/DevelopmentSettings", order = 1)]
public class DevelopmentSettings : ScriptableObject
{
    public RoundMode roundMode;
}

public enum RoundMode
{
    // Player plays any of their pawns in any order, then the AI can play their pawns
    TeamTurn,
    // Enemies can play inbetween each player turn depending on the turn order
    EntityTurn,
}