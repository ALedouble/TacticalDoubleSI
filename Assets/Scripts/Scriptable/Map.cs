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
    public List<TileData> map = new List<TileData>();

    public List<EntityPosition> entityStartPositions;

    public int size;

    public void Init()
    {
        if (size == map.Count) return;

        Debug.Log("Initializing map");

        map = new List<TileData>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map.Add(new TileData(TileType.Normal));
            }
        }
    }

    public TileData GetTile(Vector2Int position)
    {
        if (position.x >= 0 && position.x < size && position.y >= 0 && position.y < size)
            return map[position.x * size + position.y];

        return null;
    }

    public TileData GetTile(int x, int y)
    {
        if (x >= 0 && x < size && y >= 0 && y < size)
            return map[x * size + y];

        return null;
    }
}
