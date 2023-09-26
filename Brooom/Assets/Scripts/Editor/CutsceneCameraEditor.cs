using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CutsceneCamera))]
public class CutsceneCameraEditor : Editor {

	public override void OnInspectorGUI() {
		// Display the behaviour type dropdown
		SerializedProperty behaviourTypeProperty = serializedObject.FindProperty("behaviourType");
		EditorGUILayout.PropertyField(behaviourTypeProperty);

		// Check the type of cutscene camera behaviour
		int enumIndex = behaviourTypeProperty.enumValueIndex;
		CutsceneCameraBehaviour behaviourType = (CutsceneCameraBehaviour)enumIndex;

		// Display corresponding fields
		DisplayProperties(behaviourType);

		// Save all changes made to the Inspector
		serializedObject.ApplyModifiedProperties();
	}

	private void DisplayProperties(CutsceneCameraBehaviour behaviourType) {
		// Target object
		if (behaviourType == CutsceneCameraBehaviour.StaticRelativeToObject ||
			behaviourType == CutsceneCameraBehaviour.MovingRelativeToObject ||
			behaviourType == CutsceneCameraBehaviour.FollowingObject)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
		// Position
		EditorGUILayout.PropertyField(serializedObject.FindProperty("position"));
		// Rotation
		if (behaviourType != CutsceneCameraBehaviour.FollowingObject)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotation"));
		// Offset of the look at point
		if (behaviourType == CutsceneCameraBehaviour.FollowingObject)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("lookAtOffset"));
		// Movement direction and speed
		if (behaviourType == CutsceneCameraBehaviour.Moving ||
			behaviourType == CutsceneCameraBehaviour.MovingRelativeToObject) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
		}
	}

}