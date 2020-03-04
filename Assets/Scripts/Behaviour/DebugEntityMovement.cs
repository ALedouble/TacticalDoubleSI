using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEntityMovement : MonoBehaviour
{
    public bool drawDebug;

    public EntityBehaviour entity;

    // Start is called before the first frame update
    void Start()
    {
        MapManager.GetMap().map = new List<List<TileData>>();
        MapManager.GetMap().size = 10;

        for (int x = 0; x < MapManager.GetMap().size; x++)
        {
            MapManager.GetMap().map.Add(new List<TileData>());
            for (int y = 0; y < MapManager.GetMap().size; y++)
            {
                MapManager.GetMap().map[x].Add(new TileData(TileType.Normal, 1, new Vector2Int(x, y)));
            }
        }

        SelectionManager.Instance.selectedEntity = entity;

        entity.OnTurn();
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        if (Application.isPlaying && MapManager.Instance != null)
        {
            for (int x = 0; x < MapManager.GetMap().size; x++)
            {
                for (int y = 0; y < MapManager.GetMap().size; y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), new Color(.9f, .9f, .9f, .5f));

                }
            }
        }
    }
}
