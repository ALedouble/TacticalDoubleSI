using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the map data and provides static methods to access its contents
/// </summary>

public class MapManager : MonoBehaviour
{
    public Map map;

    private List<EntityBehaviour> listOfEntityOnTheMap;

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
        return Instance.map.GetTile(position);
    }

    public static TileData GetTile(int x, int y)
    {
        return Instance.map.GetTile(x, y);
    }

    public static int GetSize()
    {
        return Instance.map.size;
    }

    public static void SetSize(int value)
    {
        Instance.map.size = value;
    }

    public static List<TileData> GetMap()
    {
        return Instance.map.map;
    }

    public static void SetMap(List<TileData> map)
    {
        Instance.map.map = map;
    }

    public static List<EntityBehaviour> GetListOfEntity()
    {
        return Instance.listOfEntityOnTheMap;
    }
}
