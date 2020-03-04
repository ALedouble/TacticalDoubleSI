using System;
using System.Collections.Generic;
using UnityEngine;

public class ReachableTile : IComparable
{
    public Vector2Int coordPosition { get; }
    public List<TileData> path { get; set;  }
    public int cost { get; }

    public ReachableTile(Vector2Int coordPosition, List<TileData> path, int cout)
    {
        this.coordPosition = coordPosition;
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
}
