using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentSpellTrajectoryComputer : SpellTrajectoryComputer {

	public enum SpellTrajectoryType { 
		Linear,
		Spiral,
		Random,
		PulsingSpiral,
		FlatWaves,
		RotatingWaves
	}

	[SerializeField] SpellTrajectoryType trajectoryType;

	private float radius = 0.4f;

	// Helper variables for random trajectory
	private Vector2 previousOffset = Vector2.zero;
	private Vector2 nextOffset = Vector2.zero;
	private float lastDistanceOffset = 0;
	private float offsetKeyframeDistance = 0.4f; // new random offset every 0.4 units, linear interpolation inbetween

	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		SpellTrajectoryPoint point = new SpellTrajectoryPoint { distanceFromStart = distanceFromStart };
		switch (trajectoryType) {
			case SpellTrajectoryType.Linear:
				point.offset = Vector2.zero;
				break;
			case SpellTrajectoryType.Spiral:
				float t = distanceFromStart - Mathf.Floor(distanceFromStart); // between 0 and 1
				float angle = t * 2 * Mathf.PI; // between 0 and 2 PI
				point.offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
				break;
			case SpellTrajectoryType.Random:
				if (distanceFromStart > lastDistanceOffset) { // new offset needs to be calculated
					previousOffset = nextOffset;
					nextOffset = Random.insideUnitCircle.normalized * 0.4f;
					lastDistanceOffset += offsetKeyframeDistance;
				}
				float t2 = (distanceFromStart - (lastDistanceOffset - offsetKeyframeDistance)) / offsetKeyframeDistance;
				point.offset = (previousOffset + (nextOffset - previousOffset) * t2) * radius;
				break;
			case SpellTrajectoryType.PulsingSpiral:
				float t3 = distanceFromStart - Mathf.Floor(distanceFromStart); // between 0 and 1
				float angle3 = t3 * 2 * Mathf.PI; // between 0 and 2 PI
				float r3 = (Mathf.Sin(distanceFromStart) + 1) / 4 + 0.5f;
				point.offset = new Vector2(Mathf.Cos(angle3), Mathf.Sin(angle3)) * r3 * radius;
				break;
			case SpellTrajectoryType.FlatWaves:
				point.offset = new Vector2(Mathf.Sin(distanceFromStart), 0) * radius;
				break;
			case SpellTrajectoryType.RotatingWaves:
				// Can it be done the same way as the pulsing spiral, just with different parameter values?
				// One parameter to rotate - just use the distanceFromStart directly as an angle
				// One parameter to pulse
				float intensity = Mathf.Sin(distanceFromStart * 5f);
				// Then combine
				point.offset = new Vector2(Mathf.Cos(distanceFromStart / 4), Mathf.Sin(distanceFromStart / 4)) * intensity * radius * 0.25f;
				break;
		}
		return point;
	}

	public override void ResetTrajectory() {
		previousOffset = Vector2.zero;
		nextOffset = Vector2.zero;
		lastDistanceOffset = 0;
	}
}
