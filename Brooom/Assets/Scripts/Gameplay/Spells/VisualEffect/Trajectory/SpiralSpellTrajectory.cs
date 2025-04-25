using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class computing points on a spell's trajectory towards the target in a shape of spiral
/// (i.e. offseting points from direct line in a circle around it).
/// </summary>
public class SpiralSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Radius of the offset from the direct line.")]
	[SerializeField] protected float radius = 0.3f;
	[Tooltip("How fast the point travels on a circle.")]
	[SerializeField] protected float frequency = 2f;

	/// <inheritdoc/>
	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		// Get point on circle
		float angle = distanceFromStart * frequency;
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius
		};
	}

	/// <inheritdoc/>
	public override void ResetTrajectory() {
	}
}
