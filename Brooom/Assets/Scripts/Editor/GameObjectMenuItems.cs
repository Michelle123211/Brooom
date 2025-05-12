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
		CreateSpellPrefabVariant("Assets/Prefabs/Spells/SpellPrefab.prefab");
	}

	// Adds a menu item to create a prefab for a self-cast spell, containing a few basic components (directly in Resources/Spells/ folder)
	[MenuItem("Assets/Create/Spell System/Self-cast Spell", priority = 10)]
	public static void CreateSelfCastSpellPrefab(MenuCommand menuCommand) {
		CreateSpellPrefabVariant("Assets/Prefabs/Spells/SpellSelfCastPrefab.prefab");
	}

	// Adds a menu item to create a prefab for a spell cast to a different point, containing a few basic components (directly in Resources/Spells/ folder)
	[MenuItem("Assets/Create/Spell System/Spell with Trajectory", priority = 10)]
	public static void CreateSpellWithTrajectoryPrefab(MenuCommand menuCommand) {
		CreateSpellPrefabVariant("Assets/Prefabs/Spells/SpellWithTrajectoryPrefab.prefab");
	}

	private static void CreateSpellPrefabVariant(string prefabPath) {
		// Get an asset name
		string name = AssetNameDialogEditor.ShowDialog();
		if (string.IsNullOrEmpty(name)) // Asset name dialog was cancelled
			return;
		// Get the original spell prefab
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
