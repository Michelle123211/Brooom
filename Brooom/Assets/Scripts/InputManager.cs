using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
	[Tooltip("Names which should be visible to the player in the UI for the specific action (if different from the action name).")]
	public List<InputActionName> actionReadableNames;


	private PlayerInput playerInput;

	// All input actions with their readable names (to be used e.g. from the rebinding menu)
	private List<InputActionData> inputActions = new List<InputActionData>();
	// Storing all the input actions together with their last value
	private Dictionary<string, InputActionValue<bool>> boolActions = new Dictionary<string, InputActionValue<bool>>();
	private Dictionary<string, InputActionValue<float>> floatActions = new Dictionary<string, InputActionValue<float>>();
	private Dictionary<string, InputActionValue<Vector2>> vector2Actions = new Dictionary<string, InputActionValue<Vector2>>();
	private Dictionary<string, InputActionValue<Vector3>> vector3Actions = new Dictionary<string, InputActionValue<Vector3>>();
	// TODO: Add more if necessary


	// The following methods are used from the outside to get values of the input actions
	public bool GetBoolValue(string actionName) {
		if (boolActions.TryGetValue(actionName, out InputActionValue<bool> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type bool found.");
		return false;
	}
	public float GetFloatValue(string actionName) {
		if (floatActions.TryGetValue(actionName, out InputActionValue<float> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type float found.");
		return 0;
	}
	public Vector2 GetVector2Value(string actionName) {
		if (vector2Actions.TryGetValue(actionName, out InputActionValue<Vector2> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type Vector2 found.");
		return Vector2.zero;
	}
	public Vector3 GetVector3Value(string actionName) {
		if (vector3Actions.TryGetValue(actionName, out InputActionValue<Vector3> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type Vector3 found.");
		return Vector3.zero;
	}
	// TODO: Add more if necessary


	private void Update() {
		UpdateInputs();
	}

	private void UpdateInputs() {
		// Update values of all the input actions
		foreach (InputActionValue<bool> actionValue in boolActions.Values) {
			// Handles only the "pressed right now" event (no "released" or "is being pressed")
			actionValue.value = actionValue.actionData.action.WasPressedThisFrame();
		}
		foreach (InputActionValue<float> actionValue in floatActions.Values) {
			actionValue.value = actionValue.actionData.action.ReadValue<float>();
		}
		foreach (InputActionValue<Vector2> actionValue in vector2Actions.Values) {
			actionValue.value = actionValue.actionData.action.ReadValue<Vector2>();
		}
		foreach (InputActionValue<Vector3> actionValue in vector3Actions.Values) {
			actionValue.value = actionValue.actionData.action.ReadValue<Vector3>();
		}
		// TODO: Add more types if necessary
	}

	private void SetupInputActions() {
		// Get all the actions
		foreach (InputAction action in playerInput.actions) {
			//switch (action.type) {
			//	case InputActionType.Button:
			//		break;
			//	case InputActionType.Value:
			//		break;
			//	case InputActionType.PassThrough:
			//		break;
			//}
			InputActionData actionData = new InputActionData(action);
			// Divide the actions according to their value type into corresponding dictionaries
			inputActions.Add(actionData);
			switch (action.expectedControlType) {
				case "Axis":
					floatActions.Add(action.name, new InputActionValue<float>(actionData));
					Debug.Log($"Action {action.name} added.");
					break;
				case "Delta":
				case "Vector2":
					vector2Actions.Add(action.name, new InputActionValue<Vector2>(actionData));
					Debug.Log($"Action {action.name} added.");
					break;
				case "Vector3":
					vector3Actions.Add(action.name, new InputActionValue<Vector3>(actionData));
					Debug.Log($"Action {action.name} added.");
					break;
				case "Button":
					boolActions.Add(action.name, new InputActionValue<bool>(actionData));
					Debug.Log($"Action {action.name} added.");
					break;
				// TODO: Add more types if necessary
			}
		}
		// Override readable names if provided
		foreach (InputActionName inputActionName in actionReadableNames) {
			if (boolActions.ContainsKey(inputActionName.actionName))
				boolActions[inputActionName.actionName].actionData.readableName = inputActionName.readableName;
			if (floatActions.ContainsKey(inputActionName.actionName))
				floatActions[inputActionName.actionName].actionData.readableName = inputActionName.readableName;
			if (vector2Actions.ContainsKey(inputActionName.actionName))
				vector2Actions[inputActionName.actionName].actionData.readableName = inputActionName.readableName;
			if (vector3Actions.ContainsKey(inputActionName.actionName))
				vector3Actions[inputActionName.actionName].actionData.readableName = inputActionName.readableName;
			// TODO: Add more if necessary
		}
	}

	private void Start() {
		playerInput = GetComponent<PlayerInput>();

		SetupInputActions();
		UpdateInputs();
	}

}

[System.Serializable]
public class InputActionName {
	[Tooltip("Name of the action in the InputActions asset assigned to the PlayerInput component.")]
	public string actionName;
	[Tooltip("Name of the action visible to the player in the rebinding UI.")]
	public string readableName;
}

public class InputActionData {
	public InputAction action;
	// name of the action visible to the player in the rebinding UI
	public string readableName;

	public InputActionData(InputAction action) {
		this.action = action;
		this.readableName = action.name;
	}
}

public class InputActionValue<T> {
	// input action with its readable name
	public InputActionData actionData;
	// value read in the last frame
	public T value;

	public InputActionValue(InputActionData actionData) {
		this.actionData = actionData;
		this.value = default(T);
	}
}
