using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPointsGenerationRandomWalk : LevelGeneratorModule {

	[Header("Generator parameters")]

	[Tooltip("How many points (hoops) should be generated.")]
	public int numberOfPoints = 10;
	[Tooltip("The approximate minimum and maximum distance between two points.")]
	public Vector2 distanceRange = new Vector2(4f, 6f);
	[Tooltip("Maximum angle between two consecutive points in the X (up/down) and Y (left/right) axis.")]
	public Vector2 maxDirectionChangeAngle = new Vector2(45f, 45f);


	LevelRepresentation levelDebug;

	// TODO: Snap the points to the underlying terrain grid
	// TODO: Set grid coordinates of the points
	// TODO: Move all the points afterwards so that the final track is centered
	// TODO: Make sure the points do not overlap
	public override void Generate(LevelRepresentation level) {
		levelDebug = level;

		TrackPoint point, nextPoint;
		float rotationAngle = maxDirectionChangeAngle.y;
		// Choose start point (in the middle)
		point = new TrackPoint();
		point.gridCoords = new Vector2Int(level.pointCount.x / 2, level.pointCount.y / 2);
		point.position = level.terrain[point.gridCoords.x, point.gridCoords.y].position;
		// Choose first direction vector (forward)
		Vector3 direction = Vector3.forward;
		// Choose distance (random from reasonable interval)
		float distance = Random.Range(distanceRange.x, distanceRange.y);

		// Repeat until all the points are generated
		for (int i = 0; i < numberOfPoints; i++) {
			// Add the point to a list
			level.track.Add(point);
			// Move to the next point (given by the previous point, direction vector and distance)
			nextPoint = new TrackPoint();
			nextPoint.position = point.position + direction * distance;
			point = nextPoint;
			// Choose next distance vector (rotate the previous vector by a reasonably small amount)
			float randomValue = Random.value;
			if (randomValue < 0.4f) // continue turning to the same side
				rotationAngle = Random.Range(Mathf.Min((rotationAngle / rotationAngle) * maxDirectionChangeAngle.y, 0), Mathf.Max((rotationAngle / rotationAngle) * maxDirectionChangeAngle.y, 0));
			else if (randomValue < 0.8f) { // prefer turning
				rotationAngle = Random.Range(maxDirectionChangeAngle.y / 2, maxDirectionChangeAngle.y);
				if (Random.value < 0.5f)
					rotationAngle *= -1;
			} else // completely random
				rotationAngle = Random.Range(-maxDirectionChangeAngle.y, maxDirectionChangeAngle.y);
			direction = Quaternion.Euler(0, rotationAngle, 0) * direction;
			// Choose next distance
			distance = Random.Range(distanceRange.x, distanceRange.y);
		}

	}

	private void OnDrawGizmosSelected() {
		// Go through all the points and draw a cube in each of them
		if (levelDebug != null) {
			Gizmos.color = Color.green;
			foreach (var point in levelDebug.track) {
				Gizmos.DrawCube(point.position, Vector3.one * 0.2f);
			}
		}
	}
}
