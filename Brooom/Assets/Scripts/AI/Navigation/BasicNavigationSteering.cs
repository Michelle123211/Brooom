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
	private ForwardMotion lastForward = ForwardMotion.None;

	public override CharacterMovementValues GetCurrentMovementValue() {

		if (elapsedTime > refreshInterval) {
			elapsedTime -= refreshInterval;
			CharacterMovementValues movement = RecomputeMovementValue();
			lastForward = movement.forwardMotion;
			return movement;
		} else {
			return new CharacterMovementValues(lastForward, YawMotion.None, PitchMotion.None); // don't lower the forward speed
		}
	}

	private CharacterMovementValues RecomputeMovementValue() {
		Vector3 startPosition = this.agent.transform.position;
		Vector3 targetDirection = targetPosition - startPosition;

		CharacterMovementValues movement = new CharacterMovementValues();

		// Yaw
		float yawAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, Vector3.up);
		if (yawAngle < -yawAngleThreshold) movement.yawMotion = YawMotion.Right;
		else if (yawAngle > yawAngleThreshold) movement.yawMotion = YawMotion.Left;
		else movement.yawMotion = YawMotion.None;

		// Pitch
		float pitchAngle = Vector3.SignedAngle(targetDirection, this.agent.transform.forward, this.agent.transform.right);
		if (pitchAngle < -pitchAngleThreshold) movement.pitchMotion = PitchMotion.Down;
		else if (pitchAngle > pitchAngleThreshold) movement.pitchMotion = PitchMotion.Up;
		else movement.pitchMotion = PitchMotion.None;

		// Forward
		float absYawAngle = Mathf.Abs(yawAngle);
		if (absYawAngle < goodAngleThreshold) movement.forwardMotion = ForwardMotion.Forward; // if the current direction is reasonable, go forward
		else if (absYawAngle > badAngleThreshold) movement.forwardMotion = ForwardMotion.Brake; // if it is entirely wrong, brake
		else movement.forwardMotion = ForwardMotion.None; // otherwise do nothing


		Debug.Log($"Current movement: ({movement.forwardMotion}, {movement.yawMotion}, {movement.pitchMotion})");

		return movement;
	}

	private void Update() {
		elapsedTime += Time.deltaTime;
	}

}
