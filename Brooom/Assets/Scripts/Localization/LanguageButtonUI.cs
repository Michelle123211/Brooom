using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButtonUI : MonoBehaviour
{
    // Language activated by this button
    private string assignedLanguage;

    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private Image activeImage;
    [SerializeField]
    private Image inactiveImage;

    public void OnSelectedChanged(bool isSelected) {
        if (isSelected) { // the assigned language is now selected
            // Change language globally
            LocalizationManager.Instance.ChangeCurrentLanguage(assignedLanguage);
        }
        // Ignore deselecting a language (is always accompanied by selecting a different one, which is handled)
    }

    public void Initialize(string language, Sprite flag) {
        assignedLanguage = language;
        activeImage.sprite = flag;
        inactiveImage.sprite = flag;
    }
}