using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpellTrajectory : SpellTrajectoryComputer {

	[Tooltip("Maximum distance of the offset from the direct line.")]
	[SerializeField] protected float radius = 0.3f;
	[Tooltip("A random offset from the direct line will be generated at every step, inbetween will be linear interpolation.")]
	[SerializeField] protected float randomOffsetDistance = 0.4f;

	protected Vector2 previousOffset = Vector2.zero;
	protected Vector2 nextOffset = Vector2.zero;
	protected float lastOffsetDistance = 0;

	public override SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart) {
		if (distanceFromStart > lastOffsetDistance) { // new random offset needs to be calculated
			previousOffset = nextOffset;
			nextOffset = Random.insideUnitCircle * radius;
			lastOffsetDistance += randomOffsetDistance;
		}
		// Linear interpolation between offsets
		float t = (distanceFromStart % randomOffsetDistance) / randomOffsetDistance;
		return new SpellTrajectoryPoint {
			distanceFromStart = distanceFromStart,
			offset = previousOffset + (nextOffset - previousOffset) * t
		};
	}

	public override void ResetTrajectory() {
		previousOffset = Vector2.zero;
		nextOffset = Vector2.zero;
		lastOffsetDistance = 0;
	}
}
