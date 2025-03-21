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
	public Vector2 distanceRange = new Vector2(40f, 50f);
	[Tooltip("Maximum angle between two consecutive points in the X (up/down) and Y (left/right) axis.")]
	public Vector2 maxDirectionChangeAngle = new Vector2(10f, 20f);
	[Tooltip("Dimensions of the level are adjusted according to the generated track dimensions with some padding around.")]
	public int levelPadding = 150;
	[Tooltip("How many track points are removed during backtracking when the last generated segment intersects with the already generated track.")]
	public int backtrackDepth = 3;
	[Tooltip("How far in front of the first point the player should be placed.")]
	public float playerStartDistance = 60f;


	private float globalAngle = 0; // accumulating overall angle while adding new points, to make sure we never turn for more than 180° and intersection is not possible
	private float lastAngle = 0; // last geenrated angle around Y axis

	// Minimum and maximum position of the track points (computed while centering the track around world origin, used for changing level dimensions)
	private Vector2 minPosition;
	private Vector2 maxPosition;

	LevelRepresentation levelDebug;

	public override void Generate(LevelRepresentation level) {
		levelDebug = level;

		GenerateTrackPoints(level);
		CenterTrackPoints(level);

		// Update level dimensions so that the track is not out of bounds
		level.ChangeDimensions(new Vector2(2 * (maxPosition.x + levelPadding), 2 * (maxPosition.y + levelPadding))); // added some padding around

		SnapPointsToGrid(level);

		// Select player's start position in front of the first point
		level.playerStartPosition = level.track[0].position + Vector3.back * playerStartDistance;
	}

	private void GenerateTrackPoints(LevelRepresentation level) {
		// Initialize variables
		globalAngle = 0;
		lastAngle = 0;

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

	// Select angle up/down
	private float SelectRotationAngleX(LevelRepresentation level, TrackPoint lastPoint, Vector2 lastDirection) {
		float angle;
		if (lastPoint.position.y < 1) // only up possible
			angle = Random.Range(-maxDirectionChangeAngle.x, 0);
		else if (lastPoint.position.y > PlayerState.Instance.maxAltitude - 1) // only down possible
			angle = Random.Range(0, maxDirectionChangeAngle.x);
		else {
			float randomValue = Random.value;
			if (randomValue < 0.4f) { // continue in the same direction
				float signedMax = Mathf.Sign(lastDirection.y) * -maxDirectionChangeAngle.x; // extreme selected based on up/down
				angle = Random.Range(Mathf.Min(signedMax, 0), Mathf.Max(signedMax, 0));
			} else { // completely random
				angle = Random.Range(-maxDirectionChangeAngle.x, maxDirectionChangeAngle.x);
			}
		}
		return angle;
	}

	// Select angle left/right
	private float SelectRotationAngleY() {
		float randomValue = Random.value;
		Vector2 angleInterval = GetMinAndMaxAngleY();
		if (randomValue < 0.4f) { // continue turning to the same side
			lastAngle = SelectAngleInTheSameDirection(angleInterval);
		} else if (randomValue < 0.8f) { // prefer bigger turn but in whichever direction
			lastAngle = SelectAngleForBiggerTurn(angleInterval);
		} else // completely random
			lastAngle = SelectAngleRandomly(angleInterval);
		// Choose an angle from the given interval, but don't turn too much to prevent track intersecting itself
		globalAngle += lastAngle;
		return lastAngle;
	}

	private float SelectAngleInTheSameDirection(Vector2 angleInterval) {
		// Choose random angle to the correct side
		float direction = Mathf.Sign(lastAngle);
		if (direction < 0) return Random.Range(angleInterval.x, 0);
		else return Random.Range(0, angleInterval.y);
	}

	private float SelectAngleForBiggerTurn(Vector2 angleInterval) {
		// First check to which side it is possible to turn - if both, choose by random
		bool isLeftPossible = angleInterval.x < (-maxDirectionChangeAngle.y / 2);
		bool isRightPossible = angleInterval.y > (maxDirectionChangeAngle.y / 2);
		int direction = 1;
		if (isLeftPossible && isRightPossible) direction = (Random.value < 0.5f ? -1 : 1);
		else if (isLeftPossible) direction = -1;
		// Then select angle randomly from that direction
		if (direction < 0) {
			return Random.Range(angleInterval.x, -maxDirectionChangeAngle.y / 2);
		} else {
			return Random.Range(maxDirectionChangeAngle.y / 2, angleInterval.y);
		}
	}

	private float SelectAngleRandomly(Vector2 angleInterval) {
		return Random.Range(angleInterval.x, angleInterval.y);
		//float angle = Random.Range(-maxDirectionChangeAngle.y, maxDirectionChangeAngle.y);
	}

	private Vector2 GetMinAndMaxAngleY() {
		// Select minimum and maximum angle of rotation around Y (left/right) so that no intersections can happen (global angle cannot go over 180 in any direction)
		Vector2 minMaxAngle = new Vector2(-maxDirectionChangeAngle.y, maxDirectionChangeAngle.y);
		if (globalAngle < 0) { // there is more space to turn right (positive angle)
			minMaxAngle.x = Mathf.Max(-90 - globalAngle, -maxDirectionChangeAngle.y);
		} else { // there is more space to turn left (negative angle)
			minMaxAngle.y = Mathf.Min(90 - globalAngle, maxDirectionChangeAngle.y);
		}
		return minMaxAngle;
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
