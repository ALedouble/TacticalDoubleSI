using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Handles entity and navigation data
/// </summary>

public enum TileType { Normal, Slow, Fast, Solid, ClickStart, ClickEnd }

[System.Serializable]
public class TileData
{
    TileType typeOfTile;
    int cost;

    public TileData(TileType type, int cost)
    {
        this.typeOfTile = type;
        this.cost = cost;
    }

    public bool IsWalkable
    {
        get
        {
            // TODO : Return a bool that is a compound of multiple parameters, such as if one of the entities on this tile is solid or if the tile is marked as solid
            if (this.typeOfTile != TileType.Solid) return true;
            return false;
        }
    }
    public int Cost
    {
        get
        {
            // TODO : Return the cost of the tile
            return this.cost;
        }
    }
    public TileType TileType
    {
        get
        {
            return this.typeOfTile;
        }

        set
        {
            this.typeOfTile = value;
        }
    }

    public List<EntityBehaviour> entities;
}
