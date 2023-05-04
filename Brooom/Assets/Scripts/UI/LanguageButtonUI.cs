using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButtonUI : MonoBehaviour
{
    [Tooltip("Language activated by this button.")]
    public Language assignedLanguage;

    public void ChangeLanguage(bool buttonValue) {
        if (buttonValue) { // the assigned language is now selected
            // TODO: Change language globally (if necessary, it may be the same as the one selected do far)
            Debug.Log($"Language changed to {assignedLanguage}");
        }
        // Ignore deselecting a language {is always accompanied by selecting a different one, which is handled)
    }

	private void Start() {
        // Set the default language
        // TODO: Look up which language the player selected last time, then initialize the toggle accordingly
        Toggle toggle = GetComponent<Toggle>();
        if (toggle != null && toggle.isOn) {
            ChangeLanguage(true);
        }
	}
}



// TODO: Move to a place where localization is handled
public enum Language {
    CZECH,
    ENGLISH
}