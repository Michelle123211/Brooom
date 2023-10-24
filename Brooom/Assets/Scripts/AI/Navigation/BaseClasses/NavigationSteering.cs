using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationSteering : MonoBehaviour {

	protected GameObject agent;
	protected Vector3 targetPosition;

	public void Initialize(GameObject agent) {
		this.agent = agent;
	}

	public void SetTargetPosition(Vector3 position) {
		this.targetPosition = position;
	}

	public abstract CharacterMovementValue GetCurrentMovementValue();
}
