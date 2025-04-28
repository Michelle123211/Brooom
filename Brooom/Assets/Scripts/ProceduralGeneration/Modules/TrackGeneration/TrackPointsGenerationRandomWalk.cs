using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for generating a list of track points (i.e. points in which hoops or checkpoints will be placed) using a random walk.
/// It also prevents the track from intersecting itself by limiting accumulated turn angle.
/// </summary>
public class TrackPointsGenerationRandomWalk : TrackGenerationBase {

	private float globalAngle = 0; // accumulating overall angle while adding new points, to make sure we never turn for more than 90° and intersection is not possible
	private float lastAngle = 0; // last generated angle around Y axis

	LevelRepresentation levelDebug; // level representation stored so it is possible to draw gizmos in track points (for debug purposes)

	
	/// <summary>
	/// Generates a list of track points using a random walk while limiting turning angle to prevent the track from intersecting itself.
	/// Then sets these track points in the level representation.
	/// </summary>
	/// <param name="level"><inheritdoc/></param>
	protected override void GenerateTrackPoints(LevelRepresentation level) {
		levelDebug = level;

		// Initialize variables
		globalAngle = 0;
		lastAngle = 0;

		// Compute total number of track points - after each checkpoint a specific number of hoops follows (except the last checkpoint)
		int numberOfPoints = numberOfCheckpoints + numberOfHoopsBetween * (numberOfCheckpoints - 1);

		Vector3 position;
		// Add start point
		position = Vector3.zero;
		AddTrackPoint(level, position);
		// Choose first direction vector (forward)
		Vector3 direction = Vector3.forward;
		// Choose distance (random from reasonable interval)
		float distance = Random.Range(distanceRange.x, distanceRange.y);

		// Repeat until all the points are generated
		while (level.Track.Count < numberOfPoints) {
			// Move to the next point (given by the previous point, direction vector and distance)
			position = position + direction * distance;
			AddTrackPoint(level, position);
			// Choose next direction vector and distance
			SelectParametersForNextStep(level, position, ref direction, ref distance);
		}
	}

	// Sets new values for direction (according to the new last segment) and distance
	private void SelectParametersForNextStep(LevelRepresentation level, Vector3 position, ref Vector3 direction, ref float distance) {
		direction = level.Track.Count < 2 ? Vector3.forward : (level.Track[level.Track.Count - 1].position - level.Track[level.Track.Count - 2].position);
		direction = SelectDirection(level, position, direction);
		distance = Random.Range(distanceRange.x, distanceRange.y);
	}

	// Chooses next direction vector (by rotating the previous vector by a reasonably small amount)
	private Vector3 SelectDirection(LevelRepresentation level, Vector3 position, Vector3 previousDirection) {
		Vector3 direction = Quaternion.Euler(0, SelectRotationAngleY(), 0) * previousDirection; // left/right, rotation around the Y axis
		float newY = (Quaternion.Euler(SelectRotationAngleX(level, position, direction), 0, 0) * Vector3.forward).y; // rotate forward vector around X axis to get the Y coordinate
		direction = direction.normalized.WithY(newY).normalized; // override Y make the rotation absolute not relative
		return direction;
	}

	// Selects angle up/down
	private float SelectRotationAngleX(LevelRepresentation level, Vector3 lastPosition, Vector3 lastDirection) {
		float angle;
		if (lastPosition.y < 1) // only up possible
			angle = Random.Range(-maxDirectionChangeAngle.x, 0);
		else if (lastPosition.y > maxAltitude - 1) // only down possible
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

	// Selects angle left/right
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

	// Chooses a random angle to the same side as was the last angle
	private float SelectAngleInTheSameDirection(Vector2 angleInterval) {
		float direction = Mathf.Sign(lastAngle);
		if (direction < 0) return Random.Range(angleInterval.x, 0);
		else return Random.Range(0, angleInterval.y);
	}

	// Chooses a random angle which is closer to the maximum possible angle to allow for bigger turns
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

	// Chooses a random angle from the given interval
	private float SelectAngleRandomly(Vector2 angleInterval) {
		return Random.Range(angleInterval.x, angleInterval.y);
	}

	// Gets allowed range of angles for turning left/right, while considering maximum angle possible and limiting angles to prevent the track from intersecting itself
	private Vector2 GetMinAndMaxAngleY() {
		// Select minimum and maximum angle of rotation around Y (left/right) so that no intersections can happen (global angle cannot go over 90 in any direction)
		Vector2 minMaxAngle = new Vector2(-maxDirectionChangeAngle.y, maxDirectionChangeAngle.y);
		if (globalAngle < 0) { // there is more space to turn right (positive angle)
			minMaxAngle.x = Mathf.Max(-90 - globalAngle, -maxDirectionChangeAngle.y);
		} else { // there is more space to turn left (negative angle)
			minMaxAngle.y = Mathf.Min(90 - globalAngle, maxDirectionChangeAngle.y);
		}
		return minMaxAngle;
	}

	private void OnDrawGizmosSelected() {
		// Go through all the points and draw a cube in each of them
		if (levelDebug != null) {
			Gizmos.color = Color.yellow;
			foreach (var point in levelDebug.Track) {
				Gizmos.DrawCube(point.position, Vector3.one * 0.5f);
			}
		}
	}
}
