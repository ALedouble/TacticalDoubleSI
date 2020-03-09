using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the map data and provides static methods to access its contents
/// </summary>

public class MapManager : MonoBehaviour
{
    public Map map;

    private List<EntityBehaviour> listOfEntityOnTheMap = new List<EntityBehaviour>();

    // Static fields should be in CamelCase
    public static MapManager Instance;

    // TEMPORARY
    public List<ReachableTile> reachableTiles = new List<ReachableTile>();

    public List<Vector2Int> castableTiles = new List<Vector2Int>();

    public List<Vector2Int> effectTiles = new List<Vector2Int>();

    private void Awake()
    {
        // No need to check if the instance is null because the MapManager will always be destroyed at the end of the level
        Instance = this;

        // Create a copy of map since ScriptableObjects are persistent and we don't want to change the map in the project while in the editor
        map = Instantiate(map);
    }

    public void Init()
    {
        InstantiateEntities();
    }

    void InstantiateEntities()
    {
        GameObject entityPrefab = Resources.Load("Entity") as GameObject;

        EntityBehaviour entityBehaviour;
        for (int i = 0; i < map.entityStartPositions.Count; i++)
        {
            
            entityBehaviour = Instantiate(entityPrefab, new Vector3(map.entityStartPositions[i].position.x, 0, map.entityStartPositions[i].position.y), Quaternion.identity).GetComponent<EntityBehaviour>();
            
            entityBehaviour.data = map.entityStartPositions[i].entity;
            entityBehaviour.heldCrystalValue = map.entityStartPositions[i].heldCrystalValue;
            entityBehaviour.currentTile = MapManager.GetTile(new Vector2Int((int)map.entityStartPositions[i].position.x, (int)map.entityStartPositions[i].position.y));
            entityBehaviour.currentTile.entities.Add(entityBehaviour);

            listOfEntityOnTheMap.Add(entityBehaviour);
            entityBehaviour.Init();
            RoundManager.Instance.roundEntities.Add(entityBehaviour);
        }
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

    public static bool IsInsideMap(Vector2Int position)
    {
        return (position.x >= 0 && position.y >= 0 && position.x < MapManager.GetSize() && position.y < MapManager.GetSize());
    }

    /// <summary>
    /// Returns the target tile
    /// </summary>
    public static TileData MoveEntity(EntityBehaviour entity, Vector2Int origin, ReachableTile target)
    {
        if (target.path.Count <= 0) return GetTile(origin);
        return MoveEntity(entity, origin, target.GetCoordPosition());
    }
    public static TileData MoveEntity(EntityBehaviour entity, Vector2Int origin, Vector2Int target)
    {
        GetTile(origin).entities.Remove(entity);

        GetTile(target).entities.Add(entity);

        return GetTile(target);
    }

    public static List<EntityBehaviour> GetListOfEntity()
    {
        return Instance.listOfEntityOnTheMap;
    }
}
