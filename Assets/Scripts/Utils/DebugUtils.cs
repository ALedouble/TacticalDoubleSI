using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DebugUtils
{
    const float debugTileHeight = 0.1f;
    const float debugTileSpacing = 0.1f;

    public static void DrawTile(Vector2Int position)
    {
        DrawTile(position, new Color(.9f, .9f, .9f, .5f));
    }

    public static void DrawTile(Vector2Int position, Color color)
    {
        Color originalColor = Gizmos.color;
        Gizmos.color = color;
        Gizmos.DrawCube(new Vector3(position.x, 0, position.y) + (Vector3.down * debugTileHeight * 0.5f), new Vector3(1 - debugTileSpacing, debugTileHeight, 1 - debugTileSpacing));
        Gizmos.color = originalColor;
    }

#if UNITY_EDITOR
    public static void DrawTileEditor(Vector2Int position, Color color)
    {
        Vector3 pos = new Vector3(position.x, 0, position.y);

        float size = (-debugTileSpacing * .5f) + .5f;

        Vector3[] verts = new Vector3[]
        {
            new Vector3(pos.x - size, pos.y, pos.z - size),
            new Vector3(pos.x - size, pos.y, pos.z + size),
            new Vector3(pos.x + size, pos.y, pos.z + size),
            new Vector3(pos.x + size, pos.y, pos.z - size)
        };

        Handles.DrawSolidRectangleWithOutline(verts, color, new Color(0, 0, 0, 1));
    }
#endif
}
