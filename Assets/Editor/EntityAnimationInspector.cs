using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityAnimation))]
public class EntityAnimationInspector : Editor
{
    EntityAnimation animation;

    private void OnEnable()
    {
        animation = target as EntityAnimation;

        EditorApplication.update += OnUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
    }

    void OnUpdate()
    {
        Repaint();
    }

    float lengthOverride = 0;

    public override void OnInspectorGUI()
    {
        ////////////////////////////////////////////////////////////////////////////////////////////// PREVIEW

        GUI.DrawTexture(GUILayoutUtility.GetRect(Screen.width, 100), 
            animation.GetTexture(Mathf.Repeat((float)EditorApplication.timeSinceStartup, animation.Length)),
            ScaleMode.ScaleToFit);

        EditorGUILayout.Space(20);

        ///

        EditorGUILayout.BeginHorizontal();
        lengthOverride = EditorGUILayout.FloatField(lengthOverride);
        if (GUILayout.Button("Apply length to all frames"))
        {
            for (int i = 0; i < serializedObject.FindProperty("frames").arraySize; i++)
            {
                serializedObject.FindProperty("frames").GetArrayElementAtIndex(i).FindPropertyRelative("length").floatValue = lengthOverride;
            }
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Total animation length : " + animation.Length + "s");

        ////////////////////////////////////////////////////////////////////////////////////////////// FRAMES

        for (int i = 0; i < animation.frames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("helpbox");

            EditorGUILayout.BeginVertical();

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frames").GetArrayElementAtIndex(i), new GUIContent("Frame " + i.ToString()));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

            if (CustomEditorUtility.RemoveButton())
            {
                serializedObject.FindProperty("frames").DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                return;
            }


            EditorGUILayout.EndHorizontal();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////  DRAG AND DROP

        EditorGUILayout.Space(20);

        Event evt = Event.current;
        Rect dropRect = GUILayoutUtility.GetRect(0.0f, 80.0f, GUILayout.ExpandWidth(true));

        GUIStyle centeredText = new GUIStyle("box");
        centeredText.alignment = TextAnchor.MiddleCenter;

        GUI.Box(dropRect, "Drag and Drop Textures here", centeredText);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropRect.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is Texture)
                        {
                            animation.frames.Add(new AnimationFrame(dragged_object as Texture, .1f));

                            EditorUtility.SetDirty(animation);
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                }
                break;
        }

    }
}
