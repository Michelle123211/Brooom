using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


/// <summary>
/// A component managing all UI elements related to key bindings displayed in Settings.
/// It instantiates UI for single key bindings which allow to define binding override for a particular action.
/// </summary>
public class KeyRebindingUI : MonoBehaviour {

    [Tooltip("Input actions whose binding should not be displayed at all.")]
    public List<InputActionReference> inputActionsToExclude = new List<InputActionReference>();
    [Tooltip("Names of the input actions which should be displayed but not allowed to be modified.")]
    public List<InputActionReference> inputActionsReadOnly = new List<InputActionReference>();

    [Tooltip("Name of the action map whose actions should be displayed for rebinding.")]
    public string actionMapToDisplay = "Game";

    [Tooltip("Prefab of UI for a single key binding.")]
    [SerializeField] private KeyBindingUI keyBinding;
    [Tooltip("Transform which is a parent of the key binding UIs.")]
    [SerializeField] private Transform keyBindingParent;

    [Tooltip("An overlay displayed over the UI while waiting for an input.")]
    [SerializeField] private RebindOverlayUI rebindOverlay;

    [SerializeField] private bool logDebuggingMessages = true;

    /// <summary>
    /// Toggles visibility of this whole GameObject
    /// </summary>
    public void ToggleVisibility() {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    /// <summary>
    /// Refreshes all bindings by going through all <c>KeyBindingUI</c> instances and updating their binding text displayed in a rebinding button.
    /// </summary>
    public void RefreshAllBindings() {
        KeyBindingUI[] children = GetComponentsInChildren<KeyBindingUI>();
        foreach (KeyBindingUI child in children) {
            child.UpdateBindingText();
        }
    }

    /// <summary>
    /// Goes through all <c>KeyBindingUI</c> instances and for each one updates its duplicate warning 
    /// (checks for duplicate bindings and shows/hides duplicate warning accordingly).
    /// </summary>
    public void UpdateAllDuplicateWarnings() {
        foreach (var keyBinding in keyBindingParent.GetComponentsInChildren<KeyBindingUI>())
            keyBinding.UpdateDuplicateWarning();
    }

    // Instantiates a new KeyBindingUI for each action available (which is not in a list of excluded actions), also initializes it
    private void CreateRebindingUIsForAllActions() {
        PlayerInput playerInput = InputManager.Instance.GetPlayerInput();
        if (playerInput == null) return;
        // Try to get the desired action map
        InputActionMap actionMap = playerInput.actions.FindActionMap(actionMapToDisplay);
        if (actionMap == null) {
            if (logDebuggingMessages) Debug.Log($"The given action map ({actionMapToDisplay}) does not exist.");
        }
        // Go through all the actions in the desired action map
        foreach (InputAction action in playerInput.actions.FindActionMap(actionMapToDisplay)) {
            // Exclude the action if necessary
            if (ShouldBeExcluded(action)) continue;
            // Add binding UI for the action
            KeyBindingUI keyBindingInstance = Instantiate<KeyBindingUI>(keyBinding, keyBindingParent);
            keyBindingInstance.SetRebindOverlay(rebindOverlay);
            string name = action.name;
            if (LocalizationManager.Instance.TryGetLocalizedString($"Action{action.name}", out string readableName)) { // Get localized readable name
                name = readableName;
            }
            keyBindingInstance.Initialize(this, action, name, ShouldBeReadOnly(action)); // make read-only if necessary
        }
    }

    // Returns true if the given action is among the ones to be excluded
    private bool ShouldBeExcluded(InputAction action) {
        foreach (InputActionReference actionToExclude in inputActionsToExclude) {
            if (actionToExclude.action.name == action.name) {
                return true;
            }
        }
        return false;
    }

    // Returns true if the binding of the given action should not be configurable (it will be displayed but not interactable)
    private bool ShouldBeReadOnly(InputAction action) {
        foreach (InputActionReference actionReadOnly in inputActionsReadOnly) {
            if (actionReadOnly.action.name == action.name) {
                return true;
            }
        }
        return false;
    }

	private void OnEnable() {
        // Refresh the list of the bindings
        UtilsMonoBehaviour.RemoveAllChildren(keyBindingParent); // remove all currently displayed
        CreateRebindingUIsForAllActions();
        UpdateAllDuplicateWarnings();
	}

	private void OnDisable() {
        InputManager.Instance.SaveBindings();
	}
}