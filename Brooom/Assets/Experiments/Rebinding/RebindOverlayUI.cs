using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RebindOverlayUI : MonoBehaviour {
    [Tooltip("A label in the overlay which is used to display what part is the binding for.")]
    [SerializeField] private TextMeshProUGUI waitingForInputText;
    [Tooltip("A label for displaying what was input so far in the composite.")]
    [SerializeField] private TextMeshProUGUI alreadyBoundText;
    [Tooltip("A label with warning that the last binding is already in use.")]
    [SerializeField] private TextMeshProUGUI duplicateWarningText;

    public void SetWaitingForInputText(string text) {
        waitingForInputText.text = text;
    }

    public void SetAlreadyBoundText(string text, bool append = false) {
        if (append)
            alreadyBoundText.text += text;
        else
            alreadyBoundText.text = text;
    }

    public void SetDuplicateWarningText(string text) {
        duplicateWarningText.text = text;
    }
}
