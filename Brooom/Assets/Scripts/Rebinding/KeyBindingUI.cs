using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Text;

public class KeyBindingUI : MonoBehaviour {

    [Tooltip("Label displaying the readable name of the action.")]
    [SerializeField] private TextMeshProUGUI actionNameText;
    [Tooltip("An object with a button to start rebinding the key.")]
    [SerializeField] private Button rebindingButton;
    [Tooltip("Label on the button starting rebinding.")]
    [SerializeField] private TextMeshProUGUI rebindingButtonText;
    [Tooltip("An object displayed when a rebinding is in process.")]
    [SerializeField] private GameObject waitingForInputText;
    [Tooltip("An object with a button to reset the binding.")]
    [SerializeField] private Button resetButton;
    [Tooltip("An object representing a warning that there is a duplicate binding.")]
    [SerializeField] private GameObject duplicateWarning;
    [Tooltip("Tooltip for displaying warning about duplicate binding.")]
    [SerializeField] SimpleTooltip duplicateWarningTooltip;

    [SerializeField] private bool logDebuggingMessages = true;

    // An overlay displayed over the UI while waiting for an input
    private RebindOverlayUI rebindOverlay;

    // The parent object managing everything
    KeyRebindingUI keyRebindingUI;

    // The assigned input action
    private InputAction action;
    private int bindingIndex;

    // Human readable names of the different composite parts
    private string[] readablePartNames;

    // For storing information about an ongoing rebinding operation
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    public void SetRebindOverlay(RebindOverlayUI rebindOverlay) {
        this.rebindOverlay = rebindOverlay;
    }

