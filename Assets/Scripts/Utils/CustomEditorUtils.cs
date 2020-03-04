using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomEditorUtils : MonoBehaviour
{
    public static List<Vector2Int> PropertyToVector2Int(SerializedProperty property) {
        List<Vector2Int> newArea = new List<Vector2Int>();
        SerializedProperty tileArea = property.FindPropertyRelative("area");

        for(int i = 0; i < tileArea.arraySize; i++)
        {
            newArea.Add(tileArea.GetArrayElementAtIndex(i).vector2IntValue);
        }

        return newArea;
    }

    public static void FillPropertyWithVector2Int(SerializedProperty property, List<Vector2Int> vectors)
    {
        SerializedProperty tileArea = property.FindPropertyRelative("area");

        tileArea.ClearArray();
        tileArea.arraySize = vectors.Count;

        for (int i = 0; i < vectors.Count; i++)
        {
            tileArea.InsertArrayElementAtIndex(i);
            tileArea.GetArrayElementAtIndex(i).vector2IntValue = vectors[i];
        }
    }

    public static void RepaintInspector(SerializedObject BaseObject)
    {
        foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
            if (item.serializedObject == BaseObject)
            { item.Repaint(); return; }
    }
}
