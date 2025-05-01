using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A component for a toggle representing a single localization language.
/// When the toggle is activated, it changes the currently selected language to the associated one.
/// </summary>
public class LanguageButtonUI : MonoBehaviour {

    private string assignedLanguage; // language activated by this button

    [SerializeField]
    [Tooltip("Toggle which invokes language change when it is activated.")]
    private Toggle toggle;
    [SerializeField]
    [Tooltip("Image displayed when the toggle is active. Its sprite is changed to a flag of the assigned language.")]
    private Image activeImage;
    [SerializeField]
    [Tooltip("Image displayed when the toggle is inactive. Its sprite is changed to a flag of the assigned language.")]
    private Image inactiveImage;

    /// <summary>
    /// If the current state is changed to being selected, the assigned language is set as the currently selected one, affecting lozalization.
    /// Called whenever value of the associated toggle changes.
    /// </summary>
    /// <param name="isSelected">Whether the assigned language is selected, or deselected.</param>
    public void OnSelectedChanged(bool isSelected) {
        if (isSelected) { // the assigned language is now selected
            // Change language globally
            Analytics.Instance.LogEvent(AnalyticsCategory.Game, $"Current localization changed to {assignedLanguage}.");
            LocalizationManager.Instance.ChangeCurrentLanguage(assignedLanguage);
        }
        // Ignore deselecting a language (is always accompanied by selecting a different one, which is handled)
    }

    /// <summary>
    /// Initializes all UI elements according to the assigned language and its flag.
    /// </summary>
    /// <param name="language">Name of the language to be assigned to this toggle.</param>
    /// <param name="flag">Flag of the assigned language.</param>
    public void Initialize(string language, Sprite flag) {
        assignedLanguage = language;
        activeImage.sprite = flag;
        inactiveImage.sprite = flag;
    }

}