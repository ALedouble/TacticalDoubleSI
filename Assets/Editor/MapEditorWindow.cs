using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class MapEditorWindow : EditorWindow
{
    [MenuItem("Tools/Map Editor Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MapEditorWindow window = (MapEditorWindow)GetWindow(typeof(MapEditorWindow));
        window.name = "Map Editor";
        window.Show();

        window.PopulateTileTypeDictionary();
    }

    Map map;

    int selectedTileType;
    Dictionary<int, TileType> tileTypeDictionary = new Dictionary<int, TileType>();
    List<string> tileTypeDisplayNames = new List<string>();

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        map = EditorGUILayout.ObjectField(map, typeof(Map), false) as Map;

        selectedTileType = GUILayout.Toolbar(selectedTileType, tileTypeDisplayNames.ToArray());
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