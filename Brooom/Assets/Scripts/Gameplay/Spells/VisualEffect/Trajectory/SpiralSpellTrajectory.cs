using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Radius of the offset from the direct line.")]
	[SerializeField] protected float radius = 0.3f;
	[Tooltip("How fast the point travels on a circle.")]
	[SerializeField] protected float frequency = 2f;

	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		float angle = distanceFromStart * frequency;
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius
		};
	}

	public override void ResetTrajectory() {
	}
}
