using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CharacterInput : MonoBehaviour {
	public abstract CharacterMovementValue GetMovementInput();
}

public struct CharacterMovementValue {
	public ForwardValue forward;
	public YawValue yaw;
	public PitchIValue pitch;
}

public enum ForwardValue {
	Brake = -1,
	None = 0,
	Forward = 1
}

public enum YawValue { 
	Left = -1,
	None = 0,
	Right = 1
}

public enum PitchIValue { 
	Up = -1,
	None = 0,
	Down = 1
}