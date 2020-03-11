using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    public GameObject castAreaTilePrefab;
    public GameObject effectAreaTilePrefab;
    public GameObject HoverTilePrefab;
    public GameObject slowTilePrefab;
    public GameObject speedTilePrefab;
    public GameObject spawnTile;
    public GameObject movementTile;

    [HideInInspector] public LineRenderer pathRenderer;

    private void Awake()
    {
        Instance = this;
    }

    public List<GameObject> placePlayerTile = new List<GameObject>();

    private void Start()
    {
        pathRenderer = GetComponentInChildren<LineRenderer>();

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

            if (tiles[i].canPlacePlayerEntity)
            {
                placePlayerTile.Add(Instantiate(spawnTile, new Vector3(tiles[i].position.x, 0.01f, tiles[i].position.y), spawnTile.transform.rotation));
            }
        }

        PlayerTeamManager.Instance.OnFinishPlacement += () =>
        {
            for (int i = 0; i < placePlayerTile.Count; i++)
            {
                Destroy(placePlayerTile[i]);
            }
            placePlayerTile.Clear();
        };

        hoverTile = Instantiate(HoverTilePrefab);
        hoverSystem = hoverTile.GetComponent<ParticleSystem>();
        UpdateHoverTile(new MapRaycastHit(null, new Vector2Int()));

        SelectionManager.Instance.OnHoveredTileChanged += UpdateHoverTile;

        MapManager.Instance.OnCastableTilesChanged += UpdateCastableTiles;
        MapManager.Instance.OnEffectTilesChanged += UpdateEffectTiles;
        MapManager.Instance.OnReachableTilesChanged += UpdateMovementTiles;

        SelectionManager.Instance.OnCancel += () =>
        {
            pathRenderer.positionCount = 0;
        };
    }


    GameObject hoverTile;
    ParticleSystem hoverSystem;

    void UpdateHoverTile(MapRaycastHit mapHit)
    {
        if (mapHit.tile == null || mapHit.tile.tileType == TileType.Solid)
        {
            pathRenderer.positionCount = 0;
            hoverTile.SetActive(false);
        }
        else
        {
            hoverTile.SetActive(true);
            hoverTile.transform.position = new Vector3(mapHit.position.x, 0.01f, mapHit.position.y);
            hoverSystem.Clear(true);
            hoverSystem.Play(true);

            if (MapManager.Instance.reachableTiles == null)
            {
                pathRenderer.positionCount = 0;
                return;
            }

            bool pathExists = false;
            //movement LINE
            for (int i = 0; i < MapManager.Instance.reachableTiles.Count; i++)
            {
                if (MapManager.Instance.reachableTiles[i].GetCoordPosition() == mapHit.position)
                {
                    pathExists = true;
                    float lineHeight = 0.02f;

                    pathRenderer.positionCount = MapManager.Instance.reachableTiles[i].path.Count;
                    Vector2 position = ((Vector2)(MapManager.Instance.reachableTiles[i].path[0].position + MapManager.Instance.reachableTiles[i].path[1].position))/2;
                    pathRenderer.SetPosition(0, new Vector3(position.x, lineHeight, position.y));
                    for (int j = 1; j < pathRenderer.positionCount-1; j++)
                    {
                        position = MapManager.Instance.reachableTiles[i].path[j].position;
                        pathRenderer.SetPosition(j, new Vector3(position.x, lineHeight, position.y));
                    }
                    position = ((Vector2)(MapManager.Instance.reachableTiles[i].path[pathRenderer.positionCount - 2].position + MapManager.Instance.reachableTiles[i].path[pathRenderer.positionCount - 1].position)) / 2;
                    pathRenderer.SetPosition(pathRenderer.positionCount-1, new Vector3(position.x, lineHeight, position.y));
                }
            }
            if (!pathExists) pathRenderer.positionCount = 0;
        }
    }

    public List<GameObject> castableTiles = new List<GameObject>();
    private void UpdateCastableTiles(List<Vector2Int> obj)
    {
        for (int i = 0; i < castableTiles.Count; i++)
        {
            PoolManager.Recycle(castableTiles[i]);
        }
        castableTiles.Clear();

        if (obj == null) return;

        for (int i = 0; i < obj.Count; i++)
        {
            castableTiles.Add(PoolManager.InstantiatePooled(castAreaTilePrefab, new Vector3(obj[i].x, 0.01f, obj[i].y), this.transform));
            ParticleSystem ps;
            ps = castableTiles[i].GetComponent<ParticleSystem>();
            ps.Clear(true);
            ps.Play(true);
        }
    }

    public List<GameObject> effectTiles = new List<GameObject>();
    private void UpdateEffectTiles(List<Vector2Int> obj)
    {
        for (int i = 0; i < effectTiles.Count; i++)
        {
            PoolManager.Recycle(effectTiles[i]);
        }
        effectTiles.Clear();

        if (obj == null) return;

        for (int i = 0; i < obj.Count; i++)
        {
            effectTiles.Add(PoolManager.InstantiatePooled(effectAreaTilePrefab, new Vector3(obj[i].x, 0.01f, obj[i].y), this.transform));
            ParticleSystem ps;
            ps = effectTiles[i].GetComponent<ParticleSystem>();
            ps.Clear(true);
            ps.Play(true);
        }
    }

    public List<GameObject> movementTiles = new List<GameObject>();
    private void UpdateMovementTiles(List<ReachableTile> obj)
    {
        for (int i = 0; i < movementTiles.Count; i++)
        {
            PoolManager.Recycle(movementTiles[i]);
        }
        movementTiles.Clear();

        if (obj == null) return;

        for (int i = 0; i < obj.Count; i++)
        {
            movementTiles.Add(PoolManager.InstantiatePooled(movementTile, new Vector3(obj[i].GetCoordPosition().x, 0.01f, obj[i].GetCoordPosition().y), this.transform));
            ParticleSystem ps;
            ps = movementTiles[i].GetComponent<ParticleSystem>();
            ps.Clear(true);
            ps.Play(true);
        }
    }

    public static GameObject SpawnFX(GameObject go, Vector2Int position, Vector2 direction)
    {
        GameObject fx = PoolManager.InstantiatePooled(go, new Vector3(position.x, 0.01f, position.y), FXManager.Instance.transform);
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            PoolManager.Recycle(fx);
        });
        seq.SetDelay(10);
        return fx;
    }
}
