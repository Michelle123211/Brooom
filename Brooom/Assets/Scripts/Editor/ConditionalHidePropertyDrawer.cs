using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


// TODO: Right now this attribute doesn't work properly for fields/properties which are arrays
// Visibility of a decorate property in the Inspector is determined by value of another property (set in attribute's parameter)
[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Find out if the property should be visible, or not
        ConditionalHideAttribute attributeTyped = (ConditionalHideAttribute)attribute;
        bool enabled = GetResult(attributeTyped, property);
        // Render the property, if necessary
        bool wasGUIEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (enabled) {
            EditorGUI.PropertyField(position, property, label, true);
        }
        GUI.enabled = wasGUIEnabled; // restore back
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        ConditionalHideAttribute attributeTyped = (ConditionalHideAttribute)attribute;
        bool enabled = GetResult(attributeTyped, property);

        if (enabled) {
            return EditorGUI.GetPropertyHeight(property, label);
        } else {
            // The property is not being drawn, we want to undo the spacing added before and after the property
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    // Checks if the property should be visible, or not (based on parameters of the attribute)
    private bool GetResult(ConditionalHideAttribute attributeTyped, SerializedProperty property) {
        bool enabled = true;

        string propertyPath = property.propertyPath; // property path of the property the attribute is applied to
        // Get path to the property containing condition
        string[] tokens = propertyPath.Split('.');
        int length = tokens.Length;
        for (int i = 0; i < tokens.Length; ++i) {
            if (tokens[i] == "Array") length = i;
        }
        string[] newTokens = new string[length];
        for (int i = 0; i < length - 1; ++i) {
            newTokens[i] = tokens[i];
        }
        newTokens[newTokens.Length - 1] = attributeTyped.conditionField; // replace last token with the name of the condition field
        string conditionPath = string.Join(".", newTokens);
        SerializedProperty conditionField = property.serializedObject.FindProperty(conditionPath);
        // Check value in the condition field
        if (conditionField == null) {
            conditionField = property.serializedObject.FindProperty(attributeTyped.conditionField);
        }
        if (conditionField == null) {
            Debug.LogWarning($"Attempting to use a ConditionalHideAttribute but no matching conditionField found ({attributeTyped.conditionField}).");
        } else {
            enabled = conditionField.boolValue;
            if (attributeTyped.inverse) enabled = !enabled;
        }

        return enabled;
    }
}
