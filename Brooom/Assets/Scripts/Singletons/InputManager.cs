using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// A singleton responsible for keeping track of all input actions from an Input Action Asset.
/// It updates input actions' values in each frame so that they are always up-to-date,
/// and provides methods for others to get current values of particular input actions.
/// It is also able to override a binding for an input action and store it persistently
/// so that it can be loaded the next time the game is started.
/// </summary>
public class InputManager : MonoBehaviourSingleton<InputManager>, ISingleton {

	/// <summary>A warning displayed when a <c>PlayerInput</c> component cannot be found.</summary>
	public const string playerInputNullError = "InputManager does not have a PlayerInput component. Do you access the InputManager after it was destroyed? Or did you forget to put InputManager prefab into the scene?";

	private PlayerInput playerInput;


	// Storing all the input actions together with their last value
	private Dictionary<string, InputActionValue<bool>> boolActions = new();
	private Dictionary<string, InputActionValue<float>> floatActions = new();
	private Dictionary<string, InputActionValue<Vector2>> vector2Actions = new();
	private Dictionary<string, InputActionValue<Vector3>> vector3Actions = new();
	// TODO: Add more if necessary


	#region Methods to get values of input actions

	/// <summary>
	/// Gets current value corresponding to the given input action of type <c>bool</c>.
	/// </summary>
	/// <param name="actionName">Name of the input action whose value to get.</param>
	/// <returns>Current <c>bool</c> value of the given input action, or <c>false</c> if the input action cannot be found.</returns>
	public bool GetBoolValue(string actionName) {
		if (boolActions.TryGetValue(actionName, out InputActionValue<bool> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type bool found.");
		return false;
	}
	/// <summary>
	/// Gets current value corresponding to the given input action of type <c>float</c>.
	/// </summary>
	/// <param name="actionName">Name of the input action whose value to get.</param>
	/// <returns>Current <c>float</c> value of the given input action, or <c>0</c> if the input action cannot be found.</returns>
	public float GetFloatValue(string actionName) {
		if (floatActions.TryGetValue(actionName, out InputActionValue<float> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type float found.");
		return 0;
	}
	/// <summary>
	/// Gets current value corresponding to the given input action of type <c>Vector2</c>.
	/// </summary>
	/// <param name="actionName">Name of the input action whose value to get.</param>
	/// <returns>Current <c>Vector2</c> value of the given input action, or <c>Vector2.zero</c> if the input action cannot be found.</returns>
	public Vector2 GetVector2Value(string actionName) {
		if (vector2Actions.TryGetValue(actionName, out InputActionValue<Vector2> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type Vector2 found.");
		return Vector2.zero;
	}
	/// <summary>
	/// Gets current value corresponding to the given input action of type <c>Vector3</c>.
	/// </summary>
	/// <param name="actionName">Name of the input action whose value to get.</param>
	/// <returns>Current <c>Vector3</c> value of the given input action, or <c>Vector3.zero</c> if the input action cannot be found.</returns></returns>
	public Vector3 GetVector3Value(string actionName) {
		if (vector3Actions.TryGetValue(actionName, out InputActionValue<Vector3> action))
			return action.value;
		Debug.LogWarning($"No action of the given name ({actionName}) and of type Vector3 found.");
		return Vector3.zero;
	}

	// TODO: Add more if necessary
	#endregion

	#region Singleton initialization
	public InputManager() { 
		// Singleton options override
		Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances | SingletonOptions.LazyInitialization;
	}

	/// <inheritdoc/>
	public void AwakeSingleton() {
	}

	/// <inheritdoc/>
	public void InitializeSingleton() {
		// Get PlayerInput
		playerInput = GetComponent<PlayerInput>();
		if (playerInput == null) {
			Debug.LogWarning(playerInputNullError);
			return;
		}
		// Initialize everything
		LoadBindings();
		SetupInputActions();
		UpdateInputs();
	}
	#endregion

	/// <summary>
	/// Persistently stores current input actions binding overrides.
	/// </summary>
	public void SaveBindings() {
		if (playerInput == null) {
			Debug.LogWarning(playerInputNullError);
			return;
		}
		string keyBindings = playerInput.actions.SaveBindingOverridesAsJson();
		SaveSystem.SaveKeyBindings(keyBindings);
	}
	/// <summary>
	/// Loads input actions binding overrides from a save file.
	/// </summary>
	public void LoadBindings() {
		if (playerInput == null) {
			Debug.LogWarning(playerInputNullError);
			return;
		}
		string keyBindings = SaveSystem.LoadKeyBindings();
		if (!string.IsNullOrEmpty(keyBindings)) { // there are some previously saved bindings
			playerInput.actions.LoadBindingOverridesFromJson(keyBindings);
		}
	}

	/// <summary>
	/// Gets <c>PlayerInput</c> component.
	/// </summary>
	/// <returns><c>PlayerInput</c> component, or <c>null</c> if not found.</returns>
	public PlayerInput GetPlayerInput() {
		if (playerInput == null) {
			Debug.LogWarning(playerInputNullError);
		}
		return playerInput;
	}

	/// <summary>
	/// Gets current binding text for the given action.
	/// </summary>
	/// <param name="actionName">Name of the input action whose binding to get.</param>
	/// <returns>Current binding text of the given action.</returns>
	public string GetBindingTextForAction(string actionName) {
		if (playerInput == null) return string.Empty;
		InputAction action = playerInput.actions.FindAction(actionName);
		// Get binding index
		int bindingIndex = 0;
		for (int i = 0; i < action.bindings.Count; i++) {
			if (action.bindings[i].isComposite) // composite binding has higher priority
				bindingIndex = i;
		}
		// Get binding text
		string bindingText = action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);
		return PrettifyBindingText(bindingText, deviceLayoutName);
	}

	/// <summary>
	/// Handles special cases of binding display strings to create better human readable text.
	/// Also handles localization, when necessary.
	/// </summary>
	/// <param name="bindingText">Original binding display string.</param>
	/// <param name="deviceLayoutName">Name of the device layout.</param>
	/// <returns>A new, better human readable binding display string.</returns>
	public string PrettifyBindingText(string bindingText, string deviceLayoutName) {
		// Handle special cases (to return better human readable text)
		if (deviceLayoutName == "Mouse" && bindingText == "Delta") {
			bindingText = LocalizationManager.Instance.GetLocalizedString("RebindingMiscMouse");
		}
		if (deviceLayoutName == "Mouse" && bindingText == "LMB") {
			bindingText = LocalizationManager.Instance.GetLocalizedString("RebindingMiscLMB");
		}
		if (deviceLayoutName == "Mouse" && bindingText == "RMB") {
			bindingText = LocalizationManager.Instance.GetLocalizedString("RebindingMiscRMB");
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

	// Updates values of all input actions
	private void Update() {
		UpdateInputs();
	}

	// Updates values of all input actions
	private void UpdateInputs() {
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

	// Adds all available input actions into corresponding Dictionaries (input action name is a key)
	private void SetupInputActions() {
		if (playerInput == null) {
			Debug.LogWarning(playerInputNullError);
			return;
		}
		// Go through all the actions
		foreach (InputAction action in playerInput.actions) {
			// Divide the actions according to their value type into corresponding dictionaries
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

}

/// <summary>
/// A class pairing an input action with its current value.
/// </summary>
/// <typeparam name="T">Type of the input action's value.</typeparam>
public class InputActionValue<T> {
	/// <summary>Input action with its readable name. Can be used to read its value.</summary>
	public InputAction action;
	/// <summary>Value read in the last frame.</summary>
	public T value;

	public InputActionValue(InputAction action) {
		this.action = action;
		this.value = default;
	}
}
