using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component responsible for displaying the current value from <c>Slider</c> component of the same object or its children in a label.
/// </summary>
public class SettingsSliderUI : MonoBehaviour {

    [Tooltip("A label displaying the slider's current value.")]
    [SerializeField] TextMeshProUGUI valueLabel;

    /// <summary>
    /// Updates the label to show the given value.
    /// Called whenever the associated slider's value changes.
    /// </summary>
    /// <param name="value">Value to display.</param>
    public void UpdateValueLabel(float value) {
        valueLabel.text = value.ToString();
    }

	private void Start() {
        valueLabel.text = GetComponentInChildren<Slider>().value.ToString();
    }

}
