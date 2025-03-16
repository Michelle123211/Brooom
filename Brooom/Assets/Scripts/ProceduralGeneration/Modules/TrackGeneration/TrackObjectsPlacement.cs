using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackObjectsPlacement : LevelGeneratorModule {
	[Tooltip("Prefab of the hoop.")]
	public Hoop hoopPrefab;
	[Tooltip("Scale of the hoop.")]
	public float hoopScale = 1f;
	[Tooltip("Prefab of the checkpoint hoop.")]
	public Hoop checkpointPrefab;
	[Tooltip("An object which will be parent of all the hoops objects in the hierarchy.")]
	public Transform hoopsParent;
	[Tooltip("Prefab of the starting zone.")]
	public GameObject startingZonePrefab;
	[Tooltip("How far in front of the player's start position is the starting zone placed.")]
	public float startingZoneOffset = 60f;
	[Tooltip("Prefab of the finish line.")]
	public FinishLine finishLinePrefab;
	[Tooltip("How far behind the last hoop is the finish line placed.")]
	public float finishLineOffset = 50f;

	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated hoops
		UtilsMonoBehaviour.RemoveAllChildren(hoopsParent);
		// Instantiate hoops/checkpoints in track points
		TrackPoint point;
		Vector3 previousPosition, nextPosition, direction = Vector3.forward;
		for (int i = 0; i < level.track.Count; i++) {
			point = level.track[i];
			Hoop prefab = hoopPrefab;
			if (point.isCheckpoint)
				prefab = checkpointPrefab;
			// Orientation is given by the vector from the previous hoop to the next hoop
			previousPosition = point.position;
			nextPosition = point.position;
			if (i > 0)
				previousPosition = level.track[i - 1].position;
			if (i < level.track.Count - 1)
				nextPosition = level.track[i + 1].position;
			direction = (nextPosition - previousPosition).WithY(0); // Y = 0 to rotate only around the Y axis
			// Create instance
			point.assignedHoop = Instantiate<Hoop>(prefab, level.track[i].position, Quaternion.FromToRotation(Vector3.forward, direction), hoopsParent);
			// Set scale of the hoops
			if (!point.isCheckpoint)
				point.assignedHoop.GetComponent<Scalable>()?.SetScale(Vector3.one * hoopScale);
		}
		// Instantiate starting zone
		Vector3 startingZonePosition = (level.playerStartPosition + Vector3.back * startingZoneOffset).WithY(0);
		Instantiate(startingZonePrefab, startingZonePosition, Quaternion.identity);
		// Instantiate finish line
		// ... orientation is the same as for the last hoop
		Vector3 finishLinePosition = (level.track[level.track.Count - 1].position + direction.normalized * finishLineOffset).WithY(0);
		FinishLine finish = Instantiate<FinishLine>(finishLinePrefab, finishLinePosition, Quaternion.FromToRotation(Vector3.forward, direction), transform);
		level.finish = finish;
	}
}
