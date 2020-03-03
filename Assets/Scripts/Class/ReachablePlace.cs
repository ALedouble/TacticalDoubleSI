using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachablePlace : IComparer
{
    public Vector2 coordPosition { get; }
    public List<TileData> path { get; }
    public int cout { get; }

    public ReachablePlace(Vector2 coordPosition, List<TileData> path, int cout)
    {
        this.coordPosition = coordPosition;
        this.path = path;
        this.cout = cout;
    }

    public int Compare(object x, object y)
    {
        ReachablePlace tile1 = (ReachablePlace)x;
        ReachablePlace tile2 = (ReachablePlace)y;

        if (tile1.cout < tile2.cout) return -1;
        if (tile1.cout > tile2.cout) return 1;
        return 0;
    }
    public bool IsBetterThat(ReachablePlace tile)
    {
        if (this.cout < tile.cout) return true;
        return false;
    }
}
