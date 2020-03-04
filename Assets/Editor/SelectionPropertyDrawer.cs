using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TileArea))]
public class SelectionPropertyDrawer : PropertyDrawer {


	//Sizes
   	Vector2 intSize = new Vector2(30,30);

    private bool initialized = false;
    bool[,] areaBuffer;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        return property.isExpanded ? 50 + property.FindPropertyRelative("size").intValue * 31 : 16;
    }

    private void Init(SerializedProperty property)
    {
        
        initialized = true;
        
        int size = property.FindPropertyRelative("size").intValue;
        areaBuffer = new bool[20, 20];
        List<Vector2Int> area = CustomEditorUtils.PropertyToVector2Int(property);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {  
                if(area.Contains(new Vector2Int(x, y)))
                {
                    areaBuffer[x, y] = true;
                }
            }
        }
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = 16;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label); 
        if (property.isExpanded)
        {
            if (!initialized)
                Init(property);


            GUIStyle style = new GUIStyle(GUIStyle.none);
            style.alignment = TextAnchor.MiddleRight;
            style.padding = new RectOffset(0, 10, 0, 0);

            SerializedProperty sizeProperty = property.FindPropertyRelative("size");

            Rect rectAdd = new Rect(position.min + new Vector2(0, 20), intSize);
            Rect rectDelete = new Rect(position.min + new Vector2(40, 20), intSize);


            if (GUI.Button(rectAdd, "+") && sizeProperty.intValue < 19)
            {
                sizeProperty.intValue++;
            }

            if (GUI.Button(rectDelete, "-") && sizeProperty.intValue > 3)
            {
                sizeProperty.intValue -= 2;

                areaBuffer = new bool[20, 20];
            }


            if (sizeProperty.intValue % 2 == 0)
            {
                sizeProperty.intValue++;
            }

            //EditorGUI.PropertyField(rectL, sizeProperty);

            int x = property.FindPropertyRelative("size").intValue;
            int y = x;

            //Calculate position of the grid and anchored
            float positionAnchored = position.xMax - (x + 10) * 45 + 200;
            if (positionAnchored < position.xMax) positionAnchored = position.xMin;

            Vector2 start = new Vector2(positionAnchored, position.yMin + 50f);

            //Draw global square with all value.
            Vector2 cellSize = new Vector2(31, 31);
            EditorGUI.DrawRect(new Rect(start, new Vector2(cellSize.x * x, cellSize.y * y + 1)), Color.black);

            start += Vector2.one;

            int n = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    //Cell Drawing
                    Vector2 offset = new Vector2(cellSize.x * j, cellSize.y * i);

                    Vector2 centerPos = new Vector2(cellSize.x * Mathf.Round(x / 2), cellSize.y * Mathf.Round(x / 2));
                    Rect rectPos = new Rect(start + offset, intSize);
                    Rect centerRect = new Rect(start + centerPos, intSize);

                    EditorGUI.DrawRect(centerRect, new Color(0.6f, 0.6f, 0.6f));

                    GUI.color = Color.white;

                    if (rectPos.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(rectPos, Color.red);

                        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                        {
                            if (areaBuffer[i, j])
                            {
                                areaBuffer[i, j] = false;
                                
                            }
                            else
                            {
                                areaBuffer[i, j] = true;
                            }

                            CustomEditorUtils.FillPropertyWithVector2Int(property, areaBuffer);
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        EditorGUI.DrawRect(rectPos, areaBuffer[i, j] ? Color.green : new Color(0.8f, 0.8f, 0.8f));
                    }

                    n++;
                }
            }
        }
		



        CustomEditorUtils.RepaintInspector(property.serializedObject);
        //EditorGUI.EndProperty ();
	}
}