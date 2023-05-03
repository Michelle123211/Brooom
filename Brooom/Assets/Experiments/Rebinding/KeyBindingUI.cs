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
    public void Initialize(InputAction action, string name) {
        // Update the UI
        this.action = action;
        //bindingIndex = action.GetBindingIndexForControl(action.controls[0]); // controls[0] - first is the currently active control
        bindingIndex = 0;
        actionNameText.text = name;
        for (int i = 0; i < action.bindings.Count; i++) {
            if (action.bindings[i].isComposite) // composite binding has higher priority
                bindingIndex = i;
        }
        UpdateBindingText();
    }

    // Makes the binding not configurable
    public void MakeReadOnly() {
        rebindingButton.interactable = false;
        resetButton.SetActive(false);
    }

    public void StartRebinding() {
        // Deactivate the action we are going to rebind
        action.Disable();


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

    private void PerformSingleRebinding(int bindingIndex, bool allCompositeParts = false) {
        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Escape") // TODO: Check the correct path for Escape
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(operation => {
                RebindFinished();
            })
            .OnComplete(operation => {
                RebindFinished();
                action.Enable();
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
        // If composite, remove overrides from part bindings
        if (action.bindings[bindingIndex].isComposite) {
            for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                action.RemoveBindingOverride(i);
            }
        } else {
            action.RemoveBindingOverride(bindingIndex);
        }
        UpdateBindingText();
    }

    // Updates the UI to display the current binding
    private void UpdateBindingText() {
        rebindingButtonText.text = action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);
        //rebindingButtonText.text = InputControlPath.ToHumanReadableString(
        //    action.bindings[bindingIndex].effectivePath,
        //    InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    private void RebindFinished() {
        // Update the UI
        UpdateBindingText();
        waitingForInputText.SetActive(false);
        rebindingButton.gameObject.SetActive(true);
        rebindOverlay.SetActive(false);

        // Dispose the allocated memory
        rebindingOperation.Dispose();
        rebindingOperation = null;
    }

    private void RebindComplete() {
        // Update the UI
        UpdateBindingText();
        waitingForInputText.SetActive(false);
        rebindingButton.gameObject.SetActive(true);
        rebindOverlay.SetActive(false);

        // Dispose the allocated memory
        rebindingOperation.Dispose();
        rebindingOperation = null;

        // Enable the action again
        action.Enable();
    }
}
