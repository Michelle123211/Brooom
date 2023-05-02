using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyRebindingUI : MonoBehaviour {
    [Tooltip("Input actions whose binding should not be displayed at all.")]
    public List<InputActionReference> inputActionsToExclude = new List<InputActionReference>();
    [Tooltip("Names of the input actions which should be displayed but not allowed to be modified.")]
    public List<InputActionReference> inputActionsReadOnly = new List<InputActionReference>();
    [Tooltip("Names which should be visible to the player in the UI for the specific action (if different from the action name).")]
    public List<InputActionName> actionReadableNames;

    [Tooltip("Prefab of UI for a single key binding.")]
    [SerializeField] private KeyBindingUI keyBinding;
    [Tooltip("Transform which is a parent of the key binding UIs.")]
    [SerializeField] private Transform keyBindingParent;


    public void ToggleVisibility() {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnEnable() {
        // Refresh the list of bindings
        // First remove all
        for (int i = keyBindingParent.childCount - 1; i >= 0; i--) {
            Destroy(keyBindingParent.GetChild(i).gameObject);
        }
        // Then add the current ones
        foreach (InputAction action in InputManager.Instance.inputActions) {
            // Exclude the action if necessary
            bool exclude = false;
            foreach (InputActionReference actionToExclude in inputActionsToExclude) {
                if (actionToExclude.action.name == action.name) {
                    exclude = true;
                    break;
                }
            }
            if (exclude) continue;
            // Add binding UI for the action
            KeyBindingUI keyBindingInstance = Instantiate<KeyBindingUI>(keyBinding, keyBindingParent);
            string name = action.name;
            // Override redable names if provided
            foreach (InputActionName actionName in actionReadableNames) {
                if (actionName.actionToRename.action.name == action.name) {
                    name = actionName.readableName;
                    break;
                }
            }
            keyBindingInstance.Initialize(action, name);
            // Make read-only if necessary
            foreach (InputActionReference actionReadOnly in inputActionsReadOnly) {
                if (actionReadOnly.name == action.name) {
                    keyBindingInstance.MakeReadOnly();
                    break;
                }
            }
        }
	}
}

[System.Serializable]
public class InputActionName {
    [Tooltip("Action in the InputActions asset assigned to the PlayerInput component whose name should be displayed differently.")]
    public InputActionReference actionToRename;
    [Tooltip("Name of the action visible to the player in the rebinding UI.")]
    public string readableName;
}