using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// Adds button to apply colors from the color palette
// Shows warning to remember to apply color after changes
[CustomEditor(typeof(ColorPalette))]
public class ColorPaletteEditor : Editor {

	public override void OnInspectorGUI() {
		// Show info box to remember to apply colors after any change in color palette
		EditorGUILayout.HelpBox("Please, remember to apply colors after any change in color palette, e.g. modifying color values, creating a new palette and moving it to Resources/, etc.", MessageType.Info);
		// Draw everything as usual
		base.OnInspectorGUI();
		// Add button to apply color palette
		if (GUILayout.Button("Apply colors")) {
			ColorPaletteLinks.instance.ApplyColors(serializedObject.targetObject as ColorPalette);
		}
	}

}
