using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpellTrajectoryVisualEffect))]
public class SpellTrajectoryVisualEffectEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		// Show an error if there is no SpellTrajectoryComputer component assigned
		SerializedProperty trajectoryProperty = serializedObject.FindProperty("spellTrajectory");
		if (trajectoryProperty.objectReferenceValue == null)
			EditorGUILayout.HelpBox("A component derived from SpellTrajectoryComputer must be assigned to the 'Spell Trajectory' field.", MessageType.Error);
		// Show a warning that at least some visual effects should be assigned for the spell to be visible
		SerializedProperty spellVisualProperty = serializedObject.FindProperty("spellCastVisual");
		SerializedProperty spellTrailProperty = serializedObject.FindProperty("spellCastTrail");
		SerializedProperty spellCastParticles = serializedObject.FindProperty("spellCastParticles");
		if (spellVisualProperty.objectReferenceValue == null && spellTrailProperty.objectReferenceValue == null && spellCastParticles.objectReferenceValue == null)
			EditorGUILayout.HelpBox("Consider asigning the 'Spell Cast Visual', 'Spell Cast Trail' or 'Spell Cast Particles' field, so the spell is visible when casted. Also don't forget to change 'Spell Cast Color' if necessary.", MessageType.Warning);
	}

}
