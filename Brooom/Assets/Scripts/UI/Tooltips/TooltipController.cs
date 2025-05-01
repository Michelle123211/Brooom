using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // StringBuilder


/// <summary>
/// A singleton responsible for displaying tooltip panel.
/// It provides methods for setting its content and for showing or hiding the panel.
/// Unless explicitly set, by default it uses <c>TooltipStyle</c> named "DefaultTooltipStyle" from Resources folder.
/// It is also able to work with different user-defined tags from the tooltip style,
/// and provides static methods for obtaining mapping between the custom tooltip tags and corresponding TextMesh Pro tags
/// and also for replacing custom tooltip tags by corresponding TextMesh Pro tags in a given text.
/// </summary>
public class TooltipController : MonoBehaviourSingleton<TooltipController>, ISingleton {

    [Header("Appearance")]
    [Tooltip("A tooltip style specifying e.g. background, font color, custom tags etc.")]
    public TooltipStyle tooltipStyle;


    private TooltipPanel tooltipPanel;

    private TagsMapping tagsMapping; // mapping between user-defined tags and corresponding TMPro
    private bool isLocalized = false; // if the tooltip is localized, given texts will be used as keys for localization

	#region Static methods
	/// <summary>
	/// Converts all custom tooltip tags to equivalent TextMesh Pro opening and closing tags,
	/// and initializes a <c>TagsMapping</c> instance with all mappings between them.
	/// </summary>
	/// <param name="tooltipStyle">Tooltip style defining all custom tooltip tags.</param>
	/// <returns>Mappings between custom tooltip tags and TextMesh Pro tags.</returns>
	public static TagsMapping GetCustomTagsToTMProTagsMapping(TooltipStyle tooltipStyle) {
        TagsMapping tagsMap = new();
        // For each custom tag defined in TooltipStyle, create its TMPro equivalent
        foreach (var tag in tooltipStyle.customTags) {
            StringBuilder tmproOpenTag = new();
            StringBuilder tmproCloseTag = new();
            // Color
            if (tag.changeColor) {
                tmproOpenTag.Append($"<color={tag.textColor.ToHex()}>");
                tmproCloseTag.Insert(0, "</color>");
            }
            // Bold
            if (tag.bold) {
                tmproOpenTag.Append("<b>");
                tmproCloseTag.Insert(0, "</b>");
            }
            // Italics
            if (tag.italics) {
                tmproOpenTag.Append("<i>");
                tmproCloseTag.Insert(0, "</i>");
            }
            // Underline
            if (tag.underline) {
                tmproOpenTag.Append("<u>");
                tmproCloseTag.Insert(0, "</u>");
            }
            // Uppercase
            if (tag.uppercase) {
                tmproOpenTag.Append("<uppercase>");
                tmproCloseTag.Insert(0, "</uppercase>");
            }
            tagsMap.AddTMProTagPairFromCustomTag(tag.tag, tmproOpenTag.ToString(), tmproCloseTag.ToString());
        }
        return tagsMap;
    }

    /// <summary>
    /// Replaces user-defined tooltip tags with equivalent TextMesh Pro tags in the given text.
    /// </summary>
    /// <param name="inputText">Text in which tags should be replaced.</param>
    /// <param name="tagsMap">Mappings between custom tooltip tags and TextMesh Pro tags.</param>
    /// <returns>Text containing TextMesh Pro tags instead of user-defined tooltip tags.</returns>
    public static string ReplaceCustomTagsWithTMProTags(string inputText, TagsMapping tagsMap) {
        // TODO: Make it more efficient (e.g. using trie from tags)

        StringBuilder formattedString = new();
        Stack<string> tagStack = new(); // stack of tags which were not closed yet
        // Go through the text and replace tags
        int i = 0;
        while (i < inputText.Length) {
            // Check if any tag matches, find the longest one
            string tagFound = "";
            foreach (var customTag in tagsMap.CustomTags) {
                if (inputText.Length >= i + customTag.Length // input string is long enough to contain tag
                    && inputText.Substring(i, customTag.Length) == customTag // input string contains tag
                    && customTag.Length > tagFound.Length) { // tag is the longest one yet
                    tagFound = customTag;
                }
            }
            if (tagFound.Length > 0) { // a tag was found
                if (tagStack.Count > 0 && tagStack.Peek() == tagFound) { // closing tag
                    formattedString.Append(tagsMap.GetTMProClosingTag(tagFound));
                    tagStack.Pop();
                } else { // opening tag
                    formattedString.Append(tagsMap.GetTMProOpeningTag(tagFound));
                    tagStack.Push(tagFound);
                }
                i += tagFound.Length;
            } else { // no tag
                formattedString.Append(inputText[i]);
                i++;
            }
        }
        // Resolve any open tags
        string tag;
        while (tagStack.Count > 0) {
            tag = tagStack.Pop();
            formattedString.Append(tagsMap.GetTMProClosingTag(tag));
        }
        return formattedString.ToString();
    }
	#endregion

