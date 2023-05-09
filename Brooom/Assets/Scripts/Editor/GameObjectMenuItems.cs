using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class GameObjectMenuItems : MonoBehaviour {

	// Adds a menu item to create a GameObject with this component as well as TextMeshProUGUI
	[MenuItem("GameObject/Localization/Localized TextMeshPro", false, 10)]
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
}
