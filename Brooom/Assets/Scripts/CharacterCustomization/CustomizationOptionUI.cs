using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationOptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberLabel;
    [SerializeField] private Image colorImage;

    // Customization data
    private CharacterAppearance changedcharacter;
    private CustomizedPart changedPart;
    private CustomizationVariantData assignedData;

    public void Initialize(CharacterAppearance character, CustomizedPart part, CustomizationVariantData data, int index) {
        changedcharacter = character;
        changedPart = part;
        assignedData = data;
        if (part == CustomizedPart.SkinTone || part == CustomizedPart.HairColor) {
            colorImage.color = assignedData.assignedColor; // displaying the assigned color
            numberLabel.gameObject.SetActive(false);
        } else {
            numberLabel.text = index.ToString(); // numbers are displayed instead of color
        }
    }

    public void OnActiveChanged(bool isActive) {
        if (isActive) {
            // Apply customization to the character
            changedcharacter.ApplyCustomization(changedPart, assignedData);
        }
    }
}
