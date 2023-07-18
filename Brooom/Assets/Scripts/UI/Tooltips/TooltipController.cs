using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // StringBuilder

public class TooltipController : MonoBehaviourSingleton<TooltipController>, ISingleton {

    [Header("Appearance")]
    [Tooltip("A tooltip style specifying e.g. background, font color, custom tags etc.")]
    public TooltipStyle tooltipStyle;


    private TooltipPanel tooltipPanel;

    private Dictionary<string, string> openingTags = new Dictionary<string, string>(); // TMPro tags corresponding to the user-defined tags
    private Dictionary<string, string> closingTags = new Dictionary<string, string>();
    private bool isLocalized = false; // if the tooltip is localized, given texts will be used as keys for localization

    public void SetTooltipContent(TooltipSectionsText content) {
        // Parse and format text of each section individually
        List<string> formattedText = new List<string>();
        foreach (var text in content.Enumerate()) {
            formattedText.Add(GetFormattedText(text));
        }
        // Then set it in the tooltip
        tooltipPanel.SetContent(formattedText);
    }

    public void SetTooltipContent(string content) {
        SetTooltipContent(new TooltipSectionsText { mainTop = content });
    }

    public void SetIsLocalized(bool isLocalized) {
        this.isLocalized = isLocalized;
    }

    public void ShowTooltip() {
        gameObject.TweenAwareEnable();
    }

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
        return ReplaceCustomTags(inputText);
    }

    // Replaces user-defined tags with those supported by TMPro
    private string ReplaceCustomTags(string inputText) {
        // TODO: Make it more efficient (e.g. using trie from tags)

        StringBuilder formattedString = new StringBuilder();
        Stack<string> tagStack = new Stack<string>(); // stack of tags which were not closed yet
        // Go through the text and replace tags
        int i = 0;
        while (i < inputText.Length) {
            // Check if any tag matches, find the longest one
            string tagFound = "";
            foreach (var customTag in tooltipStyle.customTags) {
                if (inputText.Length >= i + customTag.tag.Length // input string is long enough to contain tag
                    && inputText.Substring(i, customTag.tag.Length) == customTag.tag // input string contains tag
                    && customTag.tag.Length > tagFound.Length) { // tag is the longest one yet
                    tagFound = customTag.tag;
                }
            }
            if (tagFound.Length > 0) { // a tag was found
                if (tagStack.Count > 0 && tagStack.Peek() == tagFound) { // closing tag
                    formattedString.Append(closingTags[tagFound]);
                    tagStack.Pop();
                } else { // opening tag
                    formattedString.Append(openingTags[tagFound]);
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
            formattedString.Append(closingTags[tag]);
        }
        return formattedString.ToString();
    }

    // For each custom tag composes its TMPro-supported start and end tags and stores them in Dictionaries
    private void ConvertTagsToTMProTags() {
        foreach (var tag in tooltipStyle.customTags) {
            StringBuilder tmproOpenTag = new StringBuilder();
            StringBuilder tmproCloseTag = new StringBuilder();
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
            openingTags[tag.tag] = tmproOpenTag.ToString();
            closingTags[tag.tag] = tmproCloseTag.ToString();
        }
    }

	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
        tooltipPanel = GetComponentInChildren<TooltipPanel>();
        if (tooltipStyle == null)
            tooltipStyle = Resources.Load<TooltipStyle>("DefaultTooltipStyle");
        // Apply the style (change background, font)
        tooltipPanel.ChangeAppearance(tooltipStyle);
        // For each custom tag compose its TMPro-supported start and end tag, store them in Dictionaries
        ConvertTagsToTMProTags();
    }

	protected override void SetSingletonOptions() {
        Options = (int)SingletonOptions.LazyInitialization | (int)SingletonOptions.RemoveRedundantInstances;
	}

	private void OnDestroy() {
        if (Instance == this)
            ResetInstance();
	}
}

[System.Serializable]
public struct TooltipSectionsText {
    // Top section
    public string topLeft;
    public string topCenter;
    public string topRight;
    // Main section
    [TextArea(3, 8)]
    public string mainTop;
    [TextArea(3, 8)]
    public string mainBottom;
    // Bottom section
    public string bottomLeft;
    public string bottomCenter;
    public string bottomRight;

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
