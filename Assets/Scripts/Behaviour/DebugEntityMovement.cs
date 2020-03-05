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
        MapManager.Instance.map.map = new List<List<TileData>>();
        MapManager.Instance.map.size = 10;

        for (int x = 0; x < MapManager.Instance.map.size; x++)
        {
            MapManager.Instance.map.map.Add(new List<TileData>());
            for (int y = 0; y < MapManager.Instance.map.size; y++)
            {
                MapManager.Instance.map.map[x].Add(new TileData(TileType.Normal, new Vector2Int(x, y)));
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
            for (int x = 0; x < MapManager.Instance.map.size; x++)
            {
                for (int y = 0; y < MapManager.Instance.map.size; y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), new Color(.9f, .9f, .9f, .5f));

                }
            }
        }
    }
}
