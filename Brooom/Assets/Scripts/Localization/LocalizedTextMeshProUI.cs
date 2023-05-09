using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTextMeshProUI : MonoBehaviour {

	[Tooltip("Key assigned to the phrase which should be displayed in the currently selecte language.")]
	public string assignedKey = "Default";

	[HideInInspector]
	public TextMeshProUGUI textLabel;


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
