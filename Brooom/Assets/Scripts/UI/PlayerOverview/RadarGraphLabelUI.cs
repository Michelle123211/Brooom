using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadarGraphLabelUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] TextMeshProUGUI valueChangeText;

    private Tooltip tooltip;

    public void Initialize(string content, string tooltipDescription = null) {
        labelText.text = content;
        valueChangeText.text = string.Empty;
        if (tooltipDescription != null) {
            if (tooltip == null) tooltip = GetComponent<Tooltip>();
            if (tooltip != null) tooltip.texts.mainTop = tooltipDescription;
        }
    }

    public void SetValueChange(float valueChange) {
        valueChangeText.text = valueChange.ToString("N1");
        if (valueChange < 0) {
            valueChangeText.color = Color.red; // TODO: Use a color from color palette
        } else {
            valueChangeText.color = Color.green; // TODO: Use a color from color palette
            valueChangeText.text = "+" + valueChangeText.text;
        }
    }
}
