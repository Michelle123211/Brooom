using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipPanel : MonoBehaviour
{
	[Tooltip("Text fields of individual sections.")]
	public TooltipSectionsTextFields textFields;
    [Tooltip("RectTransform of the Canvas in which the tooltip panel is located.")]
    public RectTransform canvasRectTransform;

	private Image background;
	private RectTransform rectTransform;

    private float maxWidth;

    public void ChangeAppearance(TooltipStyle style) {
        if (background == null) background = GetComponent<Image>();
        background.sprite = style.backgroundSprite;
        background.color = style.backgroundColor;
        foreach (var field in textFields.Enumerate()) {
            field.font = style.font;
            field.color = style.textColor;
        }
        maxWidth = style.maxWidth;
    }

    public void SetContent(List<string> content) {
        int i = 0;
        foreach (var text in content) {
            textFields[i].text = text;
            i++;
        }
        AdjustSize();
    }

    private void AdjustSize() {
        // Adjust size of the tooltip according to its content
        float widthTop = ComputeSectionWidth(textFields.topSection);
        float widthMain = ComputeSectionWidth(textFields.mainSection, false);
        float widthBottom = ComputeSectionWidth(textFields.bottomSection);
        float width = Mathf.Max(widthTop, widthMain, widthBottom);
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        // Respect the maximum width set in the tooltip style
        if (width > maxWidth) {
            rectTransform.sizeDelta = new Vector2(maxWidth, rectTransform.sizeDelta.y);
        } else {
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        }
    }

    private float ComputeSectionWidth(TextMeshProUGUI[] section, bool isHorizontal = true) {
        float width = 0;
        foreach (var field in section) {
            if (isHorizontal) width += field.preferredWidth;
            else if (field.preferredWidth > width) width = field.preferredWidth;
        }
        width += 20; // added padding
        if (isHorizontal && section.Length > 0) width += 15 * (section.Length - 1); // added spacing (to ensure enough space between the parts)
        return width;
    }

    private void Update() {
        // Follow the mouse cursor
        transform.position = Input.mousePosition;
        // Adjust position according to screen borders
        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        if (anchoredPosition.x + rectTransform.rect.width > canvasRectTransform.rect.width)
            anchoredPosition.x = canvasRectTransform.rect.width - rectTransform.rect.width;
        if (anchoredPosition.y + rectTransform.rect.height > canvasRectTransform.rect.height)
            anchoredPosition.y = canvasRectTransform.rect.height - rectTransform.rect.height;
        rectTransform.anchoredPosition = anchoredPosition;

    }
}


[System.Serializable]
public struct TooltipSectionsTextFields {
    [Tooltip("Text fields in the top section of the tooltip.")]
    public TextMeshProUGUI[] topSection;
    [Tooltip("Text fields in the main section of the tooltip.")]
    public TextMeshProUGUI[] mainSection;
    [Tooltip("Text fields in the bottom section of the tooltip.")]
    public TextMeshProUGUI[] bottomSection;

    public IEnumerable<TextMeshProUGUI> Enumerate() {
        foreach (var field in topSection) yield return field;
        foreach (var field in mainSection) yield return field;
        foreach (var field in bottomSection) yield return field;
    }

    public TextMeshProUGUI this[int i] {
        get {
            if (i < topSection.Length) return topSection[i];
            i -= topSection.Length;
            if (i < mainSection.Length) return mainSection[i];
            i -= mainSection.Length;
            return bottomSection[i];
        }
    }
}
