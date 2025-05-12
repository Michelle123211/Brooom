using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// Displays warning when there is no visual effect assigned
[CustomEditor(typeof(RacerAffectingSpellEffect), true)]
public class RacerAffectingSpellEffectEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		// Show a warning that a visual effect should be assigned for the target racer to see they are under influence of this spell
		SerializedProperty spellInfluenceEffectProperty = serializedObject.FindProperty("spellInfluenceVisualEffectPrefab");
		if (spellInfluenceEffectProperty.objectReferenceValue == null) {
			Debug.Log("Null.");
			EditorGUILayout.HelpBox("Consider asigning the 'Spell Influence Visual Effect Prefab' (representing a visual effect displayed around the target racer), so the target racer can easily see they are under influence of this spell.", MessageType.Warning);
		} else {
			Debug.Log("Not null.");
		}
	}

}