    // Updates the UI according to the action
    public void Initialize(KeyRebindingUI keyRebindingUI, InputAction action, string name, bool isReadonly = false) {
        this.keyRebindingUI = keyRebindingUI;
        this.action = action;
        // Get binding index
        bindingIndex = 0;
        for (int i = 0; i < action.bindings.Count; i++) {
            if (action.bindings[i].isComposite) // composite binding has higher priority
                bindingIndex = i;
        }
        // Parse individual parts names from the action name
        readablePartNames = name.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < readablePartNames.Length; i++) {
            readablePartNames[i] = readablePartNames[i].Trim();
        }
        // Update UI
        if (!isReadonly) {
            rebindingButton.interactable = true;
            resetButton.gameObject.SetActive(true);
        }
        actionNameText.text = name;
        UpdateResetButtonInteractibility();
        UpdateBindingText();
    }

    // Updates the UI to display the current binding
    public void UpdateBindingText() {
        rebindingButtonText.text = GetBindingText(this.bindingIndex);
    }

    public void StartRebinding() {
        rebindOverlay.SetAlreadyBoundText(" ");
        // For composite go through all of its parts
        if (action.bindings[bindingIndex].isComposite) {
            int firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite) {
                PerformSingleRebinding(firstPartIndex, allCompositeParts: true);
            }
        } else {
            PerformSingleRebinding(bindingIndex);
        }
    }

    public void UpdateDuplicateWarning() {
        StringBuilder duplicateBindings = new StringBuilder();
		// Check bindings of all the other actions in the same action map
		foreach (InputBinding otherBinding in action.actionMap.bindings) {
			if (otherBinding.action == action.bindings[bindingIndex].action) continue; // skip the same action
            // Go through all bindings for this action
            for (int thisBindingIndex = 0; thisBindingIndex < action.bindings.Count; thisBindingIndex++) {
                if (action.bindings[thisBindingIndex].isComposite) continue;
                if (otherBinding.effectivePath == action.bindings[thisBindingIndex].effectivePath) { // the same binding
                    if (duplicateBindings.Length > 0) duplicateBindings.Append("\n");
                    string otherActionName = LocalizationManager.Instance.GetLocalizedString($"Action{otherBinding.action}"); // localized readable name
                    duplicateBindings.Append(string.Format(LocalizationManager.Instance.GetLocalizedString("RebindingTooltipDuplicate"), GetBindingText(thisBindingIndex), otherActionName));
                }
            }
		}
        if (duplicateBindings.Length > 0) {
            duplicateWarningTooltip.text = duplicateBindings.ToString();
            duplicateWarning.TweenAwareEnable();
        } else duplicateWarning.TweenAwareDisable();
    }

    public string GetBindingText(int bindingIndex) {
        string bindingText = action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);

        // Handle special cases (to display better human readable text)
        if (deviceLayoutName == "Mouse" && bindingText == "Delta") {
            bindingText = LocalizationManager.Instance.GetLocalizedString("RebindingMiscMouse");
        }
        if (deviceLayoutName == "Mouse" && bindingText == "LMB") {
            bindingText = LocalizationManager.Instance.GetLocalizedString("RebindingMiscLMB");
        }
        if (bindingText == "Scroll Up/Scroll Down") {
            bindingText = LocalizationManager.Instance.GetLocalizedString("RebindingMiscScroll");
        }
        if (bindingText.Contains("Space")) {
            string localizedSpace = LocalizationManager.Instance.GetLocalizedString("RebindingMiscSpace");
            bindingText = bindingText.Replace("Space", localizedSpace);
        }

        return bindingText;
    }

    private void PerformSingleRebinding(int bindingIndex, bool allCompositeParts = false) {
        // Deactivate the action we are going to rebind
        action.Disable();

        string partName = "";
        if (action.bindings[bindingIndex].isPartOfComposite) {
            int index = bindingIndex - this.bindingIndex - 1;
            if (index < readablePartNames.Length) // readable name of the part is provided
                partName = readablePartNames[index];
            else
                partName = action.bindings[bindingIndex].name; // default
        } else
            partName = readablePartNames[0];

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithControlsExcluding("<Mouse>")
            .OnMatchWaitForAnother(0.1f) // short delay to give the program a chance to search for any other input which may be better match
            .OnCancel(operation => {
                // Activate the action back
                action.Enable();
                // Update UI
                UpdateUIAfterRebind();
                UpdateBindingText();
                rebindOverlay.SetDuplicateWarningText(" ");
                CleanUp();
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
            })
            .OnComplete(operation => {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.KeyDown);
                // Activate the action back
                action.Enable();
                // Update UI - part 1
                UpdateUIAfterRebind();
                // Check duplicate bindings
                string bindingText = action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);
                if (CheckForDuplicateBindingsWhileRebinding(bindingIndex)) { // only in this rebinding session
                    string duplicateString = LocalizationManager.Instance.GetLocalizedString("RebindingLabelDuplicate");
                    rebindOverlay.SetDuplicateWarningText(string.Format(duplicateString, bindingText));
                    action.RemoveBindingOverride(bindingIndex); // remove the duplicate
                    CleanUp();
                    // Give it another try
                    PerformSingleRebinding(bindingIndex, allCompositeParts);
                    return;
                }
                rebindOverlay.SetDuplicateWarningText(" ");
                // Set label containing already bound parts
                if (allCompositeParts && bindingIndex != this.bindingIndex + 1) // NOT the first part of the composite
                    rebindOverlay.SetAlreadyBoundText($", '{partName}' = {bindingText}", true);
                else // the first part of the composite or not composite
                    rebindOverlay.SetAlreadyBoundText($"'{partName}' = {bindingText}");
                // Update UI - part 2
                UpdateBindingText();
                keyRebindingUI.UpdateAllDuplicateWarnings();
                CleanUp();
                // If there are more composite parts, initiate a rebind for the next part
                if (allCompositeParts) {
                    int nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite) {
                        PerformSingleRebinding(nextBindingIndex, allCompositeParts: true);
                    }
                }
            });

        // Toggle the UI
        rebindingButton.gameObject.SetActive(false);
        waitingForInputText.SetActive(true);
        string waitingString = LocalizationManager.Instance.GetLocalizedString("RebindingLabelWaiting");
        rebindOverlay.SetWaitingForInputText(string.Format(waitingString, partName));
        rebindOverlay.gameObject.TweenAwareEnable();

        // Start rebinding
        rebindingOperation.Start();
    }

    public void ResetRebindingToDefault() {
		// If composite, remove overrides from part bindings
		if (action.bindings[bindingIndex].isComposite) {
			for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                action.RemoveBindingOverride(i);
			}
		} else {
            action.RemoveBindingOverride(bindingIndex);
		}

		UpdateBindingText();
        UpdateResetButtonInteractibility();
        keyRebindingUI.UpdateAllDuplicateWarnings();
    }

    private bool CheckForDuplicateBindingsWhileRebinding(int bindingIndex) {
        InputBinding lastBinding = action.bindings[bindingIndex];
        // Check all the composite parts up to the current one
        for (int i = this.bindingIndex + 1; i < bindingIndex; i++) {
            if (action.bindings[i].effectivePath == lastBinding.overridePath) {
                if (logDebuggingMessages) Debug.Log($"Duplicate binding found ({lastBinding.effectivePath}) in composite parts of action '{lastBinding.action}'.");
                return true;
            }
        }
        return false;
    }

    private void UpdateUIAfterRebind() {
        // Update the UI
        UpdateResetButtonInteractibility();
        waitingForInputText.SetActive(false);
        rebindingButton.gameObject.SetActive(true);
        rebindOverlay.gameObject.TweenAwareDisable();
    }

    private void UpdateResetButtonInteractibility() {
        resetButton.interactable = false;
        if (action.bindings[bindingIndex].hasOverrides) {
            resetButton.interactable = true;
        } else if (action.bindings[bindingIndex].isComposite) {
            for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                if (action.bindings[i].hasOverrides) {
                    resetButton.interactable = true;
                }
            }
        }
    }

    private void CleanUp() {
        // Dispose the allocated memory
        rebindingOperation.Dispose();
        rebindingOperation = null;
    }
}
