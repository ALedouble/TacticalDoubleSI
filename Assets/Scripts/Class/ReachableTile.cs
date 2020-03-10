using System;
using System.Collections.Generic;
using UnityEngine;

public class ReachableTile : IComparable
{
    public TileData castTile { get; set; }
    public List<TileData> path { get; set;  }
    public int cost { get; set; }

    public ReachableTile(List<TileData> path, int cout)
    {
        this.path = path;
        this.cost = cout;
    }
    public bool IsBetterThat(ReachableTile tile)
    {
        if (this.cost < tile.cost) return true;
        return false;
    }

    public int CompareTo(object obj)
    {
        ReachableTile tile2 = (ReachableTile)obj;

        if (this.cost < tile2.cost) return -1;
        if (this.cost > tile2.cost) return 1;
        return 0;
    }

    public Vector2Int GetCoordPosition()
    {
        return path[path.Count - 1].position;
    }

    public TileData GetLastTile()
    {
        return path[path.Count - 1];
    }
}
