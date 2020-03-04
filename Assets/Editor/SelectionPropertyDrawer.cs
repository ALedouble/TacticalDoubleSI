using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TileArea))]
public class SelectionPropertyDrawer : PropertyDrawer {


	//Sizes
   	Vector2 intSize = new Vector2(30,30);
	Vector2 labelSize = new Vector2 (100, 15);
    
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
		return 200; //Temporary
	}

    private bool initialized = false;
    bool[,] areaBuffer;
    private void Init(SerializedProperty property)
    {
        initialized = true;

        int size = property.FindPropertyRelative("size").intValue;
        areaBuffer = new bool[size, size];

        // TODO : fill bool 2d array with area values for faster lookup time
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        Texture boxText = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Textures/T_Square_D.jpg");

		EditorGUI.BeginProperty (position, label, property);

		//Draw small label with variable name
		EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

        GUIStyle style = new GUIStyle(GUIStyle.none);
        style.alignment = TextAnchor.MiddleRight;
        style.padding = new RectOffset(0, 1, 0, 0);


        //Store value of x and y in SelectionProperty.cs
        int x = property.FindPropertyRelative ("size").intValue;
        int y = x;


        //Calculate position of the grid and anchored
        float positionAnchored = position.xMax - (x + 1) * 65;
		if (positionAnchored < position.xMin) positionAnchored = position.xMin;

		Vector2 start = new Vector2 (positionAnchored, position.yMin + 20f);

		//Draw global square with all value.
		Vector2 cellSize = new Vector2(31, 31); 
        EditorGUI.DrawRect (new Rect (start, new Vector2 (cellSize.x * x + 1, cellSize.y * y + 1)), Color.black);

        List<Vector2Int> area = CustomEditorUtils.PropertyToVector2Int(property);

		start += Vector2.one;
		int n = 0;
		for (int i = 0; i < y; i++) {
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
                        if (area.Contains(new Vector2Int(j, i)))
                        {
                            area.Remove(new Vector2Int(j, i));
                        }
                        else
                        {
                            area.Add(new Vector2Int(j, i));
                        }
                        CustomEditorUtils.FillPropertyWithVector2Int(property, area);
                        property.serializedObject.ApplyModifiedProperties();

                    }

                }
                else
                {
                    // Remove contains and replace with areaBuffer lookup
                    EditorGUI.DrawRect(rectPos, area.Contains(new Vector2Int(j, i)) ? Color.green :  new Color(0.8f, 0.8f, 0.8f));
                }

                n++;
			}
		}


        CustomEditorUtils.RepaintInspector(property.serializedObject);
        EditorGUI.EndProperty ();
	}
}