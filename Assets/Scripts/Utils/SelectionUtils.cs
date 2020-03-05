using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SelectionUtils
{
    public static MapRaycastHit MapRaycast()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            // Get the point at enter distance on the ray
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector2Int position = PositionToMapCoords(hitPoint);

            TileData tile = null;
            if (MapManager.Instance != null) tile = MapManager.GetTile(position);
            return new MapRaycastHit(tile, position);
        }
        else
        {
            return new MapRaycastHit(null, Vector2Int.zero);
        }
    }

#if UNITY_EDITOR
    public static MapRaycastHit MapRaycastEditor(Map map, Event currentEvent)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            // Get the point at enter distance on the ray
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector2Int position = PositionToMapCoords(hitPoint);

            TileData tile = map.GetTile(position);
            return new MapRaycastHit(tile, position);
        }
        else
        {
            return new MapRaycastHit(null, Vector2Int.zero);
        }
    }
#endif

    public static Vector2Int PositionToMapCoords(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
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