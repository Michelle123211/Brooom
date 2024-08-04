using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearSpellTrajectory : SpellTrajectoryComputer {
	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		return new SpellTrajectoryPoint { distanceFromStart = distanceFromStart, offset = Vector2.zero };
	}

	public override void ResetTrajectory() {
	}
}
