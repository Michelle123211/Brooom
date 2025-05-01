using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


/// <summary>
/// A component for an object with <c>TextMeshProUGUI</c> which should display localized text based on the currently selected language.
/// A localization key is assigned to it, and then, whenever the currently selected language changes, text is updated to the corresponding phrase.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTextMeshProUI : MonoBehaviour {

	[Tooltip("Key assigned to the phrase which should be displayed.")]
	public string assignedKey = "Default";

	[HideInInspector]
	[Tooltip("TextMesh Pro label whose content will be changed to the phrase in the currently selected language. It it is not set, appropriate component on the same object will be retrieved, if exists.")]
	public TextMeshProUGUI textLabel;


	/// <summary>
	/// Updates the text of the associated TextMesh Pro label to the phrase in the currently selected language.
	/// </summary>
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

}