	/// <summary>
	/// Sets content of each tooltip panel's text field, while localizing it if necessary and replacing custom tags with TextMesh Pro tags.
	/// </summary>
	/// <param name="content">Strings corresponding to individual fields of tooltip's sections.</param>
	public void SetTooltipContent(TooltipSectionsText content) {
        // Parse and format text of each section individually
        List<string> formattedText = new();
        foreach (var text in content.Enumerate()) {
            formattedText.Add(GetFormattedText(text));
        }
        // Then set it in the tooltip
        tooltipPanel.SetContent(formattedText);
    }

    /// <summary>
    /// Sets content of the tooltip's top field of the main section to the given string.
    /// </summary>
    /// <param name="content">String to be displayed in the tooltip.</param>
    public void SetTooltipContent(string content) {
        SetTooltipContent(new TooltipSectionsText { mainTop = content });
    }

    /// <summary>
    /// Sets whether the text in tooltip is localized (then it is treated as keys for localization), or not (then it is used as is).
    /// </summary>
    /// <param name="isLocalized"><c>true</c> if the text in the tooltip should be localized, <c>false</c> otherwise.</param>
    public void SetIsLocalized(bool isLocalized) {
        this.isLocalized = isLocalized;
    }

    /// <summary>
    /// Shows the tooltip panel.
    /// </summary>
    public void ShowTooltip() {
        gameObject.TweenAwareEnable();
    }
    /// <summary>
    /// Hides the tooltip panel.
    /// </summary>
    public void HideTooltip() {
        gameObject.TweenAwareDisable();
    }

    // Localizes the string and replaces user-defined tags with those supported by TMPro
    private string GetFormattedText(string inputText) {
        // Handle empty strings
        if (string.IsNullOrEmpty(inputText))
            return string.Empty;
        // Get localized string if necessary
        if (isLocalized) {
            inputText = LocalizationManager.Instance.GetLocalizedString(inputText);
        }
        // Replace tags
        return TooltipController.ReplaceCustomTagsWithTMProTags(inputText, tagsMapping);
    }

	#region Singleton initialization
	static TooltipController() {
        // Singleton options override
        Options = SingletonOptions.LazyInitialization | SingletonOptions.RemoveRedundantInstances | SingletonOptions.PersistentBetweenScenes;
    }

    /// <inheritdoc/>
    public void AwakeSingleton() {
        gameObject.SetActive(false);
    }

    /// <inheritdoc/>
    public void InitializeSingleton() {
        tooltipPanel = GetComponentInChildren<TooltipPanel>();
        // Load default tooltip style if necessary
        if (tooltipStyle == null)
            tooltipStyle = Resources.Load<TooltipStyle>("DefaultTooltipStyle");
        // Apply the style (change background, font)
        tooltipPanel.ChangeAppearance(tooltipStyle);
        // For each custom tag compose its TMPro-supported start and end tag, store them
        tagsMapping = TooltipController.GetCustomTagsToTMProTagsMapping(tooltipStyle);
    }
    #endregion

    private void OnDestroy() {
        if (Instance == this)
            ResetInstance();
	}

}

/// <summary>
/// A class providing methods for converting custom user-defined tags (from <c>TooltipStyle</c>) used in tooltip's content
/// to TextMesh Pro tags.
/// It need to be initialized first, by adding all mappings.
/// </summary>
public class TagsMapping {

