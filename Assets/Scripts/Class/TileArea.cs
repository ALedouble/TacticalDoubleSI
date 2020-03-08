using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileArea
{
    public int size = 3;
    public List<Vector2Int> area = new List<Vector2Int>();



    public List<Vector2Int> GetTiles()
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

    public List<Vector2Int> GetRotatedTiles(Vector2Int origin, Vector2Int target)
    {
        List<Vector2Int> tiles = new List<Vector2Int>(GetTiles());
        for (int i = 0; i < tiles.Count; i++)
        {
            if (target.x - origin.x > 0)
            {
                tiles[i] = new Vector2Int(tiles[i].y, tiles[i].x);
            }

            if (target.x - origin.x < 0)
            {
                tiles[i] = new Vector2Int(tiles[i].y * -1, tiles[i].x);
            }

            if (target.y - origin.y < 0)
            {
                tiles[i] = new Vector2Int(tiles[i].x, tiles[i].y * -1);
            }
        }

        return tiles;
    }

    public List<Vector2Int> GetWorldSpace(Vector2Int position)
    {
        List<Vector2Int> worldSpaceArea = new List<Vector2Int>(GetTiles());

        for (int i = 0; i < worldSpaceArea.Count; i++)
        {
            worldSpaceArea[i] += position;
        }

        worldSpaceArea.RemoveAll(x => !MapManager.IsInsideMap(x));

        return worldSpaceArea;
    }

    public List<Vector2Int> GetWorldSpaceRotated(Vector2Int origin, Vector2Int target)
    {
        List<Vector2Int> worldSpaceArea = new List<Vector2Int>(GetRotatedTiles(origin, target));

        for (int i = 0; i < worldSpaceArea.Count; i++)
        {
            worldSpaceArea[i] += target;
        }

        worldSpaceArea.RemoveAll(x => !MapManager.IsInsideMap(x));

        return worldSpaceArea;
    }
}

