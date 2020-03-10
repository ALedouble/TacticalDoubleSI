using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public GameObject castAreaTilePrefab;
    public GameObject effectAreaTilePrefab;
    public GameObject HoverTilePrefab;
    public GameObject slowTilePrefab;
    public GameObject speedTilePrefab;

    private void Start()
    {
        List<TileData> tiles = MapManager.GetMap();
        for (int i = 0; i < tiles.Count; i++)
        {
            switch (tiles[i].TileType)
            {
                case TileType.Fast:
                    Instantiate(speedTilePrefab, new Vector3(tiles[i].position.x,0, tiles[i].position.y), speedTilePrefab.transform.rotation, this.transform);
                    break;
                case TileType.Slow:
                    Instantiate(slowTilePrefab, new Vector3(tiles[i].position.x, 0, tiles[i].position.y), slowTilePrefab.transform.rotation, this.transform);
                    break;
                default:
                    break;
            }
        }
    }
}
