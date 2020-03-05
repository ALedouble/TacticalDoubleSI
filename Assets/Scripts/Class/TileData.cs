using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles entity and navigation data
/// </summary>

public enum TileType { Solid = 0, Fast = 1, Normal = 2, Slow = 3 }

[System.Serializable]
public class TileData
{
    public List<EntityBehaviour> entities;
    public Vector2Int position;
    public TileType tileType;

    // ********* Debug **********
    public Color color;

    public Vector2Int Position { get => position; set => position = value; }


    public TileData(TileType type)
    {
        this.tileType = type;
    }

    public TileData(TileType type, Vector2Int position)
    {
        this.tileType = type;
        this.Position = position;
    }

    public bool IsWalkable
    {
        get
        {
            // TODO : Return a bool that is a compound of multiple parameters, such as if one of the entities on this tile is solid or if the tile is marked as solid
            if (this.tileType != TileType.Solid) return true;
            return false;
        }
    }

    public TileType TileType
    {
        get
        {
            return tileType;
        }
        set
        {
            tileType = value;
        }
    }
}
