using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class computing points on a spell's trajectory towards the target in a shape of flat waves
/// (i.e. repeatedly offseting points from direct line to the left and to the right).
/// </summary>
public class FlatWavesSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Maximum offset from the direct line.")]
	[SerializeField] protected float radius = 0.5f;
	[Tooltip("How fast the point travels from side to side.")]
	[SerializeField] protected float frequency = 2f;

	/// <inheritdoc/>
	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = new Vector2(Mathf.Sin(distanceFromStart * frequency), 0) * radius
		};
	}

	/// <inheritdoc/>
	public override void ResetTrajectory() {
	}

}
