using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class MapEditorWindow : EditorWindow
{
    [MenuItem("Tools/Map Editor Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MapEditorWindow window = (MapEditorWindow)GetWindow(typeof(MapEditorWindow));
        window.name = "Map Editor";
        window.Show();

    }

    private void OnEnable()
    {
        PopulateTileTypeDictionary();
        SceneView.duringSceneGui += OnScene;
    }

    public Map map;

    int selectedTileType;
    Dictionary<int, TileType> tileTypeDictionary = new Dictionary<int, TileType>();
    List<string> tileTypeDisplayNames = new List<string>();

    private void OnProjectChange()
    {
        
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        map = EditorGUILayout.ObjectField(map, typeof(Map), false) as Map;
        if (EditorGUI.EndChangeCheck())
        {
            if (map != null)
            {
                map.Init();
                EditorUtility.SetDirty(map);
            } 
        }

        if (map == null) return;

        EditorGUI.BeginChangeCheck();
        map.size = EditorGUILayout.IntField(map.size);
        if (EditorGUI.EndChangeCheck())
        {
            map.Init(); 
            EditorUtility.SetDirty(map);
        }

        EditorGUILayout.LabelField(map.size.ToString());
        EditorGUILayout.LabelField(map.map.Count.ToString());

        selectedTileType = GUILayout.Toolbar(selectedTileType, tileTypeDisplayNames.ToArray(), GUILayout.MinHeight(35));
    }


    void OnScene(SceneView scene)
    {
        if (map == null) return;

        int controlID = GUIUtility.GetControlID("Map".GetHashCode(), FocusType.Passive);

        Event currentEvent = Event.current;

        PlaceTile(currentEvent, controlID);

        DrawDebugMap();
         
        SceneView.RepaintAll();
    }

    void PlaceTile(Event e, int controlID)
    {
        if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers != EventModifiers.Control && e.modifiers != EventModifiers.Alt)
        {
            TileData hoveredTile = SelectionUtils.MapRaycastEditor(map, e).tile;

            if (hoveredTile != null)
            {
                switch (selectedTileType)
                {
                    case 0:
                        hoveredTile.tileType = TileType.Normal;
                        break;
                    case 1:
                        hoveredTile.tileType = TileType.Solid;
                        break;
                    case 2:
                        hoveredTile.tileType = TileType.Fast;
                        break;
                    case 3:
                        hoveredTile.tileType = TileType.Slow;
                        break;
                    default:
                        break;
                }

                EditorUtility.SetDirty(map);
            }
        }

        if (e.type == EventType.Layout && e.modifiers != EventModifiers.Alt && e.button == 0)
        {
            HandleUtility.AddDefaultControl(controlID);
        }
    }

    private Color normal = new Color(.9f, .9f, .9f, .5f);
    private Color solid = new Color(.1f, .1f, .1f, .5f);
    private Color fast = new Color(1, .8f, 0.8f, .5f);
    private Color slow = new Color(.5f, 0f, .5f, .5f);

    void DrawDebugMap()
    {
        if (map == null) return;
         
        Event currentEvent = Event.current;

        for (int x = 0; x < map.map.Count; x++)
        {
            for (int y = 0; y < map.map.Count; y++)
            {
                DebugUtils.DrawTileEditor(new Vector2Int(x, y), map.GetTile(new Vector2Int(x,y)).TypeOfTyle == TileType.Normal ? normal :
                    map.GetTile(new Vector2Int(x, y)).TypeOfTyle == TileType.Solid ? solid :
                    map.GetTile(new Vector2Int(x, y)).TypeOfTyle == TileType.Fast ? fast : slow);

                Vector2Int mousePosOnMap = SelectionUtils.MapRaycastEditor(map, currentEvent).position;

                if (mousePosOnMap == new Vector2Int(x,y)) DebugUtils.DrawTileEditor(new Vector2Int(x, y), new Color(.9f, .9f, .9f, .5f));
            }
        }

    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    private string mapPath
    {
        get
        {
            return "Assets/ScriptableObjects/Maps/";
        }
    }

    void PopulateTileTypeDictionary()
    {
        tileTypeDictionary.Clear();
        tileTypeDisplayNames.Clear();

        tileTypeDictionary.Add(0, TileType.Normal);
        tileTypeDisplayNames.Add(tileTypeDictionary[0].ToString());
        tileTypeDictionary.Add(1, TileType.Solid);
        tileTypeDisplayNames.Add(tileTypeDictionary[1].ToString());
        tileTypeDictionary.Add(2, TileType.Fast);
        tileTypeDisplayNames.Add(tileTypeDictionary[2].ToString());
        tileTypeDictionary.Add(3, TileType.Slow);
        tileTypeDisplayNames.Add(tileTypeDictionary[3].ToString());
    }
}