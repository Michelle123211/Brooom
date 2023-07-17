using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "Tooltip / Style", fileName = "TooltipStyle")]
public class TooltipStyle : ScriptableObject {
    [Header("Background")]
    [Tooltip("A Sprite used as a background for the tooltip text.")]
    public Sprite backgroundSprite;
    [Tooltip("Color used with the sprite specified above.")]
    public Color backgroundColor = Color.black;

    [Header("Text")]
    [Tooltip("Default color of the text (when no custom tags are applied).")]
    public Color textColor = Color.white;
    [Tooltip("Font used for all the tooltip text.")]
    public TMP_FontAsset font;

    [Header("Size")]
    [Tooltip("Minimum and maximum width of the tooltip (use -1 where it is unlimited).")]
    public Vector2Int minMaxWidth = new Vector2Int(0, 200);
    [Tooltip("Minimum and maximum height of the tooltip (use -1 where it is unlimited).")]
    public Vector2Int minMaxHeight = new Vector2Int(-1, -1);

    [Tooltip("User-defined tags which may be used in the tooltip text to change style of specific substrings.")]
    public List<TooltipStyleTag> customTags;
}

[System.Serializable]
public class TooltipStyleTag {
    [Header("Tag")]
    [Tooltip("A special identifier used in a text to change style of a substring.")]
    public string tag = "**";

    [Header("Color")]
    [Tooltip("Whether the color of the text marked with the given tag should be changed (i.e. the following color is valid and not default).")]
    public bool changeColor = false;
    [Tooltip("A color which shoudl be used fo a text marked with the given tag.")]
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
