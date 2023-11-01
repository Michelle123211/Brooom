using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class NavigationSteering : MonoBehaviour {

	protected GameObject agent;
	protected Vector3 targetPosition;

	protected bool isActive = true;

	public void Initialize(GameObject agent) {
		this.agent = agent;
	}

	public void SetTargetPosition(Vector3 position) {
		this.targetPosition = position;
	}

	public void StartSteering() {
		isActive = true;
	}

	public void StopSteering() {
		isActive = false;
	}

	public CharacterMovementValues GetCurrentMovementValue() {
		if (isActive) {
			return GetMovementToTargetPosition();
		} else
			return new CharacterMovementValues(ForwardMotion.Brake, YawMotion.None, PitchMotion.None);
	}

	protected abstract CharacterMovementValues GetMovementToTargetPosition();
}
