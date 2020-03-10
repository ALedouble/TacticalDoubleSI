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

        hoverTile = Instantiate(HoverTilePrefab);
        hoverSystem = hoverTile.GetComponent<ParticleSystem>();
        UpdateHoverTile(new MapRaycastHit(null, new Vector2Int()));

        SelectionManager.Instance.OnHoveredTileChanged += UpdateHoverTile;
    }

    GameObject hoverTile;
    ParticleSystem hoverSystem;
    void UpdateHoverTile(MapRaycastHit mapHit)
    {
        if (mapHit.tile == null || mapHit.tile.tileType == TileType.Solid)
        {
            hoverTile.SetActive(false);
        }
        else
        {
            hoverTile.SetActive(true);
            hoverTile.transform.position = new Vector3(mapHit.position.x, 0.01f, mapHit.position.y);
            hoverSystem.Clear(true);
            hoverSystem.Play(true);
        }
    }
}
