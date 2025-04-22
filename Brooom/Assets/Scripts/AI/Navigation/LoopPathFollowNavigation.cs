using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class determining opponents' movement values based on simple path-following navigation.
/// It uses <c>NavigationSteering</c> implementation for getting movement values leading to the next target position.
/// </summary>
[RequireComponent(typeof(BasicNavigationSteering))]
public class LoopPathFollowNavigation : CharacterInput {

	[Tooltip("Points on the path to follow, will be looped.")]
	public List<Vector3> pathPoints;

	[Tooltip("Agent starts moving on to the next point once it's closer than this distance to the current one.")]
	public float distanceThreshold = 3f;

	private int currentPointIndex = -1;

	// Unit handling steering to the target position
	private NavigationSteering steering;

	/// <inheritdoc/>
	public override CharacterMovementValues GetMovementInput() {
		return steering.GetCurrentMovementValue();
	}

	// Checks if the agent is close enough to the current target position
	private bool IsCloseEnough() {
		return Vector3.Distance(transform.position, pathPoints[currentPointIndex]) < distanceThreshold;
	}

	// Sets target position to the next point on the path
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
		// Draw spheres in the points of the path to visualize it
		foreach (var point in pathPoints) {
			Gizmos.DrawSphere(point, 0.5f);
		}
	}

}
