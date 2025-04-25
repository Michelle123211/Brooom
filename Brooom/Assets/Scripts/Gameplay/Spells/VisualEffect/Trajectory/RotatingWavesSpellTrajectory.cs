using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class computing points on a spell's trajectory towards the target in a shape of rotating waves
/// (i.e. repeatedly offseting points from direct line to the left and to the right while also rotating it on a circle).
/// </summary>
public class RotatingWavesSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Maximum offset from the direct line.")]
	[SerializeField] protected float radius = 0.5f;
	[Tooltip("How fast the point travels from side to side.")]
	[SerializeField] protected float frequency = 2f;
	[Tooltip("How fast the point rotates on a circle.")]
	[SerializeField] protected float rotationFrequency = 0.2f;

	/// <inheritdoc/>
	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		float intensity = Mathf.Sin(distanceFromStart * frequency); // movement from side to side
		float angle = distanceFromStart * rotationFrequency; // rotation on a circle
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * intensity * radius
		};
	}

	/// <inheritdoc/>
	public override void ResetTrajectory() {
	}

}
