using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugMapManager : MonoBehaviour
{
    public Map map;

    [Header("AreaAndPath")]
    public bool drawDebug;
    public bool fullPath;
    public int range = 150;

    private bool saveFullPath;
    private bool firstClick = true;
    private bool havePath = false;
    private Vector2Int start;
    private Vector2Int end;
    List<TileData> path;
    private List<ReachableTiles> tiles;
    MapRaycastHit tileSelect;

    [Header("Attack")]

    public static DebugMapManager Instance;

    private void Awake()
    {
        // No need to check if the instance is null because the MapManager will always be destroyed at the end of the level
        Instance = this;

        // Create a copy of map since ScriptableObjects are persistent and we don't want to change the map in the project while in the editor
        map = Instantiate(map);
    }

    private void Start()
    {
        Init();
        saveFullPath = fullPath;
    }

    private void Init()
    {
        Instance.map.map = new List<List<TileData>>();

        for (int x = 0; x < Instance.map.size; x++)
        {
            Instance.map.map.Add(new List<TileData>());
            for (int y = 0; y < Instance.map.size; y++)
            {
                Instance.map.map[x].Add(new TileData(TileType.Normal, 10 * x + y));
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
                tileSelect.tile.TileType = TileType.ClickStart;

                tiles = IAUtils.FindAllReachablePlace(tileSelect.position, Instance.map.map, range);

                foreach (ReachableTiles tile in tiles)
                {
                    GetTile(tile.coordPosition).TileType = TileType.Fast;
                }
            }

            else
            {
                end = tileSelect.position;
                havePath = true;

                foreach (ReachableTiles tile in tiles)
                {
                    if (tile.coordPosition.Equals(tileSelect.position))
                    {
                        foreach (ReachableTiles tile2 in tiles)
                        {
                            GetTile(tile2.coordPosition).TileType = TileType.Fast;
                        }

                        path = IAUtils.FindShortestPath(start, Instance.map.map, tileSelect.position, range, fullPath);

                        foreach (TileData tileData in path)
                        {
                            tileData.TileType = TileType.Slow;
                        }

                        break;
                    }
                }

                tileSelect.tile.TileType = TileType.ClickEnd;
            }
            
        }

        if (tileSelect.tile != null && Input.GetMouseButtonDown(2))
        {
            if (GetTile(tileSelect.position).TileType != TileType.Solid)
            {
                GetTile(tileSelect.position).TileType = TileType.Solid;
            }
            else
            {
                GetTile(tileSelect.position).TileType = TileType.Normal;
            }

            if (havePath)
            {
                foreach (TileData tileData in path)
                {
                    if (tileData.TileType == TileType.Slow)
                        tileData.TileType = TileType.Normal;
                }
            }

            if (!firstClick)
            {
                foreach (ReachableTiles tile in tiles)
                {
                    if (GetTile(tile.coordPosition).TileType == TileType.Fast)
                    {
                        GetTile(tile.coordPosition).TileType = TileType.Normal;
                    }
                }

                tiles = IAUtils.FindAllReachablePlace(start, Instance.map.map, range);

                foreach (ReachableTiles tile in tiles)
                {
                    if (GetTile(tile.coordPosition).TileType != TileType.ClickEnd)
                    {
                        GetTile(tile.coordPosition).TileType = TileType.Fast;
                    }
                }
            }

            if (havePath)
            {
                path = IAUtils.FindShortestPath(start, Instance.map.map, end, range, fullPath);

                foreach (TileData tileData in path)
                {
                    if (tileData.TileType != TileType.ClickEnd)
                    {
                        tileData.TileType = TileType.Slow;
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
            foreach (TileData tileData in path)
            {
                if (tileData.TileType == TileType.Slow)
                    tileData.TileType = TileType.Normal;
            }

            path = IAUtils.FindShortestPath(start, Instance.map.map, end, range, fullPath);

            foreach (TileData tileData in path)
            {
                if (tileData.TileType != TileType.ClickEnd)
                {
                    tileData.TileType = TileType.Slow;
                }
            }

            saveFullPath = fullPath;
        }
    }


    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        if (Instance != null)
        {
            for (int x = 0; x < Instance.map.size; x++)
            {
                for (int y = 0; y < Instance.map.size; y++)
                {
                    DebugUtils.DrawTile(new Vector2Int(x, y), Instance.map.map[x][y].TileType.Equals(TileType.Solid) ? new Color(.9f, 0, 0, .5f) :
                                                                Instance.map.map[x][y].TileType.Equals(TileType.Fast) ? new Color(0, .9f, 0, .5f) :
                                                                Instance.map.map[x][y].TileType.Equals(TileType.Slow) ? new Color(0, 0, .9f, .5f) :
                                                                Instance.map.map[x][y].TileType.Equals(TileType.ClickStart) ? new Color(1, 0.9f, 0, .5f) :
                                                                Instance.map.map[x][y].TileType.Equals(TileType.ClickEnd) ? new Color(1, 0, 0.8f, .5f) :
                                                                 new Color(.9f, .9f, .9f, .5f));


                    Handles.Label(new Vector3(x, .25f, y), Instance.map.map[x][y].Cost.ToString());

                }
            }
        }



        DebugUtils.DrawTile(tileSelect.position, new Color(.2f, .2f, .5f, .5f));
    }

    public static TileData GetTile(Vector2Int position)
    {
        if (position.x >= 0 && position.x < GetSize() && position.y >= 0 && position.y < GetSize())
            return Instance.map.map[position.x][position.y];

        return null;
    }

    public static int GetSize()
    {
        return Instance.map.size;
    }

    public static Map GetMap()
    {
        return Instance.map;
    }
}
