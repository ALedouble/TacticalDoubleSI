using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SelectionUtils
{
    public static MapRaycastHit MapRaycast()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        // TODO : cache main camera for better performance
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            // Get the point at enter distance on the ray
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector2Int position = new Vector2Int(Mathf.RoundToInt(hitPoint.x), Mathf.RoundToInt(hitPoint.z));

            // TODO : implement tile fetching
            TileData tile = null;
            if (DebugMapManager.Instance != null) tile = DebugMapManager.GetTile(position);
            return new MapRaycastHit(tile, position);
        }
        else
        {
            return new MapRaycastHit(null, Vector2Int.zero);
        }
    }
}

public struct MapRaycastHit
{
    public TileData tile;
    public Vector2Int position;

    public MapRaycastHit(TileData tile, Vector2Int position)
    {
        this.tile = tile;
        this.position = position;
    }
}