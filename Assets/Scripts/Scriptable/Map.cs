using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the positions of all entities at the start of the level
/// </summary>
public struct EntityPosition
{
    Entity entity;
    Vector2 position;
}

/// <summary>
/// Editor and Runtime class containing all map data
/// </summary>
[CreateAssetMenu(fileName = "Map", menuName = "ScriptableObjects/Map", order = 1)]
public class Map : ScriptableObject
{
    public List<List<TileData>> map = new List<List<TileData>>();

    public List<EntityPosition> entityStartPositions;

    public int size;

}
