using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class for determining character's movement values.
/// Derived classes are then specialized on player (getting movement values from input) or opponent (different strategies based on navigation).
/// </summary>
public abstract class CharacterInput : MonoBehaviour {
	/// <summary>
	/// Gets current movement values describing where the character wants to move.
	/// </summary>
	/// <returns><c>CharacterMovementValues</c> containing values for 3 different axes of movement.</returns>
	public abstract CharacterMovementValues GetMovementInput();
}

public class CharacterMovementValues {
	// Specifying directions
	public ForwardMotion forwardMotion;
	public YawMotion yawMotion;
	public PitchMotion pitchMotion;
	// Intensity of motion in the given direction (between 0 and 1)
	public float forwardValue = 1f;
	public float yawValue = 1f;
	public float pitchValue = 1f;

	public CharacterMovementValues()
		: this(ForwardMotion.None, YawMotion.None, PitchMotion.None) { }

	public CharacterMovementValues(ForwardMotion forward, YawMotion yaw, PitchMotion pitch)
		: this(forward, 1, yaw, 1, pitch, 1) { }

	public CharacterMovementValues(ForwardMotion forward, float forwardValue, YawMotion yaw, float yawValue, PitchMotion pitch, float pitchValue) {
		this.forwardMotion = forward;
		this.forwardValue = forwardValue;
		this.yawMotion = yaw;
		this.yawValue = yawValue;
		this.pitchMotion = pitch;
		this.pitchValue = pitchValue;
	}
}

public enum ForwardMotion {
	Brake = -1,
	None = 0,
	Forward = 1
}

public enum YawMotion { 
	Left = -1,
	None = 0,
	Right = 1
}

public enum PitchMotion { 
	Up = -1,
	None = 0,
	Down = 1
}