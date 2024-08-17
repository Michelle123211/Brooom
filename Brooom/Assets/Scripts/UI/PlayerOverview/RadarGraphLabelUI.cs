using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class RadarGraphLabelUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelText;

    [SerializeField] TextMeshProUGUI topRightValueText;
    [SerializeField] TextMeshProUGUI bottomRightValueText;
    [SerializeField] TextMeshProUGUI bottomLeftValueText;
    [SerializeField] TextMeshProUGUI topLeftValueText;

    private Tooltip tooltip;
    private RectTransform rectTransform;

    TextMeshProUGUI currentTextField;
    private float currentValue;
    private string currentFormat;

    public void Initialize(string content, string tooltipDescription = null) {
        labelText.text = content;
        if (tooltipDescription != null) {
            if (tooltip == null) tooltip = GetComponent<Tooltip>();
            if (tooltip != null) tooltip.texts.mainTop = tooltipDescription;
        }
        SetEmptyValueChange();
    }

    public void SetValueChange(float valueChange, float tweenDuration, string format) {
        currentFormat = format;
        // Reset all labels (just to make sure)
        SetEmptyValueChange();
        // Choose the best text field (according to where around the graph the label is)
        currentTextField = ChooseValueChangeTextField();
        // Tween the value change
        currentValue = 0;
        UpdateValueChange();
        DOTween.To(() => currentValue, x => { currentValue = x; UpdateValueChange(); }, valueChange, tweenDuration);
    }

    private void SetEmptyValueChange() {
        topRightValueText.text = string.Empty;
        bottomRightValueText.text = string.Empty;
        bottomLeftValueText.text = string.Empty;
        topLeftValueText.text = string.Empty;
    }

    // Chooses a suitable text field for displaying value change (placed according to the anchored position of the label)
    private TextMeshProUGUI ChooseValueChangeTextField() {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rectTransform.anchoredPosition.x < 0) { // left
            if (rectTransform.anchoredPosition.y < 0) // bottom
                return bottomLeftValueText;
            else // top
                return topLeftValueText;
        } else { // right
            if (rectTransform.anchoredPosition.y < 0) // bottom
                return bottomRightValueText;
            else // top
                return topRightValueText;
        }
    }

    private void UpdateValueChange() {
        // Prepare the string
        string valueChangeText = currentValue.ToString(currentFormat);
        if (currentValue >= 0) valueChangeText = "+" + valueChangeText;
        // Set text and color
        currentTextField.text = valueChangeText;
        if (currentValue < 0) {
            currentTextField.color = Color.red; // TODO: Use a color from color palette
        } else {
            currentTextField.color = Color.green; // TODO: Use a color from color palette
        }
    }
}
