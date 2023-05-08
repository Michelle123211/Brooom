using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTextMeshProUI : MonoBehaviour {

	[Tooltip("Key assigned to the phrase which should be displayed in the currently selecte language.")]
	public string assignedKey = "Default";

	[HideInInspector]
	public TextMeshProUGUI textLabel;


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


	public void UpdateText() {
		textLabel.text = LocalizationManager.Instance.GetLocalizedString(assignedKey);
	}

	private void Awake() {
		// Initialize with value of the currently selected language
		if (textLabel == null)
			textLabel = GetComponent<TextMeshProUGUI>();
		UpdateText();
	}

	private void OnEnable() {
		// Register the callback on current language change
		LocalizationManager.Instance.onCurrentLanguageChanged += UpdateText;
		UpdateText();
	}

	private void OnDisable() {
		// Unregister the callback on current language change
		LocalizationManager.Instance.onCurrentLanguageChanged -= UpdateText;
	}


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

	}

}
