using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates points the track goes through (using random walk)
// Prevents the track from intersecting itself
public class TrackPointsGenerationRandomWalk : LevelGeneratorModule {
	[Tooltip("How many checkpoints should be generated.")]
	public int numberOfCheckpoints = 4;
	[Tooltip("How many hoops should be generated between two consecutive checkpoints.")]
	public int numberOfHoopsBetween = 3;
	[Tooltip("The approximate minimum and maximum distance between two points.")]
	public Vector2 distanceRange = new Vector2(4f, 6f);
	[Tooltip("Maximum angle between two consecutive points in the X (up/down) and Y (left/right) axis.")]
	public Vector2 maxDirectionChangeAngle = new Vector2(45f, 45f);
	[Tooltip("Dimensions of the level are adjusted according to the generated track dimensions with some padding around.")]
	public int levelPadding = 50;
	[Tooltip("How many track points are removed during backtracking when the last generated segment intersects with the already generated track.")]
	public int backtrackDepth = 3;


	// Minimum and maximum position of the track points (computed while centering the track around world origin, used for changing level dimensions)
	Vector2 minPosition;
	Vector2 maxPosition;

	LevelRepresentation levelDebug;

	public override void Generate(LevelRepresentation level) {
		levelDebug = level;

		GenerateTrackPoints(level);
		CenterTrackPoints(level);

		// Update level dimensions so that the track is not out of bounds
		level.ChangeDimensions(new Vector2(2 * (maxPosition.x + levelPadding), 2 * (maxPosition.y + levelPadding))); // added some padding around

		SnapPointsToGrid(level);

		// Select player's start position in front of the first point
		level.playerStartPosition = level.track[0].position + Vector3.back * distanceRange.y;
	}

	private void GenerateTrackPoints(LevelRepresentation level) {
		// Compute total number of track points - after each checkpoint a specific number of hoops follows (except the last checkpoint)
		int numberOfPoints = numberOfCheckpoints + numberOfHoopsBetween * (numberOfCheckpoints - 1);

		TrackPoint point, nextPoint;
		// Choose start point (in the middle)
		point = new TrackPoint();
		point.position = Vector3.zero;
		point.isCheckpoint = true;
		// Add the point to a list
		level.track.Add(point);
		// Choose first direction vector (forward)
		Vector3 direction = Vector3.forward;
		// Choose distance (random from reasonable interval)
		float distance = Random.Range(distanceRange.x, distanceRange.y);

		// Repeat until all the points are generated
		while (level.track.Count < numberOfPoints) {
			// Check that the last generated point is suficiently far from all the other ones (if not, generate a new one instead)
			if (IsLastPointCloseToAnotherOne(level)) {
				// Remove last two points
				level.track.RemoveRange(level.track.Count - 2, 2);
				// Choose next direction vector and distance
				SelectParametersForNextStep(level, ref direction, ref distance, ref point);
			}
			// Check that the last generated segment is not intersecting with any previous one (if it is, then backtrack a few steps and try again)
			else if (IsLastSegmentIntersectingWithAnyPrevious(level)) {
				// Remove last few points
				int removeFromIndex = Mathf.Max(level.track.Count - backtrackDepth - 1, 1);
				int countToRemove = Mathf.Min(backtrackDepth, level.track.Count - 1);
				level.track.RemoveRange(removeFromIndex, countToRemove);
				// Choose next direction vector and distance
				SelectParametersForNextStep(level, ref direction, ref distance, ref point);
			}
			// Otherwise everything is all right and a new point may be generated
			else {
				// Move to the next point (given by the previous point, direction vector and distance)
				nextPoint = new TrackPoint();
				nextPoint.position = point.position + direction * distance;
				point = nextPoint;
				// Add the point to a list
				level.track.Add(point);
				// Mark checkpoints
				if ((level.track.Count - 1) % (numberOfHoopsBetween + 1) == 0) point.isCheckpoint = true;
				// Choose next direction vector and distance
				SelectParametersForNextStep(level, ref direction, ref distance, ref point);
			}
		}
	}

