using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class KeyRebindingUI : MonoBehaviour {
    [Tooltip("Input actions whose binding should not be displayed at all.")]
    public List<InputActionReference> inputActionsToExclude = new List<InputActionReference>();
    [Tooltip("Names of the input actions which should be displayed but not allowed to be modified.")]
    public List<InputActionReference> inputActionsReadOnly = new List<InputActionReference>();
    [Tooltip("Names which should be visible to the player in the UI for the specific action (if different from the action name).")]
    public List<InputActionName> actionReadableNames;

    [Tooltip("Name of the action map whose actions should be displayed for rebinding.")]
    public string actionMapToDisplay = "Game";

    [Tooltip("Prefab of UI for a single key binding.")]
    [SerializeField] private KeyBindingUI keyBinding;
    [Tooltip("Transform which is a parent of the key binding UIs.")]
    [SerializeField] private Transform keyBindingParent;

    [Tooltip("An overlay displayed over the UI while waiting for an input.")]
    [SerializeField] private RebindOverlayUI rebindOverlay;

    [SerializeField] private bool logDebuggingMessages = true;

    public void ToggleVisibility() {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public void RefreshAllBindings() {
        KeyBindingUI[] children = GetComponentsInChildren<KeyBindingUI>();
        foreach (KeyBindingUI child in children) {
            child.UpdateBindingText();
        }
    }

    // Removes all currently displayed KeyBindingUIs
    private void RemoveAllRebindingUIs() {
        for (int i = keyBindingParent.childCount - 1; i >= 0; i--) {
            Destroy(keyBindingParent.GetChild(i).gameObject);
        }
    }

    // Adds a new KeyBindingUI for all the actions
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
            string name = GetReadableActionName(action); // override redable name if provided
            keyBindingInstance.Initialize(this, action, name);
            // Make read-only if necessary
            if (ShouldBeReadOnly(action)) keyBindingInstance.MakeReadOnly();
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

    // Returns the readable name of the given action (if provided), otherwise the action name
    private string GetReadableActionName(InputAction action) {
        foreach (InputActionName actionName in actionReadableNames) {
            if (actionName.actionToRename.action.name == action.name) {
                return actionName.readableName;
            }
        }
        return action.name;
    }

    // Returns true if the binding of the given action should not be configurable
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
        RemoveAllRebindingUIs();
        CreateRebindingUIsForAllActions();
	}

	private void OnDisable() {
        InputManager.Instance.SaveBindings();
	}
}

[System.Serializable]
public class InputActionName {
    [Tooltip("Action in the InputActions asset assigned to the PlayerInput component whose name should be displayed differently.")]
    public InputActionReference actionToRename;
    [Tooltip("Name of the action visible to the player in the rebinding UI.")]
    public string readableName;
}