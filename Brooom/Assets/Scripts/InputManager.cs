using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
	private PlayerInput playerInput;

	// All input actions
	[HideInInspector]
	public List<InputAction> inputActions = new List<InputAction>();

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


	public PlayerInput GetPlayerInput() {
		return playerInput;
	}


	private void Update() {
		UpdateInputs();
	}

	private void UpdateInputs() {
		// Update values of all the input actions
		foreach (InputActionValue<bool> actionValue in boolActions.Values) {
			// Handles only the "pressed right now" event (no "released" or "is being pressed")
			actionValue.value = actionValue.action.WasPressedThisFrame();
		}
		foreach (InputActionValue<float> actionValue in floatActions.Values) {
			actionValue.value = actionValue.action.ReadValue<float>();
		}
		foreach (InputActionValue<Vector2> actionValue in vector2Actions.Values) {
			actionValue.value = actionValue.action.ReadValue<Vector2>();
		}
		foreach (InputActionValue<Vector3> actionValue in vector3Actions.Values) {
			actionValue.value = actionValue.action.ReadValue<Vector3>();
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
			// Divide the actions according to their value type into corresponding dictionaries
			inputActions.Add(action);
			switch (action.expectedControlType) {
				case "Axis":
					floatActions.Add(action.name, new InputActionValue<float>(action));
					break;
				case "Delta":
				case "Vector2":
					vector2Actions.Add(action.name, new InputActionValue<Vector2>(action));
					break;
				case "Vector3":
					vector3Actions.Add(action.name, new InputActionValue<Vector3>(action));
					break;
				case "Button":
					boolActions.Add(action.name, new InputActionValue<bool>(action));
					break;
				// TODO: Add more types if necessary
			}
		}
	}

	private void Start() {
		playerInput = GetComponent<PlayerInput>();

		SetupInputActions();
		UpdateInputs();
	}

}

public class InputActionValue<T> {
	// input action with its readable name
	public InputAction action;
	// value read in the last frame
	public T value;

	public InputActionValue(InputAction action) {
		this.action = action;
		this.value = default(T);
	}
}
