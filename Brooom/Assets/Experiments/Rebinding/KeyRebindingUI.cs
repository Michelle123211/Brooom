using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyRebindingUI : MonoBehaviour {
    [Tooltip("Names of the input actions whose binding should not be configured.")]
    public List<string> inputActionsToExclude = new List<string>();
    [Tooltip("Names of the input actions which should be displayed but not allowed to be modified.")]
    public List<string> inputActionsReadOnly = new List<string>();
    [Tooltip("Names which should be visible to the player in the UI for the specific action (if different from the action name).")]
    public List<InputActionName> actionReadableNames;

    [Tooltip("Prefab of UI for a single key binding.")]
    [SerializeField] private KeyBindingUI keyBinding;
    [Tooltip("Transform which is a parent of the key binding UIs.")]
    [SerializeField] private Transform keyBindingParent;


    private List<InputAction> inputActions;

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
        inputActions = InputManager.Instance.inputActions;
        foreach (InputAction action in inputActions) {
            if (inputActionsToExclude.Contains(action.name)) {
                continue;
            } else {
                KeyBindingUI keyBindingInstance = Instantiate<KeyBindingUI>(keyBinding, keyBindingParent);
                string name = action.name;
                // Override redable names if provided
                foreach (InputActionName actionName in actionReadableNames) {
                    if (actionName.actionName == action.name) {
                        name = actionName.readableName;
                        break;
                    }
                }
                keyBindingInstance.Initialize(action, name);
                if (inputActionsReadOnly.Contains(action.name))
                    keyBindingInstance.MakeReadOnly();
            
            }
        }
	}
}

[System.Serializable]
public class InputActionName {
    [Tooltip("Name of the action in the InputActions asset assigned to the PlayerInput component.")]
    public string actionName;
    [Tooltip("Name of the action visible to the player in the rebinding UI.")]
    public string readableName;
}