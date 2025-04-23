using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component for representing customization option in Character Creator UI.
/// It can be selected and then corresponding customization is applied to a character preview in the scene.
/// </summary>
public class CustomizationOptionUI : MonoBehaviour
{
    /// <summary>TextMesh Pro containing number of the option.</summary>
    [SerializeField] private TextMeshProUGUI numberLabel;
    /// <summary>Image displaying color assigned to this customization option.</summary>
    [SerializeField] private Image colorImage;

    // Customization data
    private CharacterAppearance changedCharacter;
    private CustomizedPart changedPart;
    private CustomizationVariantData assignedData;

    /// <summary>
    /// Initializes UI related to a customization option represented by this instance.
    /// </summary>
    /// <param name="character">Character to which this customization option is applied when selected.</param>
    /// <param name="part">Character part which is customized with this option.</param>
    /// <param name="data">Data related to the customization option to be represented.</param>
    /// <param name="index">This option's number among all options available for the given part.</param>
    public void Initialize(CharacterAppearance character, CustomizedPart part, CustomizationVariantData data, int index) {
        changedCharacter = character;
        changedPart = part;
        assignedData = data;
        if (part == CustomizedPart.SkinTone || part == CustomizedPart.HairColor) {
            colorImage.color = assignedData.assignedColor; // displaying the assigned color
            numberLabel.gameObject.SetActive(false);
        } else {
            numberLabel.text = index.ToString(); // numbers are displayed instead of color
        }
    }

    /// <summary>
    /// Applies corresponding customization to the character when activated.
    /// </summary>
    /// <param name="isActive">If this option is activated, or deactivated.</param>
    public void OnActiveChanged(bool isActive) {
        if (isActive) {
            // Apply customization to the character
            changedCharacter.ApplyCustomization(changedPart, assignedData);
        }
    }
}
