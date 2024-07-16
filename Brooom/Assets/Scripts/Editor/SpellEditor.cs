using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spell))]
public class SpellEditor : Editor {

	public override void OnInspectorGUI() {
        // Display all relevant properties
        DisplayBasicProperties();
        EditorGUILayout.Separator();
        DisplayTargetProperties();
        EditorGUILayout.Separator();
        DisplaySpellEffectProperties();
        // Save all changes made to the Inspector
        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayBasicProperties() {
        EditorGUILayout.LabelField("Basic properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("identifier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spellName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("coinsCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("manaCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("category"));
    }

    private void DisplayTargetProperties() {
        EditorGUILayout.LabelField("Spell target", EditorStyles.boldLabel);
        // Target type dropdown
        SerializedProperty targetTypeProperty = serializedObject.FindProperty("targetType");
        EditorGUILayout.PropertyField(targetTypeProperty);
        // Target tag (only if target type is object)
        SpellTargetType targetType = (SpellTargetType)targetTypeProperty.enumValueIndex;
        if (targetType == SpellTargetType.Object) {
            SerializedProperty tagProperty = serializedObject.FindProperty("spellTargetTag");
            tagProperty.stringValue = EditorGUILayout.TagField("Target object tag", tagProperty.stringValue);
            serializedObject.ApplyModifiedProperties();
        }
        //EditorGUILayout.LabelField(serializedObject.FindProperty("spellTargetTag").stringValue); // for debug
    }

    private void DisplaySpellEffectProperties() {
        EditorGUILayout.LabelField("Spell effect", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effectControllerPrefab"));
    }

}
