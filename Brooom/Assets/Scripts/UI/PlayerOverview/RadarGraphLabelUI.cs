using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class RadarGraphLabelUI : MonoBehaviour
{
    [Header("Axis label")]
    [SerializeField] TextMeshProUGUI labelText;

    [Header("Value change labels")]
    [SerializeField] GameObject topRightLabel;
    [SerializeField] GameObject bottomRightLabel;
    [SerializeField] GameObject bottomLeftLabel;
    [SerializeField] GameObject topLeftLabel;

    private Tooltip tooltip;
    private RectTransform rectTransform;

    private string label;

    RectTransform currentChangeLabelRectTransform;
    TextMeshProUGUI currentValueChangeText;
    Image currentValueChangeBackground;
    private float currentChangeValue;
    private string currentFormat;

    public void Initialize(string content, string tooltipDescription = null) {
        label = content;
        labelText.text = content;
        if (tooltipDescription != null) {
            if (tooltip == null) tooltip = GetComponentInChildren<Tooltip>();
            if (tooltip != null) tooltip.texts.mainTop = tooltipDescription;
        }
        HideAllValueChangeLabels();
    }

    public void SetValueChange(float currentValue, float valueChange, float tweenDuration, string format) {
        currentFormat = format;
        // Set current value
        labelText.text = $"{label}:{currentValue.ToString(currentFormat)}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        // Choose the best text field (according to where around the graph the label is)
        HideAllValueChangeLabels();
        ChooseValueChangeTextField();
        // Tween the value change
        currentChangeValue = 0;
        UpdateValueChange();
        DOTween.To(() => currentChangeValue, x => { currentChangeValue = x; UpdateValueChange(); }, valueChange, tweenDuration);
    }

    // Chooses a suitable text field for displaying value change (placed according to the anchored position of the label)
    private void ChooseValueChangeTextField() {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        GameObject selectedValueChangeLabel;
        if (rectTransform.anchoredPosition.x < 0) { // left
            if (rectTransform.anchoredPosition.y < 0) // bottom
                selectedValueChangeLabel = bottomLeftLabel;
            else // top
                selectedValueChangeLabel = topLeftLabel;
        } else { // right
            if (rectTransform.anchoredPosition.y < 0) // bottom
                selectedValueChangeLabel = bottomRightLabel;
            else // top
                selectedValueChangeLabel = topRightLabel;
        }
        currentValueChangeText = selectedValueChangeLabel.GetComponentInChildren<TextMeshProUGUI>();
        currentValueChangeBackground = selectedValueChangeLabel.GetComponent<Image>();
        currentChangeLabelRectTransform = selectedValueChangeLabel.GetComponent<RectTransform>();
        currentValueChangeText.text = "";
        selectedValueChangeLabel.SetActive(true);
    }

    private void UpdateValueChange() {
        // Prepare the string
        string valueChangeText = currentChangeValue.ToString(currentFormat);
        if (currentChangeValue >= 0) valueChangeText = "+" + valueChangeText;
        // Set text and color
        currentValueChangeText.text = valueChangeText;
        if (currentChangeValue < 0) {
            currentValueChangeBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
        } else {
            currentValueChangeBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_PositiveColor);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentChangeLabelRectTransform);
    }

    private void HideAllValueChangeLabels() {
        topRightLabel.SetActive(false);
        bottomRightLabel.SetActive(false);
        bottomLeftLabel.SetActive(false);
        topLeftLabel.SetActive(false);
    }
}
