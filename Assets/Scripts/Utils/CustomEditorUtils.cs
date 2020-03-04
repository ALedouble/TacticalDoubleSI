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

    public static void FillPropertyWithVector2Int(SerializedProperty property, bool[,] areaBuffer)
    {
        SerializedProperty tileArea = property.FindPropertyRelative("area");

        List<Vector2Int> vectorsBuffer = new List<Vector2Int>();

        tileArea.ClearArray();
        tileArea.arraySize = 0;
        for (int i = 0; i < areaBuffer.GetLength(0); i++)
        {
            for (int j = 0; j < areaBuffer.GetLength(1); j++)
            {
                tileArea.arraySize += areaBuffer[j, i] ? 1 : 0;
                if (!areaBuffer[j, i])
                    continue;

                vectorsBuffer.Add(new Vector2Int(j, i));
            }
        }

        

        for (int i = 0; i < tileArea.arraySize; i++)
        {
            tileArea.GetArrayElementAtIndex(i).vector2IntValue = vectorsBuffer[i];
        }
    }

    public static void RepaintInspector(SerializedObject BaseObject)
    {
        foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
            if (item.serializedObject == BaseObject)
            { item.Repaint(); return; }
    }
}
