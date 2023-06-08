using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPointsGenerationRandomWalk : LevelGeneratorModule {

	[Tooltip("How many points (hoops) should be generated.")]
	public int numberOfPoints = 10;
	[Tooltip("The approximate minimum and maximum distance between two points.")]
	public Vector2 distanceRange = new Vector2(4f, 6f);
	[Tooltip("Maximum angle between two consecutive points in the X (up/down) and Y (left/right) axis.")]
	public Vector2 maxDirectionChangeAngle = new Vector2(45f, 45f);
	[Tooltip("Dimensions of the level are adjusted according to the generated track dimensions with some padding around.")]
	public int levelPadding = 50;


	LevelRepresentation levelDebug;

	// Minimum and maximum position of the track points (used later to center the track around world origin)
	Vector2 minPosition;
	Vector2 maxPosition;

	// TODO: Make sure the points do not overlap
	public override void Generate(LevelRepresentation level) {
		levelDebug = level;

		// Initialize variables
		minPosition = new Vector2(float.MaxValue, float.MaxValue);
		maxPosition = new Vector2(float.MinValue, float.MinValue);

		GenerateTrackPoints(level);
		CenterTrackPoints(level);

		// Update level dimensions so that the track is not out of bounds
		level.ChangeDimensions(new Vector2(2 * maxPosition.x + levelPadding, 2 * maxPosition.y + levelPadding)); // added some padding around

		SnapPointsToGrid(level);
	}

	private void GenerateTrackPoints(LevelRepresentation level) {
		TrackPoint point, nextPoint;
		float rotationAngle = maxDirectionChangeAngle.y;
		// Choose start point (in the middle)
		point = new TrackPoint();
		point.position = Vector3.zero;
		// Choose first direction vector (forward)
		Vector3 direction = Vector3.forward;
		// Choose distance (random from reasonable interval)
		float distance = Random.Range(distanceRange.x, distanceRange.y);

		// Repeat until all the points are generated
		for (int i = 0; i < numberOfPoints; i++) {
			// Add the point to a list
			level.track.Add(point);
			// Update minimum and maximum positions
			if (point.position.x < minPosition.x) minPosition.x = point.position.x;
			if (point.position.x > maxPosition.x) maxPosition.x = point.position.x;
			if (point.position.z < minPosition.y) minPosition.y = point.position.z;
			if (point.position.z > maxPosition.y) maxPosition.y = point.position.z;
			// Move to the next point (given by the previous point, direction vector and distance)
			nextPoint = new TrackPoint();
			nextPoint.position = point.position + direction * distance;
			point = nextPoint;
			// Choose next direction vector (rotate the previous vector by a reasonably small amount)
			direction = Quaternion.Euler(0, SelectRotationAngleY(), 0) * direction; // left/right, rotation around the Y axis
			float newY = (Quaternion.Euler(SelectRotationAngleX(level, point, direction), 0, 0) * Vector3.forward).y; // rotate forward vector around X axis to get the Y coordinate
			direction = direction.WithY(newY).normalized; // override Y make the rotation absolute not relative
			// Choose next distance
			distance = Random.Range(distanceRange.x, distanceRange.y);
		}
	}

	private float SelectRotationAngleY() {
		float angle = 0;
		float randomValue = Random.value;
		if (randomValue < 0.4f) { // continue turning to the same side
			float signedMax = (angle / Mathf.Abs(angle)) * maxDirectionChangeAngle.y; // extreme selected based on side
			angle = Random.Range(Mathf.Min(signedMax, 0), Mathf.Max(signedMax, 0));
		} else if (randomValue < 0.8f) { // prefer turning
			angle = Random.Range(maxDirectionChangeAngle.y / 2, maxDirectionChangeAngle.y);
			if (Random.value < 0.5f)
				angle *= -1;
		} else // completely random
			angle = Random.Range(-maxDirectionChangeAngle.y, maxDirectionChangeAngle.y);
		return angle;
	}

	private float SelectRotationAngleX(LevelRepresentation level, TrackPoint lastPoint, Vector2 lastDirection) {
		float angle;
		if (lastPoint.position.y < 1) // only up possible
			angle = Random.Range(-maxDirectionChangeAngle.x, 0);
		else if (lastPoint.position.y > level.heightRange.y - 1) // only down possible
			angle = Random.Range(0, maxDirectionChangeAngle.x);
		else {
			float randomValue = Random.value;
			if (randomValue < 0.4f) { // continue in the same direction
				float signedMax = lastDirection.y / Mathf.Abs(lastDirection.y) * -maxDirectionChangeAngle.x; // extreme selected based on up/down
				angle = Random.Range(Mathf.Min(signedMax, 0), Mathf.Max(signedMax, 0));
			} else { // completely random
				angle = Random.Range(-maxDirectionChangeAngle.x, maxDirectionChangeAngle.x);
			}
		}
		return angle;
	}

	// Moves all the points so that the final track is centered around the world origin
	private void CenterTrackPoints(LevelRepresentation level) {
		// Compute offsets in the X and Z axes
		float newMaxPositionX = Mathf.Abs(minPosition.x - maxPosition.x) / 2;
		float newMaxPositionZ = Mathf.Abs(minPosition.y - maxPosition.y) / 2;
		float offsetX = newMaxPositionX - maxPosition.x;
		float offsetZ = newMaxPositionZ - maxPosition.y;
		// Apply the offset to each point
		foreach (var point in level.track) {
			point.position.x += offsetX;
			point.position.z += offsetZ;
		}
		// Update the minimum and maximum positions
		minPosition.x = -newMaxPositionX;
		maxPosition.x = newMaxPositionX;
		minPosition.y = -newMaxPositionZ;
		maxPosition.y = newMaxPositionZ;
	}

	private void SnapPointsToGrid(LevelRepresentation level) {
		int i, j;
		Vector3 topleft = level.terrain[0, 0].position; // position of the top-left grid point
		float offset = level.pointOffset; // distance between adjacent grid points
		foreach (var trackPoint in level.track) {
			// Snap the points to the underlying terrain grid (to the closest grid point)
			i = Mathf.RoundToInt(Mathf.Abs(trackPoint.position.x - topleft.x) / offset);
			j = Mathf.RoundToInt(Mathf.Abs(trackPoint.position.z - topleft.z) / offset);
			trackPoint.position = level.terrain[i, j].position.WithY(trackPoint.position.y);
			// Set grid coordinates of the points
			trackPoint.gridCoords = new Vector2Int(i, j);
		}
	}

	private void OnDrawGizmosSelected() {
		// Go through all the points and draw a cube in each of them
		if (levelDebug != null) {
			Gizmos.color = Color.yellow;
			foreach (var point in levelDebug.track) {
				Gizmos.DrawCube(point.position, Vector3.one * 0.5f);
			}
		}
	}
}
