using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackHoopsPlacement : LevelGeneratorModule {

	[Tooltip("Prefab of the hoop.")]
	public GameObject hoopPrefab;
	[Tooltip("An object which will be parent of all the hoops objects in the hierarchy.")]
	public Transform hoopsParent;

	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated hoops
		for (int i = hoopsParent.childCount - 1; i >= 0; i--) {
			Destroy(hoopsParent.GetChild(i).gameObject);
		}
		// Instantiate hoops in track points
		for (int i = 0; i < level.track.Count; i++) {
			GameObject hoop = Instantiate(hoopPrefab, level.track[i].position, Quaternion.identity, hoopsParent);
			// Orientation is given by the vector from the previous hoop to the next hoop
			Vector3 previousPosition = hoop.transform.position, nextPosition = hoop.transform.position;
			if (i > 0)
				previousPosition = level.track[i - 1].position;
			if (i < level.track.Count - 1)
				nextPosition = level.track[i + 1].position;
			float angle = Vector3.SignedAngle(Vector3.forward, nextPosition - previousPosition, Vector2.up);
			hoop.transform.Rotate(Vector3.up, angle); // rotate to the direction between the previous and the next hoops
		}
	}
}
