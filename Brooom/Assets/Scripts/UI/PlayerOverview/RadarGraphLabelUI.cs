using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;


/// <summary>
/// A component representing a single label of radar graph axis (used from <c>RadarGraphUI</c>).
/// It has a text label for displaying a value, but also another one for displaying change in value.
/// On top of that, it shows a tooltip with further description on mouse hover.
/// </summary>
public class RadarGraphLabelUI : MonoBehaviour {

    [Header("Axis label")]
    [Tooltip("Label for displaying the name of the associated axis and the axis' value.")]
    [SerializeField] TextMeshProUGUI labelText;

    [Header("Value change labels")]
    [Tooltip("Label for displaying change in value in the graph's axis, located in the top-right corner.")]
    [SerializeField] GameObject topRightLabel;
    [Tooltip("Label for displaying change in value in the graph's axis, located in the bottom-right corner.")]
    [SerializeField] GameObject bottomRightLabel;
    [Tooltip("Label for displaying change in value in the graph's axis, located in the bottom-left corner.")]
    [SerializeField] GameObject bottomLeftLabel;
    [Tooltip("Label for displaying change in value in the graph's axis, located in the top-left corner.")]
    [SerializeField] GameObject topLeftLabel;

    private Tooltip tooltip;
    private RectTransform rectTransform;

    private string label;

    // UI elements for displaying change in value - selected from 4 different options based on the placement of the axis
    RectTransform currentChangeLabelRectTransform;
    TextMeshProUGUI currentValueChangeText;
    Image currentValueChangeBackground;

    private float currentChangeValue;
    private string currentFormat;

    /// <summary>
    /// Initializes the label with the given content.
    /// </summary>
    /// <param name="content">Name of the axis to be displayed.</param>
    /// <param name="tooltipDescription">Description to be displayed in a tooltip.</param>
    public void Initialize(string content, string tooltipDescription = null) {
        label = content;
        labelText.text = content;
        if (tooltipDescription != null) {
            if (tooltip == null) tooltip = GetComponentInChildren<Tooltip>();
            if (tooltip != null) tooltip.texts.mainTop = tooltipDescription;
        }
        HideAllValueChangeLabels();
    }

    /// <summary>
    /// Displays the current value (together with the axis label as "label:value") and also a change in value (tweened over time).
    /// </summary>
    /// <param name="currentValue">Current axis' value.</param>
    /// <param name="valueChange">Change in value.</param>
    /// <param name="tweenDuration">Duration (in seconds) of tween used to display change in value.</param>
    /// <param name="format">Format in which the axis' value and value change should be displayed.</param>
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

    // Updates label displaying the change in value and changes background color based on whether the change is positive or negative
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

    // Hides all labels for displaying change in values
    private void HideAllValueChangeLabels() {
        topRightLabel.SetActive(false);
        bottomRightLabel.SetActive(false);
        bottomLeftLabel.SetActive(false);
        topLeftLabel.SetActive(false);
    }

}
