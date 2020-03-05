using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the map data and provides static methods to access its contents
/// </summary>

public class MapManager : MonoBehaviour
{
    public Map map;

    // Static fields should be in CamelCase
    public static MapManager Instance;

    private void Awake()
    {
        // No need to check if the instance is null because the MapManager will always be destroyed at the end of the level
        Instance = this;

        // Create a copy of map since ScriptableObjects are persistent and we don't want to change the map in the project while in the editor
        map = Instantiate(map);
    }

    public static TileData GetTile(Vector2Int position)
    {
        if (position.x >= 0 && position.x < GetSize() && position.y >= 0 && position.y < GetSize())
            return Instance.map.map[position.x][position.y];

        return null;
    }

    public static int GetSize()
    {
        return Instance.map.size;
    }

    public static List<List<TileData>> GetMap()
    {
        return Instance.map.map;
    }
}
