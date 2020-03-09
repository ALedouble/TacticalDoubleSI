using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CustomEditorUtility
{
    public static SerializedProperty QuickSerializeRelative(string name, SerializedProperty parent)
    {
        SerializedProperty p = parent.FindPropertyRelative(name);
        if (p != null)
        {
            EditorGUILayout.PropertyField(p, true);
        }

        return p;
    }

    public static SerializedProperty QuickSerializeObject(string name, SerializedObject obj)
    {
        SerializedProperty p = obj.FindProperty(name);
        EditorGUILayout.PropertyField(p, true);
        return p;
    }

    public static void DrawTitle(string name)
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        /*
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        */

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 20;
        titleStyle.alignment = TextAnchor.MiddleCenter;
#if UNITY_PRO_LICENSE
        titleStyle.normal.textColor = Color.white;
#endif
        EditorGUILayout.LabelField(name, titleStyle);
        /*
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        */

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    public static void CustomMinMaxSlider(ref float min, ref float max, float minLimit, float maxLimit, string name, UnityEngine.Object record)
    {
        //EditorGUILayout.BeginVertical("box");

        Undo.RecordObject(record, "Slider");
        EditorGUILayout.LabelField(name + ":");
        EditorGUILayout.BeginHorizontal();
        min = EditorGUILayout.FloatField(min, GUILayout.MaxWidth(80));
        GUILayout.FlexibleSpace();
        max = EditorGUILayout.FloatField(max, GUILayout.MaxWidth(80));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref min, ref max, minLimit, maxLimit);

        //EditorGUILayout.EndVertical();
    }

    public static Color RemoveButtonColor()
    {
        Color col = new Color(1, .44f, .5f, 1f);
        return col;
    }

    public static Color AddButtonColor()
    {
        Color col = new Color(.58f, .85f, 1f, 1f);
        return col;
    }

    public static bool RemoveButton()
    {
        Color defColor = GUI.color;
        GUI.color = RemoveButtonColor();
        bool button = GUILayout.Button("X", GUILayout.MaxWidth(50));
        GUI.color = defColor;
        return button;
    }

    public static bool AddButton()
    {
        return AddButton("+", 80, AddButtonColor());
    }

    public static bool AddButton(string name, float width, Color col)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Color defColor = GUI.color;
        GUI.color = col;
        bool button = GUILayout.Button(name, GUILayout.MaxWidth(width));
        GUI.color = defColor;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        return button;
    }

    #region Property Drawer

    public static Rect GetPropertyRect(int index, Rect position)
    {
        Rect rect = new Rect(position.position + LineOffsetVertical(index), RectSize());
        return rect;
    }

    public static Rect GetPropertyRect(int index, Rect position, float offset, float widthMul)
    {
        Rect rect = new Rect(position.position + LineOffsetVertical(index) + LineOffsetHorizontal(offset), RectSize(widthMul, offset > 0 ? 30 : 5));
        return rect;
    }

    public static Vector2 LineOffsetVertical(int index)
    {
        return Vector2.up * index * EditorGUIUtility.singleLineHeight + new Vector2(0, EditorGUIUtility.standardVerticalSpacing);
    }

    public static Vector2 LineOffsetHorizontal(float mul)
    {
        return Vector2.right * EditorGUIUtility.currentViewWidth * mul;
    }

    public static Vector2 RectSize()
    {
        return RectSize(1, 0);
    }

    public static Vector2 RectSize(float widthMul, int offset)
    {
        return new Vector2(EditorGUIUtility.currentViewWidth * widthMul - offset, EditorGUIUtility.singleLineHeight);
    }

    public static void QuickSerializeRelativeGUI(string name, SerializedProperty property, Rect position, int index, float offset, float widthMul)
    {
        Rect rect = GetPropertyRect(index, position, offset, widthMul);
        QuickSerializeRelativeGUI(name, property, rect);
    }

    public static SerializedProperty QuickSerializeRelativeGUI(string name, SerializedProperty property, Rect position, int index)
    {
        Rect rect = GetPropertyRect(index, position);
        SerializedProperty prop = property.FindPropertyRelative(name);
        EditorGUI.PropertyField(rect, prop);
        return prop;
    }

    public static void QuickSerializeRelativeGUI(string name, SerializedProperty property, Rect rect)
    {
        SerializedProperty prop = property.FindPropertyRelative(name);
        EditorGUI.PropertyField(rect, prop);
    }

    public static float PropertyHeight(int count)
    {
        return EditorGUIUtility.singleLineHeight * count;
    }

    #endregion

    public static void DrawIcon(GameObject gameObject, int idx)
    {
        var largeIcons = GetTextures("sv_label_", string.Empty, 0, 8);
        var icon = largeIcons[idx];
        var egu = typeof(EditorGUIUtility);
        var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        var args = new object[] { gameObject, icon.image };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
        setIcon.Invoke(null, args);
    }

    public static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
    {
        GUIContent[] array = new GUIContent[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
        }
        return array;
    }

    public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
    {
        property = property.Copy();
        var nextElement = property.Copy();
        bool hasNextElement = nextElement.NextVisible(false);
        if (!hasNextElement)
        {
            nextElement = null;
        }

        property.NextVisible(true);
        while (true)
        {
            if ((SerializedProperty.EqualContents(property, nextElement)))
            {
                yield break;
            }

            yield return property;

            bool hasNext = property.NextVisible(false);
            if (!hasNext)
            {
                break;
            }
        }
    }

    public static UnityEngine.Object[] DropAreaGUI(string desc)
    {
        Color def = GUI.color; ;
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, desc);

        switch (evt.type)
        {
            default:
                return null;

            case EventType.DragUpdated:
                if (drop_area.Contains(evt.mousePosition))
                {
                    GUI.color = Color.red;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                return null;


            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return null;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    return DragAndDrop.objectReferences;
                }
                break;
        }

        GUI.color = def;
        return null;
    }

    #region GL   

    public static void DrawLine(Vector3 start, Vector3 end, Color col)
    {
        GL.Begin(GL.LINES);

        GL.Color(col);
        GL.Vertex(start);
        GL.Vertex(end);

        GL.End();
    }

    public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);

        GL.Vertex(a);
        GL.Vertex(b);
        GL.Vertex(c);

        GL.End();
    }

    public static void DrawSquare(Vector3 pos, float size, Color color)
    {
        GL.Begin(GL.QUADS);
        GL.Color(color);

        Vector3 bottomLeft = new Vector3(pos.x - size, pos.y - size, 0f);
        Vector3 bottomRight = new Vector3(pos.x + size, pos.y - size, 0f);
        Vector3 topLeft = new Vector3(pos.x - size, pos.y + size, 0f);
        Vector3 topRight = new Vector3(pos.x + size, pos.y + size, 0f);

        GL.Vertex(bottomLeft);
        GL.Vertex(bottomRight);
        GL.Vertex(topRight);
        GL.Vertex(topLeft);

        GL.End();
    }

    #endregion
}
