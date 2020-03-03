using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Handles entity and navigation data
/// </summary>
public class TileData
{
    public bool IsWalkable
    {
        get
        {
            // TODO : Return a bool that is a compound of multiple parameters, such as if one of the entities on this tile is solid or if the tile is marked as solid
            throw new NotImplementedException();
        }
    }

    public List<EntityBehaviour> entities;
}
