using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Some track points may have been placed higher because of the terrain height underneath them (in the <c>TrackTerrainHeightPostprocessing</c> module).
/// This way the vertical angle between two adjacent points may be greater than the maximum angle allowed.
/// This level generator module goes through all the track points (as well as bonus spots) and changes their Y coordinate if necessary to decrease the angle,
/// so that the maximum angle is satisfied.
/// It starts in the point with the maximum height so that most of the points will be moved higher and so intersection with terrain should not appear that frequently.
/// </summary>
public class MaximumAngleCorrection : LevelGeneratorModule {

	[Tooltip("Maximum angle between two consecutive points in the X (up/down) axis.")]
	public float maxAngle = 10;

	/// <summary>
	/// Goes through all track points and bonus spots and changes their Y coordinate so that the maximum angle between two consecutive points is satisfied.
	/// </summary>
	/// <param name="level"><inheritdoc/></param>
	public override void Generate(LevelRepresentation level) {
		// Find the highest point among the track points and bonus spots
		// Set a bool variable depending on whether it is the track point or bonus spot
		float maxHeight = float.MinValue;
		int maxIndex = -1; // index of the point/spot with maximum height
		bool startInHoop = true; // true means there is a hoop in the maximum height, false means there is a bonus spot
		// Go through all the track points
		for (int i = 0; i < level.Track.Count; i++) {
			if (level.Track[i].position.y > maxHeight) {
				maxHeight = level.Track[i].position.y;
				maxIndex = i;
			}
		}
		// Go through all the bonus spots
		for (int i = 0; i < level.bonuses.Count; i++) {
			if (level.bonuses[i].position.y > maxHeight) {
				maxHeight = level.bonuses[i].position.y;
				maxIndex = i;
				startInHoop = false;
			}
		}

		// Correct height to the left (start)
		CorrectOneSide(level, maxIndex, startInHoop, -1);
		// Correct height to the right (end)
		CorrectOneSide(level, maxIndex, startInHoop, 1);

		// Update height of starting position
		level.playerStartPosition.y = Mathf.Max(level.playerStartPosition.y, level.Track[0].position.y);
	}

	// Goes from the given point in the given direction and corrects Y coordinates so that maximum angle is satisfied everywhere
	//	- index - index of the track point or bonus spot where to start
	//	- startInHoop - true if the index corresponds to a hoop, false if it corresponds to a bonus spot
	//	- direction - -1 to go left, 1 to go right
	private void CorrectOneSide(LevelRepresentation level, int startIndex, bool startInHoop, int direction) {
		// Initialize last position
		Vector3 lastPosition;
		if (startInHoop) lastPosition = level.Track[startIndex].position;
		else lastPosition = level.bonuses[startIndex].position;

		// Starting point/spot is not changed
		// Set index of the following point/spot in the given direction
		(int startHoopIndex, int startBonusIndex) = GetStartingIndices(level, startIndex, direction, ref startInHoop);

		// Correct height of the hoop if necessary
		if (startInHoop) {
			CorrectHoopHeight(level, startHoopIndex, lastPosition);
			lastPosition = level.Track[startHoopIndex].position;
			startHoopIndex += direction;
		}

		// While there are other hoops in the direction
		while (startHoopIndex >= 0 && startHoopIndex < level.Track.Count) {
			// Corect heights of all the bonuses until the next hoop
			startBonusIndex = CorrectBonusHeightUntilNextHoop(level, startBonusIndex, lastPosition, direction);
			lastPosition = level.bonuses[startBonusIndex - direction].position;
			// Correct height of the next hoop
			CorrectHoopHeight(level, startHoopIndex, lastPosition);
			lastPosition = level.Track[startHoopIndex].position;
			startHoopIndex += direction;
		}
	}

