using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


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



// Inspired by PaletteObject from Colorlink project (https://github.com/leth4/Colorlink/blob/main/Editor/PaletteObject.cs)
[FilePath("/ProjectSettings/ColorPaletteLinks.asset", FilePathAttribute.Location.ProjectFolder)]
public class ColorPaletteLinks : ScriptableSingleton<ColorPaletteLinks> {

	public List<ColorGroup> colorGroups = new List<ColorGroup>(); // Dictionary would be nicer but it is not serializable

	private ColorPalette currentlyUsedColorPalette = null;

	public bool HasColorLinked(ColorFromPalette color, string guid, string propertyPath) {
		foreach (var colorGroup in colorGroups) {
			if (colorGroup.color == color && colorGroup.HasProperty(guid, propertyPath)) return true;
		}
		return false;
	}

	public bool HasAnyColorLinked(string guid, string propertyPath) {
		foreach (var colorGroup in colorGroups)
			if (colorGroup.HasProperty(guid, propertyPath)) return true;
		return false;
	}

	public ColorFromPalette GetLinkedColor(string guid, string propertyPath) {
		foreach (var colorGroup in colorGroups)
			if (colorGroup.HasProperty(guid, propertyPath)) return colorGroup.color;
		return ColorFromPalette.None;
	}

	public void AddProperty(ColorFromPalette color, string guid, string propertyPath, ColorProperty.Type objectType) {
		// Add property to the color, remove it from any other
		foreach (var colorGroup in colorGroups) {
			if (colorGroup.color == color) colorGroup.AddProperty(guid, propertyPath, objectType);
			else colorGroup.RemoveProperty(guid, propertyPath);
		}
		Save(true);
	}

	public void RemoveProperty(string guid, string propertyPath) {
		// Remove property
		foreach (var colorGroup in colorGroups) {
			colorGroup.RemoveProperty(guid, propertyPath);
		}
		Save(true);
	}

	// Applies all linked colors in all the scenes (while opening them and saving them), while removing any invalid property
	public void ApplyColors(ColorPalette colorPalette = null) {
		InitializeColorGroups(); // make sure all color groups are present

		if (colorPalette != null) currentlyUsedColorPalette = colorPalette;
		else currentlyUsedColorPalette = ColorPalette.Instance;

		var initialScenePath = EditorSceneManager.GetActiveScene().path;
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), initialScenePath);
		// Update all linked color properties
		foreach (var colorGroup in colorGroups) {
			for (int i = colorGroup.properties.Count - 1; i >= 0; i--) {
				if (!TryApplyColorToColorProperty(colorGroup.color, colorGroup.properties[i]))
					colorGroup.properties.RemoveAt(i); // remove invalid properties
			}
		}
		// Handle prefabs
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), EditorSceneManager.GetActiveScene().path);
		if (PrefabStageUtility.GetCurrentPrefabStage() != null) { // prefab is being edited
			var prefabPath = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
			StageUtility.GoBackToPreviousStage();
			AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
		}
		// Restore initial scene
		else EditorSceneManager.OpenScene(initialScenePath);

		currentlyUsedColorPalette = null;
	}

	// Returns false if applying color failed (e.g. because the property is invalid)
	private bool TryApplyColorToColorProperty(ColorFromPalette color, ColorProperty property) {
		if (GlobalObjectId.TryParse(property.guid, out GlobalObjectId guidObject)) {
			if (property.objectType == ColorProperty.Type.GameObject)
				EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guidObject.assetGUID));

			var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(guidObject);
			if (obj == null) return false;

			var serializedObject = new UnityEditor.SerializedObject(guidObject.identifierType == 3 ? obj : (Component)obj);
			var serializedProperty = serializedObject.FindProperty(property.propertyPath);

			if (serializedProperty == null) return false;

			serializedProperty.colorValue = currentlyUsedColorPalette.GetColor(color).WithA(serializedProperty.colorValue.a); // keep original alpha

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(serializedObject.targetObject);

			if (property.objectType == ColorProperty.Type.GameObject)
				EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), AssetDatabase.GUIDToAssetPath(guidObject.assetGUID));

			return true;
		} else {
			return false;
		}
	}

	private bool HasColorGroup(ColorFromPalette color) {
		foreach (var colorGroup in colorGroups) {
			if (colorGroup.color == color) return true;
		}
		return false;
	}

	private void InitializeColorGroups() {
		// Initialize ColorGroup for each color
		foreach (ColorFromPalette color in Enum.GetValues(typeof(ColorFromPalette))) {
			if (!HasColorGroup(color)) {
				colorGroups.Add(new ColorGroup(color));
			}
		}
	}

	private void Awake() {
		InitializeColorGroups();
	}

}


// Inspired by ColorGroup from Colorlink project (https://github.com/leth4/Colorlink/blob/main/Editor/ColorGroup.cs)
[System.Serializable]
public class ColorGroup {

	public ColorFromPalette color;
	public List<ColorProperty> properties = new List<ColorProperty>();

	public ColorGroup(ColorFromPalette color) {
		this.color = color;
	}

	public bool HasProperty(string guid, string propertyPath) {
		foreach (var property in properties)
			if (property.guid == guid && property.propertyPath == propertyPath) return true;
		return false;
	}

	public void AddProperty(string guid, string propertyPath, ColorProperty.Type objectType) {
		if (!HasProperty(guid, propertyPath)) {
			properties.Add(new ColorProperty(guid, propertyPath, objectType));
		}
	}

	public void RemoveProperty(string guid, string propertyPath) {
		for (int i = properties.Count - 1; i >= 0; i--) {
			if (properties[i].guid == guid && properties[i].propertyPath == propertyPath) {
				properties.RemoveAt(i);
			}
		}
	}

}


// Inspired by ColorProperty from Colorlink project (https://github.com/leth4/Colorlink/blob/main/Editor/ColorGroup.cs)
[System.Serializable]
public class ColorProperty {
	public string guid;
	public string propertyPath;
	public Type objectType;

	public ColorProperty(string guid, string propertyPath, Type type) {
		this.guid = guid;
		this.propertyPath = propertyPath;
		this.objectType = type;
	}

	public enum Type { 
		GameObject,
		Asset
	}
}