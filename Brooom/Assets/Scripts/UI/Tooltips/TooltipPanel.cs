using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a tooltip panel which is located in a scene.
/// It is reused for all different objects displaying tooltip on mouse hover.
/// It can be displayed or hidden from <c>TooltipController</c>.
/// Its appearance may be changed by a specific <c>TooltipStyle</c>.
/// It may be static (just a static panel with text on the screen) 
/// or follow a cursor (then its size and position are adjusted according to the content as well as screen borders).
/// </summary>
public class TooltipPanel : MonoBehaviour {

	[Tooltip("Text fields of individual sections.")]
	[SerializeField] TooltipSectionsTextFields textFields;
    [Tooltip("RectTransform of the Canvas in which the tooltip panel is located.")]
    [SerializeField] RectTransform canvasRectTransform;

    [Tooltip("Whether this tooltip panel is just a static text on the screen or it is supposed to follow a cursor.")]
    [SerializeField] bool isStatic = false;

	private Image background;
	private RectTransform rectTransform;

    private float maxWidth;

    /// <summary>
    /// Changes appearance of the tooltip based on the given tooltip style.
    /// </summary>
    /// <param name="style">Tooltip style to be applied.</param>
    public void ChangeAppearance(TooltipStyle style) {
        // Background sprite an dcolor
        if (background == null) background = GetComponent<Image>();
        background.sprite = style.backgroundSprite;
        background.color = style.backgroundColor;
        // Font and text color
        foreach (var field in textFields.Enumerate()) {
            field.font = style.font;
            field.color = style.textColor;
        }
        // Maximum width
        if (!isStatic)
            maxWidth = style.maxWidth;
    }

    /// <summary>
    /// Sets the tooltip's content to the given values.
    /// </summary>
    /// <param name="content">A list of content of tooltip's individual fields.</param>
    public void SetContent(List<string> content) {
        int i = 0;
        foreach (var text in content) {
            textFields[i].text = text;
            i++;
        }
        if (!isStatic)
            AdjustSize();
    }

    /// <summary>
    /// Adjusts size of the tooltip panel according to its content while respecting the maximum width set in <c>TooltipStyle</c>.
    /// </summary>
    private void AdjustSize() {
        // Find width requested by each section, take the maximum
        float widthTop = ComputeSectionWidth(textFields.topSection);
        float widthMain = ComputeSectionWidth(textFields.mainSection, false);
        float widthBottom = ComputeSectionWidth(textFields.bottomSection);
        float width = Mathf.Max(widthTop, widthMain, widthBottom);
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        // Respect the maximum width set in the tooltip style
        if (width > maxWidth && maxWidth != -1) { // -1 means unlimited width
            rectTransform.sizeDelta = new Vector2(maxWidth, rectTransform.sizeDelta.y);
        } else {
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        }
    }

    // Computes width requested by a specific section
    //  - isHorizontal - whether fields in the section are organized horizontally (next to each other), or vertically (on top of each other)
    private float ComputeSectionWidth(TextMeshProUGUI[] section, bool isHorizontal = true) {
        // Compute the minimum width
        float width = 0;
        foreach (var field in section) {
            if (isHorizontal) width += field.preferredWidth; // sum up preferred width of all fields in the section
            else if (field.preferredWidth > width) width = field.preferredWidth; // take maximum preferred width from fields in the section
        }
        // Add padding and spacing
        width += 20; // added padding
        if (isHorizontal && section.Length > 0) width += 15 * (section.Length - 1); // added spacing (to ensure enough space between the parts)
        return width;
    }

    // Updates position of tooltip following a cursor
    private void Update() {
        if (!isStatic) {
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
}


/// <summary>
/// A structure containing text fields for different sections of tooltip content.
/// It provides method for enumerating them and also indexer to access them more easily.
/// </summary>
[System.Serializable]
public struct TooltipSectionsTextFields {

    [Tooltip("Text fields in the top section of the tooltip.")]
    public TextMeshProUGUI[] topSection;
    [Tooltip("Text fields in the main section of the tooltip.")]
    public TextMeshProUGUI[] mainSection;
    [Tooltip("Text fields in the bottom section of the tooltip.")]
    public TextMeshProUGUI[] bottomSection;

    /// <summary>
    /// Enumerates all individual text fields in several sections of the tooltip.
    /// </summary>
    /// <returns>An enumerable containing tooltip's text fields.</returns>
    public IEnumerable<TextMeshProUGUI> Enumerate() {
        foreach (var field in topSection) yield return field;
        foreach (var field in mainSection) yield return field;
        foreach (var field in bottomSection) yield return field;
    }

    /// <summary>
    /// Provides access to individual text field of the tooltip by indexing.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
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
