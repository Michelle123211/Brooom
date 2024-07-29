using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

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

	// Adds a menu item to create a spell prefab containing a few basic components
	[MenuItem("Assets/Create/Spell System/Spell", priority = 10)]
	public static void CreateSpellPrefab(MenuCommand menuCommand) {
		int i = Resources.FindObjectsOfTypeAll<Spell>().Length;
		string assetPath = $"Assets/Resources/Spells/SpellPrefab{i+1}.prefab";
		// Create game objects
		GameObject spell = new GameObject("Spell");
		GameObject spellEffect = new GameObject("SpellEffect");
		spellEffect.transform.parent = spell.transform;
		// Add components and connect them
		Spell spellComponent = spell.AddComponent<Spell>();
		SpellEffectController spellEffectController = spellEffect.AddComponent<SpellEffectController>();
		spellComponent.InitializeSpellEffectController(spellEffectController);
		// Save the prefab and remove the original template
		PrefabUtility.SaveAsPrefabAsset(spell, assetPath);
		DestroyImmediate(spell);
	}
}
