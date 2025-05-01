using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// Displays error when the spell identifies is empty
[CustomEditor(typeof(SpellEffectColorInitializer))]
public class SpellEffectColorInitializerEditor : Editor {

	public override void OnInspectorGUI() {
		// Display error if the spell identifier is empty
		SerializedProperty identifierProperty = serializedObject.FindProperty("spellIdentifier");
		if (string.IsNullOrEmpty(identifierProperty.stringValue)) {
			EditorGUILayout.HelpBox("Please make sure the spell identifier field is not empty.", MessageType.Error);
		}
		// Draw the rest
		base.OnInspectorGUI();
	}

}
