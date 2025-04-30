using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// A class specifying a style used in a tooltip.
/// It contains basic parameters such as background sprite and color, text color, font and maximum width of the tooltip panel.
/// But it also defines custom tags which may be used in the tooltip's content to change style of the text (e.g. color, bold, italics, underline, uppercase).
/// If the tooltip panel doesn't have an explicitly assigned style, a style named "DefaultTooltipStyle" from Resources folder is used by default.
/// </summary>
[CreateAssetMenu(menuName = "Tooltip / Style", fileName = "TooltipStyle")]
public class TooltipStyle : ScriptableObject {
    [Header("Background")]
    [Tooltip("A Sprite used as a background of the tooltip panel.")]
    public Sprite backgroundSprite;
    [Tooltip("Color used with the sprite specified above.")]
    public Color backgroundColor = Color.black;

    [Header("Text")]
    [Tooltip("Default color of the text (when no custom tags are applied).")]
    public Color textColor = Color.white;
    [Tooltip("Font used for all the tooltip text.")]
    public TMP_FontAsset font;

    [Header("Size")]
    [Tooltip("Maximum width of the tooltip (use -1 if it is unlimited).")]
    public int maxWidth = 500;

    [Tooltip("User-defined tags which may be used in the tooltip text to change style of specific substrings.")]
    public List<TooltipStyleTag> customTags;
}

/// <summary>
/// A class representing a user-defined tag which may be used in tooltip content to change style of a specific substring.
/// </summary>
[System.Serializable]
public class TooltipStyleTag {

    [Header("Tag")]
    [Tooltip("A special identifier used in a text to change style of a substring (it must be placed before and after the substring).")]
    public string tag = "**";

    [Header("Color")]
    [Tooltip("Whether the color of the text marked with the given tag should be changed (i.e. the following color is valid and not default).")]
    public bool changeColor = false;
    [Tooltip("A color which should be used for a text marked with the given tag.")]
    public Color textColor = Color.gray;

    [Header("Format")]
    [Tooltip("Whether the text marked with the given tag should be bold.")]
    public bool bold = false;
    [Tooltip("Whether the text marked with the given tag should be in italics.")]
    public bool italics = false;
    [Tooltip("Whether the text marked with the given tag should be underlined.")]
    public bool underline = false;
    [Tooltip("Whether the text marked with the given tag should be uppercase.")]
    public bool uppercase = false;

}
