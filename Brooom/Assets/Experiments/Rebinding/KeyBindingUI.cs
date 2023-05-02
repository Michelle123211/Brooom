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


    // The assigned input action
    private InputAction action;

    // For storing information about an ongoing rebinding operation
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    // Updates the UI according to the action
    public void Initialize(InputAction action, string name) {
        // Update the UI
        this.action = action;
        actionNameText.text = name;
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

        // Toggle the UI
        rebindingButton.gameObject.SetActive(false);
        waitingForInputText.SetActive(true);

        rebindingOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("Escape") // TODO: Check the correct path for Escape
            .OnMatchWaitForAnother(0.1f) // short delay to give the program a chance to search for any other input which may be better match
            .OnComplete(operation => RebindComplete()) // when the rebinding is complete (input was given)
            .Start(); // to set it all up
    }

    public void ResetRebinding() { 
        // TODO
    }

    // Updates the UI to display the current binding
    private void UpdateBindingText() {
        int bindingIndex = action.GetBindingIndexForControl(action.controls[0]); // controls[0] - first is the currently active control
        rebindingButtonText.text = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    private void RebindComplete() {
        // Update the UI
        UpdateBindingText();
        waitingForInputText.SetActive(false);
        rebindingButton.gameObject.SetActive(true);

        // Dispose the allocated memory
        rebindingOperation.Dispose();

        // Enable the action again
        action.Enable();
    }
}
