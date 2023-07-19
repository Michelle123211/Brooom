using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingsSliderUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI valueLabel;

    public void UpdateValueLabel(float value) {
        valueLabel.text = value.ToString();
    }
}
