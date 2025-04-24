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

/// <summary>
/// A class representing a direction in which a character should move.
/// It contains values for three possible axes of movement.
/// </summary>
public class CharacterMovementValues {

	// Specifying directions
	/// <summary>Direction of movement along the forward axis (i.e. forward, brake, none).</summary>
	public ForwardMotion forwardMotion;
	/// <summary>Direction of movement around the yaw axis (i.e. left, right, none).</summary>
	public YawMotion yawMotion;
	/// <summary>Direction of movement around the pitch axis (i.e. up, down, none).</summary>
	public PitchMotion pitchMotion;

	// Intensity of motion in the given direction (between 0 and 1)
	/// <summary>Intensity of motion along the forward axis (between 0 and 1).</summary>
	public float forwardValue = 1f;
	/// <summary>Intensity of motion around the yaw axis (between 0 and 1).</summary>
	public float yawValue = 1f;
	/// <summary>Intensity of motion around the pitch axis (between 0 and 1).</summary>
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

/// <summary>
/// Different possible motions along the forward axis.
/// </summary>
public enum ForwardMotion {
	Brake = -1,
	None = 0,
	Forward = 1
}

/// <summary>
/// Different possible motions around the yaw axis.
/// </summary>
public enum YawMotion { 
	Left = -1,
	None = 0,
	Right = 1
}

/// <summary>
/// Different possible motions around the pitch axis.
/// </summary>
public enum PitchMotion { 
	Up = -1,
	None = 0,
	Down = 1
}