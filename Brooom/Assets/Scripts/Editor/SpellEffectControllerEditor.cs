using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// Displays error message when SpellEffect component is not assigned to a field
[CustomEditor(typeof(SpellEffectController))]
public class SpellEffectControllerEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		// Show an error if there is no SpellEffect component assigned
		SerializedProperty effectProperty = serializedObject.FindProperty("actualSpellEffect");
		if (effectProperty.objectReferenceValue == null)
			EditorGUILayout.HelpBox("A component derived from SpellEffect must be assigned to the 'Actual Spell Effect' field.", MessageType.Error);
	}

}
