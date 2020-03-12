using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Map))]
public class MapInspector : Editor
{
    private ReorderableList list;

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("entityStartPositions"),
                true, true, false, false);
        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Map Entities");
        };
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => 
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.LabelField(
                new Rect(rect.x, rect.y, Screen.width, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("entity").objectReferenceValue.name);
        };
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
