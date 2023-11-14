using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(RaycastCollisionDetection))]
public class BasicNavigationSteering : NavigationSteering {

	[Header("Angle thresholds")]
	[Tooltip("The current yaw direction is changed only if it is at least this angle from the direction to the target.")]
	[SerializeField] float yawAngleThreshold = 15f;
	[Tooltip("The current pitch direction is changed only if it is at least this angle from the direction to the target.")]
	[SerializeField] float pitchAngleThreshold = 5f;

	[Tooltip("If the angle between current direction and the direction to the target is greater than this angle, it is considered better to brake.")]
	[SerializeField] float brakeAngleThreshold = 90f;

	[Header("Input curves")]
	[Tooltip("Yaw input value depending on the angle.")]
	[SerializeField] AnimationCurve yawInputCurve;
	[Tooltip("Pitch input value depending on the angle.")]
	[SerializeField] AnimationCurve pitchInputCurve;
	[Tooltip("Forward input value depending on the angle.")]
	[SerializeField] AnimationCurve forwardInputCurveAngle;
	[Tooltip("Forward input value depending on distance to the target.")]
	[SerializeField] AnimationCurve forwardInputCurveDistance;

	[Header("Collisions")]
	[Tooltip("If the agent is closer to the target position than this value, they will not try to avoid collisions at all.")]
	[SerializeField] float collisionsMinDistance = 3;
	[Tooltip("If the agent is farther to the target position than this value, they will give full weight to avoiding collisions.")]
	[SerializeField] float collisionsMaxDistance = 30;
	[Tooltip("Directions with weight lower than this (between 0 and 1) will not be considered possible (to significant collisions would occur).")]
	[SerializeField] float directionWeightThreshold = 0.1f;
	[SerializeField] bool debugLogs = false;

	[Header("Skill-based mistakes")]
	[Tooltip("The curve describes mapping from speed mistake probability to percentage of maximum speed which is used to go forward.")]
	[SerializeField] AnimationCurve speedBasedOnMistakeProbability;


	private RaycastCollisionDetection collisionDetection;

	protected override CharacterMovementValues GetMovementToTargetPosition() {
		CharacterMovementValues movement = ComputeMovementValues();
		movement = AdjustMovementToAvoidCollisions(movement);
		// Slow down based on the probability of speed mistakes
		if (movement.forwardMotion == ForwardMotion.Forward) {
			movement.forwardValue *= speedBasedOnMistakeProbability.Evaluate(agentSkillLevel.GetSpeedMistakeProbability());
		}
		return movement;
	}

	private CharacterMovementValues ComputeMovementValues() {
		Vector3 startPosition = this.agent.transform.position;
		Vector3 targetDirection = targetPosition - startPosition;
		float distance = Vector3.Distance(startPosition, targetPosition);

		CharacterMovementValues movement = new CharacterMovementValues();

		// Yaw
		float yawAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, Vector3.up);
		float yawAngleAbs = Mathf.Abs(yawAngle);
		if (yawAngle < -yawAngleThreshold) movement.yawMotion = YawMotion.Right;
		else if (yawAngle > yawAngleThreshold) movement.yawMotion = YawMotion.Left;
		else movement.yawMotion = YawMotion.None;
		movement.yawValue = yawInputCurve.Evaluate(yawAngleAbs);

		// Pitch
		float pitchAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, this.agent.transform.right);
		float pitchAngleAbs = Mathf.Abs(pitchAngle);
		if (pitchAngle < -pitchAngleThreshold) movement.pitchMotion = PitchMotion.Down;
		else if (pitchAngle > pitchAngleThreshold) movement.pitchMotion = PitchMotion.Up;
		else movement.pitchMotion = PitchMotion.None;
		movement.pitchValue = pitchInputCurve.Evaluate(pitchAngleAbs);

		// Forward
		if (yawAngleAbs > brakeAngleThreshold) movement.forwardMotion = ForwardMotion.Brake; // if the current direction is entirely wrong, brake
		else {
			movement.forwardMotion = ForwardMotion.Forward;
			movement.forwardValue = Mathf.Max(forwardInputCurveAngle.Evaluate(yawAngleAbs), forwardInputCurveDistance.Evaluate(distance));
		}

		return movement;
	}

	private CharacterMovementValues AdjustMovementToAvoidCollisions(CharacterMovementValues movement) {
		Vector3 startPosition = this.agent.transform.position;
		Vector3 targetDirection3 = (targetPosition - startPosition);
		Vector2 targetDirection = new Vector2(targetDirection3.x, targetDirection3.y).normalized; // only left/right, up/down
		float distance = Vector3.Distance(startPosition, targetPosition);

		List<CollisionInfo> collisions = collisionDetection.GetListOfCollisions();
		// If there are no collisions to avoid, simply continue in the original direction
		if (collisions == null || collisions.Count == 0) return movement;
		if (debugLogs)
			Debug.Log("Collisions detected.");
		// Create list of all possible directions (only left/right, up/down)
		List<FlightDirection> possibleDirections = new List<FlightDirection>();
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if (i == 0 || j == 0) // only 5 directions
					possibleDirections.Add(new FlightDirection(new Vector2(i, j)));
			}
		}
		// Lower weight of directions leading to collision (depending on the collision distance - smaller distance, smaller weight)
		foreach (var collision in collisions) {
			foreach (var direction in possibleDirections) {
				if (direction.direction == collision.direction) {
					direction.weight *= collision.distance;
				}
			}
		}
		// Lower weight of directions based on the distance to target position (smaller distance, smaller weight) - so that the agent is more focused on the target the closer it is
		float targetDistanceWeight = (Mathf.Clamp(distance, collisionsMinDistance, collisionsMaxDistance) - collisionsMinDistance) / (collisionsMaxDistance - collisionsMinDistance);
		foreach (var direction in possibleDirections) {
			direction.weight *= targetDistanceWeight;
		}
		// Remove all directions with too small weight
		for (int i = possibleDirections.Count - 1; i >= 0; i--) {
			if (possibleDirections[i].weight < directionWeightThreshold)
				possibleDirections.RemoveAt(i);
		}
		// If not possible to avoid collisions, keep the direction to the target as is
		if (possibleDirections.Count == 0) return movement;
		// From the remaining directions choose the one which is closest to the direction to target
		FlightDirection closestDirection = null;
		float closestDistance = float.MaxValue;
		foreach (var direction in possibleDirections) { 
			float directionDistance = Vector2.Distance(direction.direction.normalized, targetDirection);
			if (directionDistance < closestDistance) {
				closestDistance = directionDistance;
				closestDirection = direction;
			}
		}
		// Combine direction to target with the direction to avoid collisions (take their average)
		float yaw = ((int)movement.yawMotion * movement.yawValue + closestDirection.direction.x * closestDirection.weight) / 2f;
		float pitch = ((int)movement.pitchMotion * movement.pitchValue + closestDirection.direction.y * closestDirection.weight) / 2f;
		movement.yawMotion = (YawMotion)Mathf.RoundToInt(yaw / Mathf.Abs(yaw));
		movement.yawValue = Mathf.Abs(yaw);
		movement.pitchMotion = (PitchMotion)Mathf.RoundToInt(pitch / Mathf.Abs(pitch));
		movement.pitchValue = Mathf.Abs(pitch);

		return movement;
	}

	private void Start() {
		collisionDetection = GetComponent<RaycastCollisionDetection>();
	}

}

internal class FlightDirection {
	public Vector2 direction; // omiting forward direction (just left/right, up/down)
	public float weight;

	public FlightDirection(Vector2 direction) {
		this.direction = direction;
		weight = 1;
	}
}