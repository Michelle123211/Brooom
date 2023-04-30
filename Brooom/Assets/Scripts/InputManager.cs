using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
	public float ForwardInput { get; private set; }
	public float TurnInput { get; private set; }
	public float PitchInput { get; private set; }
	
	public Vector2 LookInput { get; private set; }
	public bool ViewPressed { get; private set; }

	//public bool CastSpellPressed { get; private set; }
	//public float SwitchSpellInput { get; private set; }

	//public bool RestartPressed { get; private set; }
	//public bool PausePressed { get; private set; }


	private PlayerInput playerInput;

	private InputAction forwardAction, turnAction, pitchAction;
	private InputAction lookAction, viewAction;
	//private InputAction castSpellAction, switchSpellAction;
	//private InputAction restartAction, pauseAction;

	private void Update() {
		UpdateInputs();
	}

	private void UpdateInputs() {
		ForwardInput = forwardAction.ReadValue<float>();
		TurnInput = turnAction.ReadValue<float>();
		PitchInput = pitchAction.ReadValue<float>();

		LookInput = lookAction.ReadValue<Vector2>();
		ViewPressed = viewAction.WasPressedThisFrame();

		//CastSpellPressed = castSpellAction.WasPressedThisFrame();
		//SwitchSpellInput = switchSpellAction.ReadValue<float>();

		//RestartPressed = restartAction.WasPressedThisFrame();
		//PausePressed = pauseAction.WasPressedThisFrame();
	}

	private void SetupInputActions() {
		forwardAction = playerInput.actions["Forward"];
		if (forwardAction == null) Debug.Log("Forward Action is null.");
		turnAction = playerInput.actions["Turn"];
		pitchAction = playerInput.actions["Pitch"];
		lookAction = playerInput.actions["Look"];
		viewAction = playerInput.actions["View"];
		//castSpellAction = playerInput.actions["CastSpell"];
		//switchSpellAction = playerInput.actions["SwitchSpell"];
		//restartAction = playerInput.actions["Restart"];
		//pauseAction = playerInput.actions["Pause"];
	}

	private void Start() {
		playerInput = GetComponent<PlayerInput>();

		SetupInputActions();
		UpdateInputs();
	}

}
