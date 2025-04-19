using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsSliderUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI valueLabel;

    public void UpdateValueLabel(float value) {
        valueLabel.text = value.ToString();
    }

	private void Start() {
        valueLabel.text = GetComponentInChildren<Slider>().value.ToString();
    }
}
