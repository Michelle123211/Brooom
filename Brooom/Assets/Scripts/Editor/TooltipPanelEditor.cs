using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TooltipPanel))]
public class TooltipPanelEditor : Editor {

	public override void OnInspectorGUI() {
		// textFields
		SerializedProperty textFieldsProperty = serializedObject.FindProperty("textFields");
		EditorGUILayout.PropertyField(textFieldsProperty);
		// isStatic
		SerializedProperty isStaticProperty = serializedObject.FindProperty("isStatic");
		EditorGUILayout.PropertyField(isStaticProperty);
		// canvasRectTransform only if isStatic == false
		if (!isStaticProperty.boolValue) {
			SerializedProperty canvasRectTransformProperty = serializedObject.FindProperty("canvasRectTransform");
			EditorGUILayout.PropertyField(canvasRectTransformProperty);
		}
		// Save all changes made to the Inspector
		serializedObject.ApplyModifiedProperties();
	}
}
