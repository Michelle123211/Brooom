using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationSteering))]
public class LoopPathFollowNavigation : CharacterInput {

	[Tooltip("Points on the path to follow, will be looped.")]
	public List<Vector3> pathPoints;
	[Tooltip("Starts moving on to the next point if closer than this distance to the current one.")]
	public float distanceThreshold = 3f;

	private int currentPointIndex = -1;

	private NavigationSteering steering;

	public override CharacterMovementValues GetMovementInput() {
		return steering.GetCurrentMovementValue();
	}

	private bool IsCloseEnough() {
		return Vector3.Distance(transform.position, pathPoints[currentPointIndex]) < distanceThreshold;
	}

	private void AdvanceToNextPoint() {
		currentPointIndex = (currentPointIndex + 1) % pathPoints.Count;
		steering.SetTargetPosition(pathPoints[currentPointIndex]);
	}

	private void Update() {
		if (IsCloseEnough()) {
			AdvanceToNextPoint();
		}
	}

	private void Start() {
		steering = GetComponent<NavigationSteering>();
		steering.Initialize(gameObject);
		currentPointIndex = -1;
		AdvanceToNextPoint();
	}

	private void OnDrawGizmosSelected() {
		foreach (var point in pathPoints) {
			Gizmos.DrawSphere(point, 0.5f);
		}
	}

}
