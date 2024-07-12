using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingWavesSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Maximum offset from the direct line.")]
	[SerializeField] protected float radius = 0.5f;
	[Tooltip("How fast the point travels from side to side.")]
	[SerializeField] protected float frequency = 2f;
	[Tooltip("How fast the point rotates on a circle.")]
	[SerializeField] protected float rotationFrequency = 0.2f;

	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		float intensity = Mathf.Sin(distanceFromStart * frequency); // movement from side to side
		float angle = distanceFromStart * rotationFrequency; // rotation on a circle
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * intensity * radius
		};
	}

	public override void ResetTrajectory() {
	}
}
