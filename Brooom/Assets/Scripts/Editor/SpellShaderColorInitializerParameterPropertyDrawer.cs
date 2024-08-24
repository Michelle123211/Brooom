using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(SpellShaderColorInitializerParameter))]
public class SpellShaderColorInitializerParameterPropertyDrawer : SpellColorInitializerParameterPropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		lineCount = 0;

		// Shader parameter name
		Rect shaderParameterPos = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		SerializedProperty parameterNameProperty = property.FindPropertyRelative("shaderParameterName");
		EditorGUI.PropertyField(shaderParameterPos, parameterNameProperty);
		lineCount++;
		// The rest is the same as for SpellColorInitializerParameterPropertyDrawer
		Rect newPosition = shaderParameterPos;
		newPosition.y += EditorGUIUtility.singleLineHeight;
		DrawGUIContent(newPosition, property);

		EditorGUI.EndProperty();
	}

}
