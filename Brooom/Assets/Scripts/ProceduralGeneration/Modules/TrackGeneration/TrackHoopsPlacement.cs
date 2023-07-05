using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackHoopsPlacement : LevelGeneratorModule {
	[Tooltip("Prefab of the hoop.")]
	public GameObject hoopPrefab;
	[Tooltip("Scale of the hoop.")]
	public float hoopScale = 1f;
	[Tooltip("Prefab of the checkpoint hoop.")]
	public GameObject checkpointPrefab;
	[Tooltip("An object which will be parent of all the hoops objects in the hierarchy.")]
	public Transform hoopsParent;

	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated hoops
		for (int i = hoopsParent.childCount - 1; i >= 0; i--) {
			Destroy(hoopsParent.GetChild(i).gameObject);
		}
		// Instantiate hoops/checkpoints in track points
		TrackPoint point;
		for (int i = 0; i < level.track.Count; i++) {
			point = level.track[i];
			GameObject prefab = hoopPrefab;
			if (point.isCheckpoint)
				prefab = checkpointPrefab;
			// Orientation is given by the vector from the previous hoop to the next hoop
			Vector3 previousPosition = point.position, nextPosition = point.position;
			if (i > 0)
				previousPosition = level.track[i - 1].position;
			if (i < level.track.Count - 1)
				nextPosition = level.track[i + 1].position;
			Vector3 direction = (nextPosition - previousPosition).WithY(0); // Y = 0 to rotate only around the Y axis
			// Create instance
			point.assignedObject = Instantiate(prefab, level.track[i].position, Quaternion.FromToRotation(Vector3.forward, direction), hoopsParent);
			// Set scale of the hoops
			if (!point.isCheckpoint)
				point.assignedObject.GetComponent<Hoop>()?.SetScale(Vector3.one * hoopScale);
		}
	}
}