	private void SelectParametersForNextStep(LevelRepresentation level, ref Vector3 direction, ref float distance, ref TrackPoint point) {
		// Set new values for direction (according to the new last segment) and distance
		direction = level.track.Count < 2 ? Vector3.forward : (level.track[level.track.Count - 1].position - level.track[level.track.Count - 2].position);
		direction = SelectDirection(level, point, direction);
		distance = Random.Range(distanceRange.x, distanceRange.y);
		// Select the last point which will be used in the next iteration
		point = level.track[level.track.Count - 1];
	}

	private Vector3 SelectDirection(LevelRepresentation level, TrackPoint point, Vector3 previousDirection) {
		// Choose next direction vector (rotate the previous vector by a reasonably small amount)
		Vector3 direction = Quaternion.Euler(0, SelectRotationAngleY(), 0) * previousDirection; // left/right, rotation around the Y axis
		float newY = (Quaternion.Euler(SelectRotationAngleX(level, point, direction), 0, 0) * Vector3.forward).y; // rotate forward vector around X axis to get the Y coordinate
		direction = direction.WithY(newY).normalized; // override Y make the rotation absolute not relative
		return direction;
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
		else if (lastPoint.position.y > PlayerState.Instance.maxAltitude - 1) // only down possible
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

	private bool IsLastPointCloseToAnotherOne(LevelRepresentation level) {
		// Check if the last track point is closer to another one than the minimum distance between two points
		if (level.track.Count < 3) return false;
		for (int i = 0; i < level.track.Count - 1; i++) {
			if (Vector3.Distance(level.track[i].position.WithY(0), level.track[level.track.Count - 1].position.WithY(0)) < distanceRange.x) {
				//Debug.Log($"Last point {level.track[level.track.Count - 1].position.WithY(0)} is too close to {level.track[i].position.WithY(0)}");
				return true;
			}
		}
		return false;
	}

	private bool IsLastSegmentIntersectingWithAnyPrevious(LevelRepresentation level) {
		if (level.track.Count < 3) return false; // there must be at least two segments
		int lastSegmentStart = level.track.Count - 2;
		for (int firstSegmentStart = 0; firstSegmentStart < lastSegmentStart; firstSegmentStart++) {
			if (AreTwoSegmentsIntersecting(level.track[firstSegmentStart].position, level.track[firstSegmentStart + 1].position,
										   level.track[lastSegmentStart].position, level.track[lastSegmentStart + 1].position)) {
				//Debug.Log($"Intersection found between [{level.track[firstSegmentStart].position}, {level.track[firstSegmentStart + 1].position}] and [{level.track[lastSegmentStart].position}, {level.track[lastSegmentStart + 1].position}].");
				return true;
			}
		}
		return false;
	}

	// Determines if two segments given as a + t * (b - a) and c + u * (d - c) are intersecting when projected to the XZ plane (Y = 0)
	private bool AreTwoSegmentsIntersecting(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
		// Equations taken from: https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
		float t = ((a.x - c.x) * (c.z - d.z) - (a.z - c.z) * (c.x - d.x)) /
				  ((a.x - b.x) * (c.z - d.z) - (a.z - b.z) * (c.x - d.x));
		float u = ((a.x - c.x) * (a.z - b.z) - (a.z - c.z) * (a.x - b.x)) /
				  ((a.x - b.x) * (c.z - d.z) - (a.z - b.z) * (c.x - d.x));
		return t >= 0 && t < 1 && u > 0 && u <= 1; // end point is shared, so t < 1 (not t <= 1) and u > 0 (not u >= 0)
	}

	// Moves all the points so that the final track is centered around the world origin
	private void CenterTrackPoints(LevelRepresentation level) {
		// Compute min and max positions
		minPosition = new Vector2(float.MaxValue, float.MaxValue);
		maxPosition = new Vector2(float.MinValue, float.MinValue);
		foreach (var trackPoint in level.track) {
			if (trackPoint.position.x < minPosition.x) minPosition.x = trackPoint.position.x;
			if (trackPoint.position.x > maxPosition.x) maxPosition.x = trackPoint.position.x;
			if (trackPoint.position.z < minPosition.y) minPosition.y = trackPoint.position.z;
			if (trackPoint.position.z > maxPosition.y) maxPosition.y = trackPoint.position.z;
		}
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
