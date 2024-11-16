using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;


// ------------------------------------------------------------------------------------------------------------------------------------------------------------

// The following code is a modification of Colorlink project, published on https://github.com/leth4/Colorlink, under the following license.

// ------------------------------------------------------------------------------------------------------------------------------------------------------------

// MIT License

// Copyright (c) 2023 Eugene Radaev

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// ------------------------------------------------------------------------------------------------------------------------------------------------------------



// Inspired by ColorPropertyHandler from Colorlink project (https://github.com/leth4/Colorlink/blob/main/Editor/ColorPropertyHandler.cs)
public static class ColorPropertyEditor {

	#region Context Menu
	[InitializeOnLoadMethod]
	private static void Start() {
		EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
	}

	private static void OnPropertyContextMenu(GenericMenu genericMenu, SerializedProperty serializedProperty) {
		if (serializedProperty.propertyType != SerializedPropertyType.Color) return;

		SerializedProperty propertyCopy = serializedProperty.Copy();
		if (!(propertyCopy.serializedObject.targetObject is Component) && !(propertyCopy.serializedObject.targetObject is ScriptableObject)) return;

		GlobalObjectId guid = GlobalObjectId.GetGlobalObjectIdSlow(propertyCopy.serializedObject.targetObject);
		string guidStr = VerifyGUIDForPrefabStage(guid.ToString());
		string propertyPath = propertyCopy.propertyPath;
		ColorProperty.Type objectType = (guid.identifierType == 2 && !PrefabStageUtility.GetCurrentPrefabStage()) ? ColorProperty.Type.GameObject : ColorProperty.Type.Asset;

		// Show all possible colors from color palette
		foreach (ColorFromPalette color in Enum.GetValues(typeof(ColorFromPalette))) {
			if (color == ColorFromPalette.None) continue;
			string[] tokens = color.ToString().Split('_');
			genericMenu.AddItem(
				new GUIContent($"Link from Color Palette / {tokens[0]} / {tokens[1]}"),
				ColorPaletteLinks.instance.HasColorLinked(color, guidStr, propertyPath),
				() => {
					// Add color link and apply color
					ColorPaletteLinks.instance.AddProperty(color, guidStr, propertyPath, objectType);
					propertyCopy.colorValue = ColorPalette.Instance.GetColor(color).WithA(propertyCopy.colorValue.a);
					propertyCopy.serializedObject.ApplyModifiedProperties();
					EditorUtility.SetDirty(propertyCopy.serializedObject.targetObject);
				}
			);
		}
		// Add option to easily unlink color
		genericMenu.AddItem(
			new GUIContent($"Unlink from Color Palette"),
			false,
			() => {
				// Remove color link
				ColorPaletteLinks.instance.RemoveProperty(guidStr, propertyPath);
			}
		);
	}

	public static string VerifyGUIDForPrefabStage(string guid) {
		if (PrefabStageUtility.GetCurrentPrefabStage() == null) return guid;
		// Handle editing a prefab
		var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabStageUtility.GetCurrentPrefabStage().assetPath);
		var prefabGUID = GlobalObjectId.GetGlobalObjectIdSlow(prefabAsset);
		return guid.ToString().Replace("-2-", "-1-").Replace("00000000000000000000000000000000", prefabGUID.assetGUID.ToString());
	}
	#endregion

	#region Property Drawer
	[CustomPropertyDrawer(typeof(Color))]
	public class ColorPropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label) {
			EditorGUI.BeginProperty(position, label, serializedProperty);

			// Color property
			Rect colorPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(colorPosition, serializedProperty, label);

			// Linked color name
			GlobalObjectId guid = GlobalObjectId.GetGlobalObjectIdSlow(serializedProperty.serializedObject.targetObject);
			string guidStr = VerifyGUIDForPrefabStage(guid.ToString());
			ColorFromPalette linkedColor = ColorPaletteLinks.instance.GetLinkedColor(guidStr, serializedProperty.propertyPath);
			if (linkedColor != ColorFromPalette.None) {
				Rect pathPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.LabelField(pathPosition, "<color=red> = <b>LINKED from palette</b>: " + linkedColor.ToString().Replace('_', '/') + "</color>", new GUIStyle { richText = true });
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			GlobalObjectId guid = GlobalObjectId.GetGlobalObjectIdSlow(property.serializedObject.targetObject);
			string guidStr = VerifyGUIDForPrefabStage(guid.ToString());
			if (ColorPaletteLinks.instance.HasAnyColorLinked(guidStr, property.propertyPath)) return EditorGUIUtility.singleLineHeight * 2;
			else return EditorGUIUtility.singleLineHeight;
		}

	}
	#endregion

}
