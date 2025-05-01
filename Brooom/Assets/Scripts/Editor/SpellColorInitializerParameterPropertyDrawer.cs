using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// If an override of a particular color component is enabled, a slider appears next to it to select the custom value
// If it is disabled, the slider is not visible
[CustomPropertyDrawer(typeof(SpellColorInitializerParameter), true)]
public class SpellColorInitializerParameterPropertyDrawer : PropertyDrawer {

	protected int lineCount = 0;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		EditorGUI.BeginProperty(position, label, property);
		lineCount = 0;

		DrawGUIContent(position, property);

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight * lineCount;
	}

	protected void DrawGUIContent(Rect position, SerializedProperty property) {
		// Color type
		Rect colorTypePos = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		SerializedProperty colorTypeProperty = property.FindPropertyRelative("colorType");
		EditorGUI.PropertyField(colorTypePos, colorTypeProperty);
		lineCount++;
		// Component overrides
		DrawComponentOverrides(new Rect(colorTypePos.x, colorTypePos.y + EditorGUIUtility.singleLineHeight, colorTypePos.width, EditorGUIUtility.singleLineHeight), property);
	}

	private void DrawComponentOverrides(Rect position, SerializedProperty property) {
		// Component overrides
		EditorGUI.LabelField(position, "Component overrides:");
		lineCount++;
		// R - label, checkbox, data field
		Rect componentRPos = new Rect(position.x + 20, position.y + EditorGUIUtility.singleLineHeight, position.width - 20, EditorGUIUtility.singleLineHeight);
		DrawColorComponentOverride(componentRPos, property, "R", "overrideR", "customRValue");
		// G - label, checkbox, data field
		Rect componentGPos = componentRPos;
		componentGPos.y += EditorGUIUtility.singleLineHeight;
		DrawColorComponentOverride(componentGPos, property, "G", "overrideG", "customGValue");
		// B - label, checkbox, data field
		Rect componentBPos = componentGPos;
		componentBPos.y += EditorGUIUtility.singleLineHeight;
		DrawColorComponentOverride(componentBPos, property, "B", "overrideB", "customBValue");
		// A - label, checkbox, data field
		Rect componentAPos = componentBPos;
		componentAPos.y += EditorGUIUtility.singleLineHeight;
		DrawColorComponentOverride(componentAPos, property, "A", "overrideA", "customAValue");
	}

	private void DrawColorComponentOverride(Rect position, SerializedProperty property, string componentLabel, string boolPropertyName, string valuePropertyName) {
		// Label
		Rect labelPos = position;
		labelPos.width = 25;
		EditorGUI.LabelField(labelPos, componentLabel);
		// Checkbox
		Rect checkboxPos = position;
		checkboxPos.x += labelPos.width;
		checkboxPos.width = 20;
		SerializedProperty overrideProperty = property.FindPropertyRelative(boolPropertyName);
		EditorGUI.PropertyField(checkboxPos, overrideProperty, GUIContent.none);
		// Tooltip
		Rect tooltipPos = new Rect(labelPos.x, labelPos.y, labelPos.width + checkboxPos.width, labelPos.height);
		EditorGUI.LabelField(tooltipPos, new GUIContent("", overrideProperty.tooltip));
		// Data field for custom value - only if the component override is enabled
		if (overrideProperty.boolValue) {
			Rect fieldPos = position;
			float offset = labelPos.width + checkboxPos.width + 40;
			fieldPos.x += offset;
			fieldPos.width -= offset;
			SerializedProperty valueProperty = property.FindPropertyRelative(valuePropertyName);
			EditorGUI.PropertyField(fieldPos, valueProperty, GUIContent.none);
			EditorGUI.LabelField(fieldPos, new GUIContent("", valueProperty.tooltip)); // tooltip
		}

		lineCount++;
	}

}
