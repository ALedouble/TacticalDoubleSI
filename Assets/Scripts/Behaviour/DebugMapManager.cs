using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugMapManager : MonoBehaviour
{
    [Header("AreaAndPath")]
    public bool drawDebug;
    public bool fullPath;
    public int range = 15;

    private bool saveFullPath;
    private bool firstClick = true;
    private bool havePath = false;
    private Vector2Int start;
    private Vector2Int end;
    ReachableTile path;
    private List<ReachableTile> tiles;
    MapRaycastHit tileSelect;
       
    private Color normal = new Color(.9f, .9f, .9f, .5f);
    private Color player = new Color(1, 0.9f, 0, .5f);
    private Color target = new Color(1, 0, 0.8f, .5f);
    private Color chemin = new Color(0, 0, .9f, .5f);
    private Color wall = new Color(.9f, 0, 0, .5f);
    private Color area = new Color(0, .9f, 0, .5f);
    private Color attack = new Color(0, 0, 0, .5f);
    private Color attackBest = new Color(0, 1, 1, .5f);
    private TileArea tileArea;
    
    private void Start()
    {
        Init();
        saveFullPath = fullPath;

        tileArea = new TileArea();

        tileArea.size = 5;
        tileArea.area.Add(new Vector2Int(0, 2));
        tileArea.area.Add(new Vector2Int(1, 1));
        tileArea.area.Add(new Vector2Int(2, 0));
        tileArea.area.Add(new Vector2Int(3, 1));
        tileArea.area.Add(new Vector2Int(4, 2));
        tileArea.area.Add(new Vector2Int(3, 3));
        tileArea.area.Add(new Vector2Int(2, 4));
        tileArea.area.Add(new Vector2Int(1, 3));
    }

    private void Init()
    {
        Random rand = new Random();

        MapManager.Instance.map.map = new List<List<TileData>>();

        for (int x = 0; x < MapManager.Instance.map.size; x++)
        {
            MapManager.Instance.map.map.Add(new List<TileData>());
            for (int y = 0; y < MapManager.Instance.map.size; y++)
            {
                switch (rand.Next(0,4))
                {
                    case 0:
                        MapManager.Instance.map.map[x].Add(new TileData(TileType.Solid));
                        MapManager.Instance.map.map[x][y].color = wall;
                        break;

                    case 1:
                        MapManager.Instance.map.map[x].Add(new TileData(TileType.Fast));
                        MapManager.Instance.map.map[x][y].color = normal;
                        break;

                    case 2:
                        MapManager.Instance.map.map[x].Add(new TileData(TileType.Normal));
                        MapManager.Instance.map.map[x][y].color = normal;
                        break;

                    case 3:
                        MapManager.Instance.map.map[x].Add(new TileData(TileType.Slow));
                        MapManager.Instance.map.map[x][y].color = normal;
                        break;
                }
            }
        }
    }

    private void Update()
    {
        AreaAndPath();
        //Attack();
    }

    private void Attack()
    {
        tileSelect = SelectionUtils.MapRaycast();
        if (tileSelect.tile != null && Input.GetMouseButtonDown(0))
        {
            if (firstClick)
            {
                firstClick = false;
                start = tileSelect.position;
                tileSelect.tile.color = player;

                tiles = IAUtils.FindAllReachablePlace(tileSelect.position, MapManager.Instance.map.map, range);

                foreach (ReachableTile tile in tiles)
                {
                    MapManager.GetTile(tile.GetCoordPosition()).color = area;
                }
            }
            else
            {
                for (int x = 0; x < MapManager.Instance.map.size; x++)
                {
                    for (int y = 0; y < MapManager.Instance.map.size; y++)
                    {
                        if (MapManager.Instance.map.map[x][y].color != player)
                            MapManager.Instance.map.map[x][y].color = normal;
                    }
                }


                foreach (ReachableTile tile in tiles)
                {
                    MapManager.GetTile(tile.GetCoordPosition()).color = area;
                }

                end = tileSelect.position;
                tileSelect.tile.color = target;

                List<ReachableTile> canCast = IAUtils.ValidCastFromTile(tileArea, tiles, end);
                canCast.Sort();

                if (canCast.Count > 0)
                {
                    MapManager.GetTile(canCast[0].GetCoordPosition()).color = attackBest;

                    for (int i = 1; i < canCast.Count; i++)
                    {
                        MapManager.GetTile(canCast[i].GetCoordPosition()).color = attack;
                    }
                }
                else
                {
                    path = IAUtils.FindShortestPath(start, MapManager.Instance.map.map, end, range, fullPath);

                    path.path[path.path.Count - 1].color = chemin;
                }
                    

            }
        }
    }


    private void AreaAndPath()
    { 
        tileSelect = SelectionUtils.MapRaycast();
        if (tileSelect.tile != null && Input.GetMouseButtonDown(0))
        {
            if (firstClick)
            {
                firstClick = false;
                start = tileSelect.position;
                tileSelect.tile.color = player;

                tiles = IAUtils.FindAllReachablePlace(tileSelect.position, MapManager.Instance.map.map, range);

                foreach (ReachableTile tile in tiles)
                {
                    MapManager.GetTile(tile.GetCoordPosition()).color = area;
                }
            }

            else
            {
                end = tileSelect.position;
                havePath = true;

                for (int x = 0; x < MapManager.Instance.map.size; x++)
                {
                    for (int y = 0; y < MapManager.Instance.map.size; y++)
                    {
                        if (MapManager.Instance.map.map[x][y].color != player && MapManager.Instance.map.map[x][y].color != wall)
                            MapManager.Instance.map.map[x][y].color = normal;
                    }
                }

                foreach (ReachableTile tile in tiles)
                {
                    MapManager.GetTile(tile.GetCoordPosition()).color = area;
                }

                path = IAUtils.FindShortestPath(start, MapManager.Instance.map.map, tileSelect.position, range, fullPath);

                if (path != null)
                    foreach (TileData tileData in path.path)
                    {
                        tileData.color = chemin;
                    }

                tileSelect.tile.color = target;
            }
            
        }

        if (tileSelect.tile != null && Input.GetMouseButtonDown(2))
        {
            if (MapManager.GetTile(tileSelect.position).tileType != TileType.Solid)
            {
                MapManager.GetTile(tileSelect.position).tileType = TileType.Solid;
                MapManager.GetTile(tileSelect.position).color = wall;
            }
            else
            {
                MapManager.GetTile(tileSelect.position).tileType = TileType.Normal;
                MapManager.GetTile(tileSelect.position).color = normal;
            }

            if (havePath)
            {
                foreach (TileData tileData in path.path)
                {
                    if (tileData.color == chemin)
                        tileData.color = normal;
                }
            }

            if (!firstClick)
            {
                foreach (ReachableTile tile in tiles)
                {
                    if (MapManager.GetTile(tile.GetCoordPosition()).color == area)
                    {
                        MapManager.GetTile(tile.GetCoordPosition()).color = normal;
                    }
                }

                tiles = IAUtils.FindAllReachablePlace(start, MapManager.Instance.map.map, range);

                foreach (ReachableTile tile in tiles)
                {
                    if (MapManager.GetTile(tile.GetCoordPosition()).color != target)
                    {
                        MapManager.GetTile(tile.GetCoordPosition()).color = area;
                    }
                }
            }

            if (havePath)
            {
                path = IAUtils.FindShortestPath(start, MapManager.Instance.map.map, end, range, fullPath);

                foreach (TileData tileData in path.path)
                {
                    if (tileData.color != target)
                    {
                        tileData.color = chemin;
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Init();
            firstClick = true;
            havePath = false;
        }

        if (saveFullPath != fullPath)
        {
            foreach (TileData tileData in path.path)
            {
                if (tileData.color == chemin)
                    tileData.color = normal;
            }

            path = IAUtils.FindShortestPath(start, MapManager.Instance.map.map, end, range, fullPath);

            if (path != null)
                foreach (TileData tileData in path.path)
                {
                    if (tileData.color != target)
                    {
                        tileData.color = chemin;
                    }
                }

            saveFullPath = fullPath;
        }
    }


    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        if (MapManager.Instance != null)
        {
            for (int x = 0; x < MapManager.Instance.map.size; x++)
            {
                for (int y = 0; y < MapManager.Instance.map.size; y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), MapManager.Instance.map.map[x][y].color);

                    Handles.Label(new Vector3(x, .25f, y), MapManager.Instance.map.map[x][y].tileType.ToString());
                }
            }
        }

        DebugUtils.DrawTile(tileSelect.position, new Color(.2f, .2f, .5f, .5f));
    }
}
