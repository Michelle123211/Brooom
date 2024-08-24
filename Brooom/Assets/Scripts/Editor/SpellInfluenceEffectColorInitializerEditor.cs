using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpellInfluenceEffectColorInitializer))]
public class SpellInfluenceEffectColorInitializerEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		// Display error if the spell identifier is empty
		SerializedProperty identifierProperty = serializedObject.FindProperty("spellIdentifier");
		if (string.IsNullOrEmpty(identifierProperty.stringValue)) {
			EditorGUILayout.HelpBox("Please make sure the spell identifier field is not empty.", MessageType.Error);
		}
	}

}
