using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Holds the map data and provides static methods to access its contents
/// </summary>

public class MapManager : MonoBehaviour
{
    public Map map;

    private List<EntityBehaviour> listOfEntityOnTheMap = new List<EntityBehaviour>();

    public static MapManager Instance;

    public List<ReachableTile> reachableTiles = new List<ReachableTile>();
    public Action<List<ReachableTile>> OnReachableTilesChanged;
    public static void SetReachableTilesPreview(List<ReachableTile> tiles)
    {
        Instance.reachableTiles = tiles;
        Instance.OnReachableTilesChanged?.Invoke(Instance.reachableTiles);
    }

    public List<Vector2Int> castableTiles = new List<Vector2Int>();

    public Action<List<Vector2Int>> OnCastableTilesChanged;

    public static void SetCastableTilesPreview(List<Vector2Int> tiles)
    {
        Instance.castableTiles = tiles;
        Instance.OnCastableTilesChanged?.Invoke(Instance.castableTiles);
    }

    public List<Vector2Int> effectTiles = new List<Vector2Int>();

    public Action<List<Vector2Int>> OnEffectTilesChanged;
    public static void SetEffectTilesPreview(List<Vector2Int> tiles)
    {
        Instance.effectTiles = tiles;
        Instance.OnEffectTilesChanged?.Invoke(Instance.effectTiles);
    }

    public GameObject entityPrefab;

    private void Awake()
    {
        entityPrefab = Resources.Load("Entity") as GameObject;

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
        for (int i = 0; i < map.entityStartPositions.Count; i++)
        {
            RoundManager.Instance.roundEntities.Add(SpawnEntity(map.entityStartPositions[i].entity, map.entityStartPositions[i].position, map.entityStartPositions[i].heldCrystalValue));
        }
    }

    public static EntityBehaviour SpawnEntity(Entity entity, Vector2 position, int heldCrystalValue)
    {
        EntityBehaviour entityBehaviour = Instantiate(MapManager.Instance.entityPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity).GetComponent<EntityBehaviour>();

        entityBehaviour.data = entity;
        entityBehaviour.heldCrystalValue = heldCrystalValue;
        entityBehaviour.currentTile = MapManager.GetTile(new Vector2Int((int)position.x, (int)position.y));
        entityBehaviour.currentTile.entities.Add(entityBehaviour);

        MapManager.GetListOfEntity().Add(entityBehaviour);
        entityBehaviour.Init();

        return entityBehaviour;
    }

    public static void DeleteEntity(EntityBehaviour entity)
    {
        MapManager.GetListOfEntity().Remove(entity);

        entity.currentTile.entities.Remove(entity);

        switch (entity.data.alignement)
        {
            case Alignement.Enemy:

                RoundManager.Instance.roundEntities.Remove(entity);

                break;
            case Alignement.Player:

                PlayerTeamManager.Instance.playerEntitybehaviours.Remove(entity);

                break;
            case Alignement.Neutral:

                RoundManager.Instance.roundEntities.Remove(entity);

                break;
            default:
                break;
        }
    }

    public static Vector2 GetCenter()
    {
        return Instance.map.center;
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

        Debug.Log("moving " + entity.name + " from " + origin + " to " + target);


        GetTile(origin).entities.Remove(entity);

        GetTile(target).entities.Add(entity);

        return GetTile(target);
    }

    public static List<EntityBehaviour> GetListOfEntity()
    {
        return Instance.listOfEntityOnTheMap;
    }
}
