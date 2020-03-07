using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileArea
{
    public int size = 3;
    public List<Vector2Int> area = new List<Vector2Int>();

    public List<Vector2Int> RelativeArea()
    {
        List<Vector2Int> goodArea = new List<Vector2Int>();

        int precision = size / 2;
        Vector2Int decalage = new Vector2Int(precision, precision);

        for (int i = 0; i < area.Count; i++)
        {
            Vector2Int position = area[i] - decalage;
            position.y *= -1;

            goodArea.Add(position);
        }

        return goodArea;
    }
}

