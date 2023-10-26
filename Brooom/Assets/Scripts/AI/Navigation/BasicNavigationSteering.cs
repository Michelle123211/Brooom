using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNavigationSteering : NavigationSteering {

	[Header("Angle thresholds")]
	[Tooltip("The current yaw direction is changed only if it is at least this angle from the direction to the target.")]
	public float yawAngleThreshold = 15f;
	[Tooltip("The current pitch direction is changed only if it is at least this angle from the direction to the target.")]
	public float pitchAngleThreshold = 5f;

	[Tooltip("If the angle between current direction and the direction to the target is greater than this angle, it is considered better to brake.")]
	public float brakeAngleThreshold = 90f;

	[Header("Input curves")]
	[Tooltip("Yaw input value depending on the angle.")]
	public AnimationCurve yawInputCurve;
	[Tooltip("Pitch input value depending on the angle.")]
	public AnimationCurve pitchInputCurve;
	[Tooltip("Forward input value depending on the angle.")]
	public AnimationCurve forwardInputCurveAngle;
	[Tooltip("Forward input value depending on distance to the target.")]
	public AnimationCurve forwardInputCurveDistance;

	protected override CharacterMovementValues GetMovementToTargetPosition() {
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

}
