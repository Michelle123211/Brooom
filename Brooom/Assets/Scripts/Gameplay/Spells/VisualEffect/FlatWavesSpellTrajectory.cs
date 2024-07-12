using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatWavesSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Maximum offset from the direct line.")]
	[SerializeField] protected float radius = 0.5f;
	[Tooltip("How fast the point travels from side to side.")]
	[SerializeField] protected float frequency = 2f;

	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = new Vector2(Mathf.Sin(distanceFromStart * frequency), 0) * radius
		};
	}

	public override void ResetTrajectory() {
	}
}
