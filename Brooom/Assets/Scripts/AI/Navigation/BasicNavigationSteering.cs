using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationSteering : NavigationSteering {

	[Tooltip("Interval in seconds between movement direction reevaluations.")]
	public float refreshInterval = 0.01f;

	[Header("Angle thresholds")]
	[Tooltip("The current yaw direction is changed only if it is at least this angle from the direction to the target.")]
	public float yawAngleThreshold = 15f;
	[Tooltip("The current pitch direction is changed only if it is at least this angle from the direction to the target.")]
	public float pitchAngleThreshold = 5f;

	[Tooltip("If the angle between current direction and the direction to the target is below this angle, it is considered safe to continue forward in full speed.")]
	public float goodAngleThreshold = 45f;
	[Tooltip("If the angle between current direction and the direction to the target is greater than this angle, it is considered better to brake.")]
	public float badAngleThreshold = 90f;

	private float elapsedTime = 0;
	private ForwardValue lastForward = ForwardValue.None;

	public override CharacterMovementValue GetCurrentMovementValue() {

		if (elapsedTime > refreshInterval) {
			elapsedTime -= refreshInterval;
			CharacterMovementValue movement = RecomputeMovementValue();
			lastForward = movement.forward;
			return movement;
		} else {
			return new CharacterMovementValue {
				forward = lastForward, // we don't want to lower the speed
				yaw = YawValue.None,
				pitch = PitchValue.None
			};
		}
	}

	private CharacterMovementValue RecomputeMovementValue() {
		Vector3 startPosition = this.agent.transform.position;
		Vector3 targetDirection = targetPosition - startPosition;

		CharacterMovementValue movement = new CharacterMovementValue();

		// Yaw
		float yawAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, Vector3.up);
		if (yawAngle < -yawAngleThreshold) movement.yaw = YawValue.Right;
		else if (yawAngle > yawAngleThreshold) movement.yaw = YawValue.Left;
		else movement.yaw = YawValue.None;

		// Pitch
		float pitchAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, this.agent.transform.right);
		if (pitchAngle < -pitchAngleThreshold) movement.pitch = PitchValue.Down;
		else if (pitchAngle > pitchAngleThreshold) movement.pitch = PitchValue.Up;
		else movement.pitch = PitchValue.None;

		// Forward
		float absYawAngle = Mathf.Abs(yawAngle);
		if (absYawAngle < goodAngleThreshold) movement.forward = ForwardValue.Forward; // if the current direction is reasonable, go forward
		else if (absYawAngle > badAngleThreshold) movement.forward = ForwardValue.Brake; // if it is entirely wrong, brake
		else movement.forward = ForwardValue.None; // otherwise do nothing


		Debug.Log($"Current movement: ({movement.forward}, {movement.yaw}, {movement.pitch})");

		return movement;
	}

	private void Update() {
		elapsedTime += Time.deltaTime;
	}

}
