using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;


// Custom menu items to create predefined objects or assets
public class GameObjectMenuItems : MonoBehaviour {

	// Adds a menu item to create a GameObject with LocalizedTextMeshProUI component as well as TextMeshProUGUI
	[MenuItem("GameObject/Localization/Localized TextMeshPro", priority = 10)]
	public static void CreateLocalizedTextMeshProUI(MenuCommand menuCommand) {
		GameObject go = new GameObject("LocalizedTextMeshPro");
		TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
		LocalizedTextMeshProUI localizedText = go.AddComponent<LocalizedTextMeshProUI>();
		localizedText.textLabel = text;
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	// Adds a menu item to create a spell prefab containing a few basic components (directly in Resources/Spells/ folder)
	[MenuItem("Assets/Create/Spell System/Spell", priority = 10)]
	public static void CreateSpellPrefab(MenuCommand menuCommand) {
		// Get an asset name
		string name = AssetNameDialogEditor.ShowDialog();
		if (string.IsNullOrEmpty(name)) // Asset name dialog was cancelled
			return;
		// Get the original spell prefab
		string prefabPath = "Assets/Prefabs/Spells/SpellPrefab.prefab";
		GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
		if (originalPrefab == null) Debug.Log("Prefab is null.");
		// Create an instance and save it as a prefab variant
		GameObject prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
		if (prefabInstance == null) Debug.Log("Prefab instance is null.");
		string assetPath = $"Assets/Resources/Spells/{name}.prefab";
		GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
		// Remove the original template
		DestroyImmediate(prefabInstance);
		// Open the newly created asset in the Project window
		ProjectWindowUtil.ShowCreatedAsset(prefabVariant);
	}
}