	// Returns starting hoop index and starting bonus index
	private (int startHoopIndex, int startBonusIndex) GetStartingIndices(LevelRepresentation level, int index, int direction, ref bool startInHoop) {
		// Starting point/spot is not changed
		// Set index of the following point/spot in the given direction
		int startHoopIndex = index;
		int startBonusIndex = index;
		if (startInHoop) { // the starting point is a hoop so the adjacent one must be bonus
			for (int i = 0; i < level.bonuses.Count; i++) {
				if (direction < 0) { // find the first bonus spot to the left
					if (level.bonuses[i].previousHoopIndex < startHoopIndex) startBonusIndex = i;
					else break;
				} else { // find the first bonus spot to the right
					if (level.bonuses[i].previousHoopIndex < startHoopIndex) continue;
					else {
						startBonusIndex = i;
						break;
					}
				}
			}
			startHoopIndex += direction;
			startInHoop = false;
		} else { // the starting point is a bonus so the adjacent one may be either another bonus or a hoop
			if (direction < 0) { // going to the left
				startHoopIndex = level.bonuses[startBonusIndex].previousHoopIndex; // take the first hoop to the left
				if (startBonusIndex > 0 && level.bonuses[startBonusIndex - 1].previousHoopIndex == level.bonuses[startBonusIndex].previousHoopIndex) // there is another bonus before the hoop
					startBonusIndex--;
				else startInHoop = true;
			} else { // going to the right
				startHoopIndex = level.bonuses[startBonusIndex].previousHoopIndex + 1;  // take the first hoop to the right
				if (startBonusIndex < level.bonuses.Count - 1 && level.bonuses[startBonusIndex + 1].previousHoopIndex == level.bonuses[startBonusIndex].previousHoopIndex) // there is another bonus before the hoop
					startBonusIndex++;
				else startInHoop = true;
			}
		}

		return (startHoopIndex, startBonusIndex);
	}

	// Corrects height of the hoop with the given index to satisfy maximum angle between the hoop and the previous point
	//	- hoopIndex - index of the hoop
	//	- lastPosition - position of the previous point
	//	- direction ... -1 to go left, 1 to go right
	private void CorrectHoopHeight(LevelRepresentation level, int hoopIndex, Vector3 lastPosition) {
		level.Track[hoopIndex].position.y = GetPointHeightWithLimitedAngle(level.Track[hoopIndex].position, lastPosition);
	}

	// Corrects heights of all bonuses in the given direction until the next hoop, and returns index of the next bonus (which was not processed because it is after the hoop)
	//	- startIndex - index of the bonus where to start
	//	- lastPosition - position of the previous point
	//	- direction -1 to go left, 1 to go right
	private int CorrectBonusHeightUntilNextHoop(LevelRepresentation level, int startIndex, Vector3 lastPosition, int direction) {
		int previousHoopIndex = level.bonuses[startIndex].previousHoopIndex;
		while (startIndex >= 0 && startIndex < level.bonuses.Count && level.bonuses[startIndex].previousHoopIndex == previousHoopIndex) {
			level.bonuses[startIndex].position.y = GetPointHeightWithLimitedAngle(level.bonuses[startIndex].position, lastPosition);
			lastPosition = level.bonuses[startIndex].position;
			startIndex += direction;
		}
		return startIndex;
	}

	// Returns new height of the pointToCorrect so that the vertical angle between pointToCorrect and otherPoint is at most maxAngle
	private float GetPointHeightWithLimitedAngle(Vector3 pointToCorrect, Vector3 otherPoint) {
		// Compute distance between the points (in the XZ plane)
		float distance = Vector3.Distance(pointToCorrect.WithY(0), otherPoint.WithY(0));
		// Compute maximum height difference from the angle and the distance
		float maximumDifference = distance * Mathf.Tan(Mathf.Deg2Rad * maxAngle);

		// If the current height is within the interval, return it
		float difference = pointToCorrect.y - otherPoint.y;
		if (Mathf.Abs(difference) < maximumDifference)
			return pointToCorrect.y;
		// Otherwise return height at the end point of the interval
		else
			return otherPoint.y + Mathf.Sign(difference) * maximumDifference;
	}
}
