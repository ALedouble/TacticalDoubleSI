using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SelectionProperty))]
public class SelectionPropertyDrawer : PropertyDrawer {


	//Sizes
   	Vector2 intSize = new Vector2(30,15);
	Vector2 labelSize = new Vector2 (100, 15);

    

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 100; //Temporary
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		Texture boxText = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Textures/T_Carre_D.jpg");

        
		EditorGUI.BeginProperty (position, label, property);

		//Draw small label with variable name
		EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

		//Store value of x and y in SelectionProperty.cs
		int x = property.FindPropertyRelative ("x").intValue;
		int y = property.FindPropertyRelative ("y").intValue;

		//Calculate position of the grid and anchored
		SerializedProperty serArray = property.FindPropertyRelative ("sizeArray");
		float positionAnchored = position.xMax - (x + 1) * 65;
		if (positionAnchored < position.xMin) positionAnchored = position.xMin;

		Vector2 start = new Vector2 (positionAnchored, position.yMin + 20f);

		//Draw global square with all value.
		Vector2 cellSize = new Vector2 (31, 16);
		EditorGUI.DrawRect (new Rect (start, new Vector2 (cellSize.x * x + 1, cellSize.y * y + 1)), Color.black);

		start += Vector2.one;
		int n = 0;
		for (int i = 0; i < y; i++) {
			for (int j = 0; j < x; j++) {
				//Cell Drawing
				Vector2 offset = new Vector2 (cellSize.x * j, cellSize.y * i);
				Rect rectPos = new Rect (start + offset, intSize);
				GUI.color = Color.white;

				//Button at rectPosition
                if(GUI.Button(rectPos, "")){
                   GUI.color = Color.red;
				  
                }
                
				n++;
			}
		}

		EditorGUI.EndProperty ();
	}
}