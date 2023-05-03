using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    [SerializeField] private GameObject resetButton;

    // An overlay displayed over the UI while waiting for an input
    private GameObject rebindOverlay;
    // A label in the overlay which is used to display what was pressed already
    private TextMeshProUGUI rebindOverlayText;

    // The parent object managing everything
    KeyRebindingUI keyRebindingUI;

    // The assigned input action
    private InputAction action;
    private int bindingIndex;

    // For storing information about an ongoing rebinding operation
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    public void SetRebindOverlay(GameObject rebindOverlay, TextMeshProUGUI rebindOverlayText) {
        this.rebindOverlay = rebindOverlay;
        this.rebindOverlayText = rebindOverlayText;
    }

    // Updates the UI according to the action
    public void Initialize(KeyRebindingUI keyRebindingUI, InputAction action, string name) {
        this.keyRebindingUI = keyRebindingUI;
        this.action = action;
        // Get binding index
        bindingIndex = 0;
        for (int i = 0; i < action.bindings.Count; i++) {
            if (action.bindings[i].isComposite) // composite binding has higher priority
                bindingIndex = i;
        }
        // Update UI
        actionNameText.text = name;
        UpdateBindingText();
    }

    // Makes the binding not configurable
    public void MakeReadOnly() {
        rebindingButton.interactable = false;
        resetButton.SetActive(false);
    }

    public void StartRebinding() {
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

    // Updates the UI to display the current binding
    public void UpdateBindingText() {
        rebindingButtonText.text = action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);
    }

    private void PerformSingleRebinding(int bindingIndex, bool allCompositeParts = false) {
        // Deactivate the action we are going to rebind
        action.Disable();

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithControlsExcluding("<Mouse>")
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(operation => {
                action.Enable();
                UpdateUIAfterRebind();
                UpdateBindingText();
                CleanUp();
            })
            .OnComplete(operation => {
                action.Enable();
                UpdateUIAfterRebind();
                // Check duplicate bindings
                if (CheckForDuplicateBindings(bindingIndex, allCompositeParts)) {
                    action.RemoveBindingOverride(bindingIndex); // remove the duplicate
                    CleanUp();
                    // Give it another try
                    PerformSingleRebinding(bindingIndex, allCompositeParts);
                    return;
                }
                UpdateBindingText();
                CleanUp();
                // If there are more composite parts, initiate a rebind for the next part
                if (allCompositeParts) {
                    int nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite) {
                        PerformSingleRebinding(nextBindingIndex, allCompositeParts: true);
                    }
                }
            });

        // If it's a part of a composite, show the name of the part in the UI
        string partName = "";
        if (action.bindings[bindingIndex].isPartOfComposite) { 
            partName = $"Binding '{action.bindings[bindingIndex].name}'. ";
        }

        // Toggle the UI
        rebindOverlayText.text =  !string.IsNullOrEmpty(rebindingOperation.expectedControlType)
            ? $"{partName}Waiting for {rebindingOperation.expectedControlType} input..."
            : $"{partName}Waiting for input...";
        rebindOverlay.SetActive(true);

        // Start rebinding
        rebindingOperation.Start();
    }

    public void ResetRebindingToDefault() {
        // TODO: Allows duplicates after reset

		// If composite, remove overrides from part bindings
		if (action.bindings[bindingIndex].isComposite) {
			for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
				action.RemoveBindingOverride(i);
			}
		} else {
			action.RemoveBindingOverride(bindingIndex);
		}

		// TODO: Use the following to handle duplicates (not working for composites though)
		// Cache the current binding
		//InputBinding newBinding = action.bindings[bindingIndex];
		//string oldOverridePath = newBinding.overridePath;
		//// Remove the current binding
		//action.RemoveBindingOverride(bindingIndex);
		//// Check all the actions for duplicates
		//foreach (InputAction otherAction in action.actionMap.actions) {
		//    if (otherAction == action) continue; // skip the same action
		//    for (int i = 0; i < otherAction.bindings.Count; i++) {
		//        InputBinding otherBinding = otherAction.bindings[i];
		//        if (otherBinding.overridePath == newBinding.path) { // other action has a binding override equal to the default binding of the current action
		//            // Swap the bindings - the other action will get the override of the current action we are jsut resetting
		//            otherAction.ApplyBindingOverride(i, oldOverridePath);
		//            keyRebindingUI.RefreshAllBindings();
		//        }
		//    }
		//}

		UpdateBindingText();
    }

    private bool CheckForDuplicateBindings(int bindingIndex, bool allCompositeParts = false) {
        InputBinding lastBinding = action.bindings[bindingIndex];
        // Check bindings in all the other actions
        foreach (InputBinding otherBinding in action.actionMap.bindings) {
            if (otherBinding.action == lastBinding.action) continue; // skip the same action
            if (otherBinding.effectivePath == lastBinding.effectivePath) { // the same binding
                Debug.Log($"Duplicate binding found ({lastBinding.effectivePath}) for actions '{otherBinding.action}' and '{lastBinding.action}'.");
                return true;
            }
        }
        // Check all the composite parts up to the current one
        for (int i = this.bindingIndex + 1; i < bindingIndex; i++) {
            if (action.bindings[i].effectivePath == lastBinding.overridePath) {
                Debug.Log($"Duplicate binding found ({lastBinding.effectivePath}) in composite parts of action '{lastBinding.action}'.");
                return true;
            }
        }

        return false;
    }

    private void UpdateUIAfterRebind() {
        // Update the UI
        waitingForInputText.SetActive(false);
        rebindingButton.gameObject.SetActive(true);
        rebindOverlay.SetActive(false);

    }

    private void CleanUp() {
        // Dispose the allocated memory
        rebindingOperation.Dispose();
        rebindingOperation = null;
    }
}
