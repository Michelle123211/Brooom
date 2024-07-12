using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsingSpiralSpellTrajectory : SpiralSpellTrajectory {

	[Tooltip("The minimum offset from the direct line.")]
	[SerializeField] protected float minRadius;
	[Tooltip("How fast the spiral decreases and increases.")]
	[SerializeField] protected float pulseFrequency = 1f;

	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		// Get basic spiral point
		SpellTrajectoryPoint point = base.GetNextTrajectoryPoint(distanceFromStart);
		// And scale down
		float scale = (Mathf.Sin(distanceFromStart * pulseFrequency) + 1f) / 2f; // between 0 and 1
		float radiusRatio = minRadius / radius;
		scale = scale * (1 - radiusRatio) + radiusRatio; // between (minRadius/radius) and 1
		point.offset = point.offset * scale;
		return point;
	}
}
