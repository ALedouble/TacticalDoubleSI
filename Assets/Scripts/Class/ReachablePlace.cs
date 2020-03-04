using System;
using System.Collections.Generic;
using UnityEngine;

public class ReachableTiles : IComparable
{
    public Vector2Int coordPosition { get; }
    public List<TileData> path { get; }
    public int cost { get; }

    public ReachableTiles(Vector2Int coordPosition, List<TileData> path, int cout)
    {
        this.coordPosition = coordPosition;
        this.path = path;
        this.cost = cout;
    }
    public bool IsBetterThat(ReachableTiles tile)
    {
        if (this.cost < tile.cost) return true;
        return false;
    }

    public int CompareTo(object obj)
    {
        ReachableTiles tile2 = (ReachableTiles)obj;

        if (this.cost < tile2.cost) return -1;
        if (this.cost > tile2.cost) return 1;
        return 0;
    }
}
