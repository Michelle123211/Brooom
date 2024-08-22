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
        // Get serialized properties
        SerializedProperty identifierProperty = serializedObject.FindProperty("identifier");
        SerializedProperty nameProperty = serializedObject.FindProperty("spellName");
        SerializedProperty baseColorProperty = serializedObject.FindProperty("baseColor");
        SerializedProperty emissionColorProperty = serializedObject.FindProperty("emissionColor");
        SerializedProperty iconProperty = serializedObject.FindProperty("icon");
        SerializedProperty coinsProperty = serializedObject.FindProperty("coinsCost");
        SerializedProperty manaProperty = serializedObject.FindProperty("manaCost");
        SerializedProperty cooldownProperty = serializedObject.FindProperty("cooldown");
        SerializedProperty categoryProperty = serializedObject.FindProperty("category");
        // Draw properties
        EditorGUILayout.LabelField("Basic properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(identifierProperty);
        EditorGUILayout.PropertyField(nameProperty);
        EditorGUILayout.PropertyField(baseColorProperty);
        EditorGUILayout.PropertyField(emissionColorProperty);
        EditorGUILayout.PropertyField(iconProperty);
        EditorGUILayout.PropertyField(coinsProperty);
        EditorGUILayout.PropertyField(manaProperty);
        EditorGUILayout.PropertyField(cooldownProperty);
        EditorGUILayout.PropertyField(categoryProperty);
        // Display errors/warning if some of the properties are invalid
        if (string.IsNullOrEmpty(identifierProperty.stringValue) || string.IsNullOrEmpty(nameProperty.stringValue) ||
            iconProperty.objectReferenceValue == null || coinsProperty.intValue <= 0 || manaProperty.intValue <= 0 ||
            cooldownProperty.floatValue <= 0 || (SpellCategory)categoryProperty.enumValueIndex == SpellCategory.Invalid) {
            EditorGUILayout.HelpBox("Please make sure all fields have valid values.", MessageType.Error);
        }
    }

    private void DisplayTargetProperties() {
        EditorGUILayout.LabelField("Spell target", EditorStyles.boldLabel);
        // Target type dropdown
        SerializedProperty targetTypeProperty = serializedObject.FindProperty("targetType");
        EditorGUILayout.PropertyField(targetTypeProperty);
        // Target tag (only if target type is object)
        SerializedProperty tagProperty = serializedObject.FindProperty("spellTargetTag");
        SpellTargetType targetType = (SpellTargetType)targetTypeProperty.enumValueIndex;
        if (targetType == SpellTargetType.Object) {
            tagProperty.stringValue = EditorGUILayout.TagField(new GUIContent("Target object tag", tagProperty.tooltip), tagProperty.stringValue);
            serializedObject.ApplyModifiedProperties();
        }
        //EditorGUILayout.LabelField(serializedObject.FindProperty("<SpellTargetTag>k__BackingField").stringValue); // for debug
        
        // Display errors/warning if some of the properties are invalid
        if (targetType == SpellTargetType.Invalid || (targetType == SpellTargetType.Object && string.IsNullOrEmpty(tagProperty.stringValue))) {
            EditorGUILayout.HelpBox("Please make sure all fields have valid values.", MessageType.Error);
        }
    }

    private void DisplaySpellEffectProperties() {
        EditorGUILayout.LabelField("Spell effect", EditorStyles.boldLabel);
        SerializedProperty effectControllerProperty = serializedObject.FindProperty("effectControllerPrefab");
        EditorGUILayout.PropertyField(effectControllerProperty);
        if (effectControllerProperty.objectReferenceValue == null) {
            EditorGUILayout.HelpBox("A SpellEffectController component must be assigned to the 'Effect Controller Prefab' field.", MessageType.Error);
        }
    }

}
