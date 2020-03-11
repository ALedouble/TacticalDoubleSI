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

    List<Entity> entities = new List<Entity>();
    string[] entitiesNames;

    private void OnEnable()
    {
        PopulateTileTypeDictionary();
        SceneView.duringSceneGui += OnScene;

        string[] entitiesGUIDs = AssetDatabase.FindAssets("t:Entity");

        entities.Clear();
        for (int i = 0; i < entitiesGUIDs.Length; i++)
        {
            Entity entityBuffer = (AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(entitiesGUIDs[i]), typeof(Entity)) as Entity);

            if (entityBuffer.alignement != Alignement.Player) entities.Add(entityBuffer);
        }


        entitiesNames = new string[entities.Count];
        for (int i = 0; i < entities.Count; i++)
        {
            entitiesNames[i] = entities[i].name;
        }
    }

    public Map map;

    int selectedTileType;
    Dictionary<int, TileType> tileTypeDictionary = new Dictionary<int, TileType>();
    List<string> tileTypeDisplayNames = new List<string>();

    int selectedEntity;

    int brushMode;
    int selectedEntityBrush;

    int selectedCrystalValue;

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        map = EditorGUILayout.ObjectField("Map",map, typeof(Map), false) as Map;
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
        map.size = EditorGUILayout.IntField("Map Size", map.size);
        if (map.size > 100) map.size = 100;
        if (EditorGUI.EndChangeCheck())
        {
            map.Init(); 
            EditorUtility.SetDirty(map);
        }

        EditorGUI.BeginChangeCheck();
        map.center = EditorGUILayout.Vector2Field("Camera Focus Point", map.center);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(map);
        }
    }

    void OnScene(SceneView scene)
    {
        if (map == null) return;

        int controlID = GUIUtility.GetControlID("Map".GetHashCode(), FocusType.Passive);

        Event currentEvent = Event.current;


        OnClick(currentEvent, controlID);

        DrawDebugMap();

        if (brushMode == 0) TileToolbar();
        else EntityToolbar();

        BrushModeToolbar();

        SceneView.RepaintAll();
    }

    void BrushModeToolbar()
    {
        if (!focused) return;

        Handles.BeginGUI();

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Brush mode", EditorStyles.whiteLargeLabel, GUILayout.MaxWidth(100));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        brushMode = GUILayout.Toolbar(brushMode, new string[]{"Tile","Entities"}, GUILayout.MinHeight(35));
        EditorGUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(40);
        Handles.EndGUI();

    }

    void TileToolbar()
    {
        if (!focused) return;

        Handles.BeginGUI();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Brush selection", EditorStyles.whiteLargeLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        selectedTileType = GUILayout.Toolbar(selectedTileType, tileTypeDisplayNames.ToArray(), GUILayout.MinHeight(35));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        Handles.EndGUI();

    }

    void EntityToolbar()
    {
        if (!focused) return;

        Handles.BeginGUI();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Brush selection", EditorStyles.whiteLargeLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        selectedEntityBrush = GUILayout.Toolbar(selectedEntityBrush, new string[]{"Place","Remove","Add Crystal"}, GUILayout.MinHeight(35));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();



        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        if (selectedEntityBrush != 2) selectedEntity = EditorGUILayout.Popup(selectedEntity, entitiesNames);
        else selectedCrystalValue = EditorGUILayout.Popup(selectedCrystalValue, new string[] {"No Crystal","Crystal A", "Crystal B", "Crystal C" });
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        Handles.EndGUI();

    }

    void OnClick(Event e, int controlID)
    {
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && e.modifiers != EventModifiers.Control && e.modifiers != EventModifiers.Alt && focused)
        {
            TileData hoveredTile = SelectionUtils.MapRaycastEditor(map, e).tile;

            if (hoveredTile != null)
            {
                switch (brushMode)
                {
                    case 0:

                        // TILE MODE

                        if (selectedTileType < 4) hoveredTile.tileType = tileTypeDictionary[selectedTileType];
                        else
                        {
                            hoveredTile.canPlacePlayerEntity = !hoveredTile.canPlacePlayerEntity;
                        }
                        break;
                    case 1:

                        // ENTITY MODE

                        if (e.type != EventType.MouseDown) break;

                        switch (selectedEntityBrush)
                        {
                            case 0:

                                bool validTile = true;
                                for (int i = 0; i < map.entityStartPositions.Count; i++)
                                {
                                    if (map.entityStartPositions[i].position == hoveredTile.position)
                                    {
                                        validTile = false;
                                    }
                                }
                                if (hoveredTile.position.x < 0 || hoveredTile.position.y < 0 || hoveredTile.position.x > map.size || hoveredTile.position.y > map.size) validTile = false;

                                if (validTile) map.entityStartPositions.Add(new EntityRoundStartState(entities[selectedEntity], -1, hoveredTile.position));

                                break;
                            case 1:

                                for (int i = 0; i < map.entityStartPositions.Count; i++)
                                {
                                    if (map.entityStartPositions[i].position != hoveredTile.position) continue;

                                    map.entityStartPositions.RemoveAt(i);
                                    break;
                                }

                                break;
                            case 2:

                                for (int i = 0; i < map.entityStartPositions.Count; i++)
                                {
                                    if (map.entityStartPositions[i].position == hoveredTile.position)
                                    {
                                        map.entityStartPositions[i].heldCrystalValue = selectedCrystalValue - 1;
                                        break;
                                    }
                                }

                                break;
                            default:
                                break;
                        }

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

    private Color normal = new Color(.9f, .9f, .9f, .1f);
    private Color solid = new Color(.1f, .1f, .1f, .5f);
    private Color fast = new Color(1, .5f, 0.8f, .1f);
    private Color slow = new Color(.5f, 0f, .5f, .1f);
    private Color playerPlacement = new Color(.0f, 0f, .8f, .2f);

    void DrawDebugMap()
    {
        if (map == null) return;
         
        Event currentEvent = Event.current;

        for (int x = 0; x < map.size; x++)
        {
            for (int y = 0; y < map.size; y++)
            {
                DebugUtils.DrawTileEditor(new Vector2Int(x, y), map.GetTile(new Vector2Int(x,y)).TileType == TileType.Normal ? normal :
                    map.GetTile(new Vector2Int(x, y)).TileType == TileType.Solid ? solid :
                    map.GetTile(new Vector2Int(x, y)).TileType == TileType.Fast ? fast : slow);

                // Player placement
                if (map.GetTile(x, y).canPlacePlayerEntity) DebugUtils.DrawTileEditor(new Vector2Int(x, y), playerPlacement, .5f);


                // Mouse tile
                Vector2Int mousePosOnMap = SelectionUtils.MapRaycastEditor(map, currentEvent).position;

                if (mousePosOnMap == new Vector2Int(x, y) && focused)
                {
                    DebugUtils.DrawTileEditor(new Vector2Int(x, y), new Color(.9f, .9f, .9f, .5f));
                }

            }
        }


        for (int x = 0; x < map.size; x++)
        {
            for (int y = 0; y < map.size; y++)
            {
                // Entities
                for (int i = 0; i < map.entityStartPositions.Count; i++)
                {
                    if (map.entityStartPositions[i].position == new Vector2Int(x, y))
                    {
                        GUIStyle newStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
                        newStyle.alignment = TextAnchor.MiddleCenter;
                        Handles.Label(new Vector3(map.entityStartPositions[i].position.x, .2f, map.entityStartPositions[i].position.y), 
                            map.entityStartPositions[i].entity.displayName + (map.entityStartPositions[i].heldCrystalValue == -1 ? "" :
                            map.entityStartPositions[i].heldCrystalValue == 0 ? "[A]" :
                            map.entityStartPositions[i].heldCrystalValue == 1 ? "[B]" : "[C]")
                            , newStyle);
                    }
                }
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

    bool focused;

    private void OnBecameVisible()
    {
        focused = true;
    }

    private void OnBecameInvisible()
    {
        focused = false;
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

        tileTypeDisplayNames.Add("PlayerPlacement");

    }
}