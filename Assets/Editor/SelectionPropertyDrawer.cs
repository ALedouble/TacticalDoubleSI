using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TileArea))]
public class SelectionPropertyDrawer : PropertyDrawer {


	//Sizes
   	Vector2 intSize = new Vector2(30,30);
	Vector2 labelSize = new Vector2 (100, 15);

    private bool initialized = false;
    bool[,] areaBuffer;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
		return 210; //Temporary
	}


    private void Init(SerializedProperty property)
    {
        
        initialized = true;

        int size = property.FindPropertyRelative("size").intValue;
        areaBuffer = new bool[10, 10];
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
        if (!initialized)
            Init(property);

        EditorGUI.BeginProperty (position, label, property);

		//Draw small label with variable name
		EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

        GUIStyle style = new GUIStyle(GUIStyle.none);
        style.alignment = TextAnchor.MiddleRight;
        style.padding = new RectOffset(0, 1, 0, 0);

        //Store value of x and y in SelectionProperty.cs
        int x = property.FindPropertyRelative ("size").intValue;
        int y = x;

        SerializedProperty sizeProperty = property.FindPropertyRelative("size");

        Rect rectL = new Rect(position.min + new Vector2(0, 20), labelSize);
        Rect rectX = new Rect(position.min + new Vector2(120, 20), intSize);
        Rect rectY = new Rect(position.min + new Vector2(150, 20), intSize);

        EditorGUI.PropertyField(rectY, sizeProperty);


       


        //Calculate position of the grid and anchored
        float positionAnchored = position.xMax - (x + 1) * 65;
		if (positionAnchored < position.xMin) positionAnchored = position.xMin;

		Vector2 start = new Vector2 (positionAnchored, position.yMin + 50f);

		//Draw global square with all value.
		Vector2 cellSize = new Vector2(31, 31); 
        EditorGUI.DrawRect (new Rect (start, new Vector2 (cellSize.x * x + 1, cellSize.y * y + 1)), Color.black);

		start += Vector2.one;

		int n = 0;
		for (int i = 0; i < x; i++) {
            for (int j = 0; j < x; j++) {
                //Cell Drawing
                Vector2 offset = new Vector2(cellSize.x * j, cellSize.y * i);
                Rect rectPos = new Rect(start + offset, intSize);
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
                    EditorGUI.DrawRect(rectPos, areaBuffer[i, j] ? Color.green :  new Color(0.8f, 0.8f, 0.8f));
                }

                n++;
			}
		}


        CustomEditorUtils.RepaintInspector(property.serializedObject);
        EditorGUI.EndProperty ();
	}
}