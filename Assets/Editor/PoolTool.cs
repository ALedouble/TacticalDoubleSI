using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

public class PoolTool : EditorWindow
{
    [MenuItem("Window/PoolTool")]
    public static void OnOpen()
    {
        PoolTool myWindow = EditorWindow.GetWindow(typeof(PoolTool)) as PoolTool;
        myWindow.Init();
    }
    
    public void Init()
    {
        minSize = new Vector2(350, 100);

        Show();
    }

    int selectedCollectionIndex = -1;
    PoolCollection currentCollection = null;
    bool collectionRenameToggle = false;
    bool renameWarning = false;

    Vector2 scrollView;
    private void OnGUI()
    {
        scrollView = EditorGUILayout.BeginScrollView(scrollView);

        EditorGUILayout.Space();

        //////////////////////////////////////////////////  Title
        /*
        Color oldColor = GUI.color;

        GUI.color = Color.gray;

        EditorGUILayout.BeginVertical();

        GUI.color = new Color(0.9f,0.9f,0.9f);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 30;

        EditorGUILayout.LabelField(new GUIContent("Pool Tool"), titleStyle,GUILayout.ExpandHeight(true), GUILayout.Height(30));
        
        EditorGUILayout.EndVertical();

        GUI.color = oldColor;*/

        //////////////////////////////////////////////////  Pool collection selection

        EditorGUILayout.Space();
        
        Color oldColor = GUI.color;

        GUI.color = new Color(0.8f,0.8f,0.8f);

        EditorGUILayout.BeginVertical(new GUIStyle("box"));

        GUI.color = oldColor;

        if (selectedCollectionIndex == -1) EditorGUILayout.HelpBox("Select a Pool Collection", MessageType.Info,true);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        string[] poolCollectionsPaths = AssetDatabase.FindAssets("t:PoolCollection");
        string[] poolCollectionsNames = new string[poolCollectionsPaths.Length];

        for (int i = 0; i < poolCollectionsPaths.Length; i++)
        {
            poolCollectionsNames[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(poolCollectionsPaths[i]));
            poolCollectionsPaths[i] = AssetDatabase.GUIDToAssetPath(poolCollectionsPaths[i]);
        }

        EditorStyles.popup.fontSize = 13;
        EditorStyles.popup.fixedHeight = 20;

        selectedCollectionIndex = EditorGUILayout.Popup(GUIContent.none, selectedCollectionIndex, poolCollectionsNames, GUILayout.MinWidth(Screen.width - 100*2), GUILayout.MinHeight(20));

        EditorStyles.popup.fontSize = 10;
        EditorStyles.popup.fixedHeight = 16;

        if (GUILayout.Button("New", GUILayout.MaxWidth(40), GUILayout.MaxHeight(20)))
        {
            CreateNewPoolCollection();

            poolCollectionsPaths = AssetDatabase.FindAssets("t:PoolCollection");
            poolCollectionsNames = new string[poolCollectionsPaths.Length];

            return;
        }
        if (GUILayout.Button("Rename", GUILayout.MaxWidth(60), GUILayout.MaxHeight(20)))
        {
            collectionRenameToggle = !collectionRenameToggle;
            renameWarning = false;
        }

        if (currentCollection != null)
        {
            oldColor = GUI.color;
            GUI.color = new Color(.9f, .3f, .3f);
            if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
            {
                selectedCollectionIndex = -1;
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(currentCollection.name)[0]));
                return;
            }
            GUI.color = oldColor;
        }

        if (selectedCollectionIndex != -1) currentCollection = AssetDatabase.LoadAssetAtPath(poolCollectionsPaths[selectedCollectionIndex], typeof(PoolCollection)) as PoolCollection;

        if (currentCollection != null) currentCollection.pools.RemoveAll(p => p == null);

        EditorGUILayout.EndHorizontal();


        //////////////////////////////////////////////////  Pool Collection renaming

        if (currentCollection == null)
        {
            EditorGUILayout.HelpBox("No Collection is selected", MessageType.Warning);
        }
        else
        {
            if (collectionRenameToggle)
            {
                EditorGUILayout.LabelField("Renaming :", GUILayout.MaxWidth(110));

                EditorGUI.BeginChangeCheck();

                string desiredName = EditorGUILayout.DelayedTextField(currentCollection.name);

                if (EditorGUI.EndChangeCheck())
                {
                    if (AssetExists<PoolCollection>("Assets/ScriptableObjects/Pooling/PoolCollections/" + desiredName))
                    {
                        renameWarning = true;
                    }
                    else
                    {
                        collectionRenameToggle = false;
                        renameWarning = false;
                        currentCollection.name = desiredName;
                        AssetDatabase.RenameAsset(poolCollectionsPaths[selectedCollectionIndex], currentCollection.name);
                        AssetDatabase.SaveAssets();
                    }

                }
                
            }
            
            if (renameWarning)
            {
                EditorGUILayout.HelpBox("There is already an asset with this name", MessageType.Info);
            }

            //////////////////////////////////////////////////  Pools


            for (int i = 0; i < currentCollection.pools.Count; i++)
            {
                EditorGUILayout.Space();
                DrawPool(currentCollection.pools[i]);
            }

            EditorGUILayout.Space();

            
            if (GUILayout.Button("Create new pool"))
            {
                currentCollection.pools.Add(CreateNewPool());
                EditorUtility.SetDirty(currentCollection);
            }

            EditorGUILayout.Space();

            //////////////////////////////////////////////////////////////////////////////////////////////  DRAG AND DROP

            Event evt = Event.current;
            Rect dropRect = GUILayoutUtility.GetRect(0.0f, 80.0f, GUILayout.ExpandWidth(true));

            GUIStyle centeredText = new GUIStyle("box");
            centeredText.alignment = TextAnchor.MiddleCenter;

            GUI.Box(dropRect, "Drag and Drop Pools here", centeredText);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropRect.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object is Pool)
                            {
                                currentCollection.pools.Add((Pool)dragged_object);

                                EditorUtility.SetDirty(currentCollection);
                            }
                        }
                    }
                    break;
            }
            
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    Pool CreateNewPool()
    {
        string path = "Assets/ScriptableObjects/Pooling/Pools";

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects")) AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Pooling")) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Pooling");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Pooling/Pools")) AssetDatabase.CreateFolder("Assets/ScriptableObjects/Pooling", "Pools");

        path += "/new Pool.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        Pool newPool = ScriptableObject.CreateInstance<Pool>();
        newPool.name = Path.GetFileNameWithoutExtension(path);

        AssetDatabase.CreateAsset(newPool, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        return newPool;
    }
    
    void DrawPool(Pool pool)
    {
        SerializedObject serializedObject = new SerializedObject(pool);

        SerializedProperty prefab = serializedObject.FindProperty("prefab");
        SerializedProperty size = serializedObject.FindProperty("size");

        EditorGUILayout.BeginVertical(new GUIStyle("box"));

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Rename", GUILayout.MaxHeight(14)))
        {
            pool.renaming = !pool.renaming;
        }

        if (GUILayout.Button("Remove", GUILayout.MaxHeight(14)))
        {
            currentCollection.pools.Remove(pool);
        }

        Color oldColor = GUI.color;
        GUI.color = new Color(.9f, .3f, .3f);
        if (GUILayout.Button("Delete", GUILayout.MaxHeight(14)))
        {
            for (int i = 0; i < currentCollection.pools.Count; i++)
            {
                if (currentCollection.pools[i] == pool) currentCollection.pools.Remove(pool);
            }
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(pool.name)[0]));
            return;
        }
        GUI.color = oldColor;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
        if (pool.renaming)
        {
            EditorGUI.BeginChangeCheck();

            string desiredName = EditorGUILayout.DelayedTextField(pool.name);
            

            if (EditorGUI.EndChangeCheck())
            {
                if (AssetExists<Pool>("Assets/ScriptableObjects/Pooling/Pools/"+desiredName))
                {
                    renameWarning = true;
                }
                else
                {
                    pool.renaming = false;
                    renameWarning = false;
                    AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(pool.name)[0]), desiredName);
                    pool.name = desiredName;
                    AssetDatabase.SaveAssets();
                }

            }
        }
        else
        {
            EditorGUILayout.LabelField(pool.name, EditorStyles.boldLabel);
        }
        
        EditorGUILayout.PropertyField(prefab,GUIContent.none);
        
        EditorGUILayout.EndHorizontal();

        size.intValue = EditorGUILayout.IntSlider(size.intValue, 1, 1000);

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    void CreateNewPoolCollection()
    {
        string path = "Assets/ScriptableObjects/Pooling/PoolCollections";

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects")) AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Pooling")) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Pooling");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Pooling/PoolCollections")) AssetDatabase.CreateFolder("Assets/ScriptableObjects/Pooling", "PoolCollections");

        path += "/new PoolCollection.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        PoolCollection newCollection = ScriptableObject.CreateInstance<PoolCollection>();
        newCollection.name = Path.GetFileNameWithoutExtension(path);

        AssetDatabase.CreateAsset(newCollection, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        selectedCollectionIndex = FindCollectionIndexByPath(path);
    }

    int FindCollectionIndexByPath(string path)
    {
        string[] poolCollectionsPaths = AssetDatabase.FindAssets("t:PoolCollection");

        for (int i = 0; i < poolCollectionsPaths.Length; i++)
        {
            if (Path.GetFileNameWithoutExtension(path) == Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(poolCollectionsPaths[i])))
            {
                return i;
            }
        }

        return -1;
    }

    bool AssetExists<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path + ".asset");
        if (asset == null) return false;
        else return true;
    }
}
