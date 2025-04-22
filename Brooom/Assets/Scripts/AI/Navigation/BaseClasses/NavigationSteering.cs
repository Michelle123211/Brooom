using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A base class for opponents' navigation component which is responsible for handling steering to a given target point.
/// Different derived classes may adapt different strategies.
/// </summary>
public abstract class NavigationSteering : MonoBehaviour {

	/// <summary>Agent which is controlled by this component.</summary>
	protected GameObject agent;
	/// <summary>Agent is steered towards this position.</summary>
	protected Vector3 targetPosition;

	/// <summary>Whether the steering is active (agent is navigated towards target position) or not (agent is actively braking).</summary>
	protected bool isActive = true; // may be started and stopped

	/// <summary>
	/// Initializes everything necessary for steering the given agent.
	/// </summary>
	/// <param name="agent">Agent to which this component belongs.</param>
	public void Initialize(GameObject agent) {
		this.agent = agent;
		InitializeDerivedType();
	}

	/// <summary>
	/// Sets target position the agent should be steered towards from now on.
	/// </summary>
	/// <param name="position">Position the agent should be steered towards.</param>
	public void SetTargetPosition(Vector3 position) {
		this.targetPosition = position;
	}

	/// <summary>
	/// Activates steering, any consecutive <c>GetCurrentMovementValue()</c> calls will return movement values for steering the agent towards target position.
	/// </summary>
	public void StartSteering() {
		isActive = true;
	}
	/// <summary>
	/// Deactivates steering, any consecutive <c>GetCurrentMovementValue()</c> calls will return movement values for braking.
	/// </summary>
	public void StopSteering() {
		isActive = false;
	}

	/// <summary>
	/// Computes current movement values which should be used right now to steer towards current target position (stored in <c>targetPosition</c>).
	/// Takes into consideration whether steering is active, otherwise braking will be used as a movement values.
	/// </summary>
	/// <returns>Movement values as <c>CharacterMovementValues</c> for the agent to use.</returns>
	public CharacterMovementValues GetCurrentMovementValue() {
		if (isActive) {
			return GetMovementToTargetPosition();
		} else
			return new CharacterMovementValues(ForwardMotion.Brake, YawMotion.None, PitchMotion.None);
	}

	/// <summary>
	/// Initialize everything necessary for a specific implementation of steering to a target position.
	/// </summary>
	protected virtual void InitializeDerivedType() { }

	/// <summary>
	/// Computes movement values which should be used right now to steer towards current target position (stored in <c>targetPosition</c>).
	/// </summary>
	/// <returns>Movement values as <c>CharacterMovementValues</c> necessary to steer towards current target position.</returns>
	protected abstract CharacterMovementValues GetMovementToTargetPosition();
}
