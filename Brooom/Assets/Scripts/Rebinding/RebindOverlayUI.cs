using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// A component for an overlay displayed during rebinding operation.
/// It contains UI elements to show the action for which binding is being created and what was bound so far in a composite action.
/// It can also display a warning that the chosen binding is already in use.
/// </summary>
public class RebindOverlayUI : MonoBehaviour {

    [Tooltip("A label in the overlay which is used to display what part the binding is for.")]
    [SerializeField] private TextMeshProUGUI waitingForInputText;
    [Tooltip("A label for displaying what was input so far in the composite.")]
    [SerializeField] private TextMeshProUGUI alreadyBoundText;
    [Tooltip("A label with warning that the last binding is already in use.")]
    [SerializeField] private TextMeshProUGUI duplicateWarningText;

    /// <summary>
    /// Sets content of the label which is used to display what part the binding is for.
    /// </summary>
    /// <param name="text">Content to be displayed.</param>
    public void SetWaitingForInputText(string text) {
        waitingForInputText.text = text;
    }

    /// <summary>
    /// Sets content of the label displaying what was bound so far in a composite action.
    /// </summary>
    /// <param name="text">Content to be displayed.</param>
    /// <param name="append"><c>true</c> if the text should be appended, <c>false</c> if the new content should replace the previous one.</param>
    public void SetAlreadyBoundText(string text, bool append = false) {
        if (append)
            alreadyBoundText.text += text;
        else
            alreadyBoundText.text = text;
    }

    /// <summary>
    /// Sets content of the label displaying a warning that the last binding is already in use.
    /// </summary>
    /// <param name="text">Content to be displayed.</param>
    public void SetDuplicateWarningText(string text) {
        duplicateWarningText.text = text;
    }
}
