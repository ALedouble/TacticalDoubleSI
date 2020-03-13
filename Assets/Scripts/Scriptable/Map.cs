using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Contains the positions of all entities at the start of the level
/// </summary>
[System.Serializable]
public class EntityRoundStartState
{
    public Entity entity;
    public int heldCrystalValue = -1;
    public Vector2 position;

    public EntityRoundStartState(Entity entity, int heldCrystalValue, Vector2 position)
    {
        this.entity = entity;
        this.heldCrystalValue = heldCrystalValue;
        this.position = position;
    }
}

/// <summary>
/// Editor and Runtime class containing all map data
/// </summary>
[CreateAssetMenu(fileName = "Map", menuName = "ScriptableObjects/Map", order = 1)]
public class Map : ScriptableObject
{
    public List<TileData> map = new List<TileData>();

    [HideInInspector] public List<EntityRoundStartState> entityStartPositions = new List<EntityRoundStartState>();

    public int size;
    public Vector2 center;

    public void Init()
    {
        if (size*size == map.Count) return;

        map = new List<TileData>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map.Add(new TileData(TileType.Normal, x, y));
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