    /// <summary>An enumerable of all custom tooltip tags.</summary>
    public IEnumerable<string> CustomTags => openingTags.Keys;

    private Dictionary<string, string> openingTags; // custom tooltip tag --> TextMesh Pro opening tag
    private Dictionary<string, string> closingTags; // custom tooltip tag --> TextMesh Pro closing tag

    public TagsMapping() {
        openingTags = new Dictionary<string, string>();
        closingTags = new Dictionary<string, string>();
    }

    /// <summary>
    /// Adds a new mapping from custom tooltip tag to TextMesh Pro tag.
    /// </summary>
    /// <param name="customTag">Custom tag used in tooltip's content.</param>
    /// <param name="TMProOpeningTag">Corresponding TextMeshPro opening tag.</param>
    /// <param name="TMProClosingTag">Corresponding TextMeshPro closing tag.</param>
    public void AddTMProTagPairFromCustomTag(string customTag, string TMProOpeningTag, string TMProClosingTag) {
        openingTags[customTag] = TMProOpeningTag;
        closingTags[customTag] = TMProClosingTag;
    }

    /// <summary>
    /// Gets TextMesh Pro opening tag corresponding to the custom tag used in tooltip's content.
    /// </summary>
    /// <param name="customTag">Custom tag used in tooltip's content.</param>
    /// <returns>Corresponding TextMesh Pro opening tag, or <c>null</c> if not defined.</returns>
    public string GetTMProOpeningTag(string customTag) { 
        return openingTags.ContainsKey(customTag) ? openingTags[customTag] : null;
    }

    /// <summary>
    /// Gets TextMesh Pro closing tag corresponding to the custom tag used in tooltip's content.
    /// </summary>
    /// <param name="customTag">Custom tag used in tooltip's content.</param>
    /// <returns>Corresponding TextMesh Pro closing tag, or <c>null</c> if not defined.</returns>
    public string GetTMProClosingTag(string customTag) {
        return closingTags.ContainsKey(customTag) ? closingTags[customTag] : null;
    }

    /// <summary>
    /// Gets a pair TextMesh Pro tags (opening and closing) corresponding to the custom tag used in tooltip's content.
    /// </summary>
    /// <param name="customTag">Custom tag used in tooltip's content.</param>
    /// <returns>A pair of corresponding TextMesh Pro tags, or a pair of <c>null</c>s if not defined.</returns>
    public (string openingTag, string closingTag) GetTMProTagPairFromCustomTag(string customTag) {
        string openingTag = openingTags.ContainsKey(customTag) ? openingTags[customTag] : null;
        string closingTag = closingTags.ContainsKey(customTag) ? closingTags[customTag] : null;
        return (openingTag, closingTag);
    }
}

/// <summary>
/// A data structure containing strings corresponding to individual fields in tooltip's sections.
/// </summary>
[System.Serializable]
public struct TooltipSectionsText {

    // Top section
    /// <summary>String corresponding to the left field of the top section.</summary>
    public string topLeft;
    /// <summary>String corresponding to the middle field of the top section.</summary>
    public string topCenter;
    /// <summary>String corresponding to the right field of the top section.</summary>
    public string topRight;
    // Main section
    /// <summary>String corresponding to the top field of the main section.</summary>
    [TextArea(3, 8)]
    public string mainTop;
    /// <summary>String corresponding to the bottom field of the main section.</summary>
    [TextArea(3, 8)]
    public string mainBottom;
    // Bottom section
    /// <summary>String corresponding to the left field of the bottom section.</summary>
    public string bottomLeft;
    /// <summary>String corresponding to the middle field of the bottom section.</summary>
    public string bottomCenter;
    /// <summary>String corresponding to the right field of the bottom section.</summary>
    public string bottomRight;

    /// <summary>
    /// Enumerates all strings corresponding to individual fields of tooltip's sections.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> Enumerate() {
        yield return topLeft;
        yield return topCenter;
        yield return topRight;

        yield return mainTop;
        yield return mainBottom;

        yield return bottomLeft;
        yield return bottomCenter;
        yield return bottomRight;
    }

}
