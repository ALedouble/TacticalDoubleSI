using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEntityAttack : MonoBehaviour
{

    public bool drawDebug;

    public EntityBehaviour entity;
    // Start is called before the first frame update
    void Start()
    {
        MapManager.SetMap(new List<TileData>());
        MapManager.SetSize(10);

        for (int x = 0; x < MapManager.GetSize(); x++)
        {
            for (int y = 0; y < MapManager.GetSize(); y++)
            {
                MapManager.GetMap().Add(new TileData(TileType.Normal, new Vector2Int(x, y)));
            }
        }

        entity.OnTurn();
    }

    // Update is called once per frame
    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        if (Application.isPlaying && MapManager.Instance != null)
        {
            for (int x = 0; x < MapManager.GetSize(); x++)
            {
                for (int y = 0; y < MapManager.GetSize(); y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), new Color(.9f, .9f, .9f, .5f));
                }
            }
        }
    }
}
