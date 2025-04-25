using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class computing points on spell's trajectory towards a target simply in direct line.
/// </summary>
public class LinearSpellTrajectory : SpellTrajectoryComputer {

	/// <inheritdoc/>
	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		return new SpellTrajectoryPoint { distanceFromStart = distanceFromStart, offset = Vector2.zero };
	}

	/// <inheritdoc/>
	public override void ResetTrajectory() {
	}

}
