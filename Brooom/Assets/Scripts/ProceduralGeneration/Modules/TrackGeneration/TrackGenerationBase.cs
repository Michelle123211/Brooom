using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrackGenerationBase : LevelGeneratorModule {

	[Tooltip("How many checkpoints should be generated.")]
	public int numberOfCheckpoints = 4;
	[Tooltip("How many hoops should be generated between two consecutive checkpoints.")]
	public int numberOfHoopsBetween = 3;
	[Tooltip("The approximate minimum and maximum distance between two points.")]
	public Vector2 distanceRange = new Vector2(40f, 50f);
	[Tooltip("Maximum angle between two consecutive points in the X (up/down) and Y (left/right) axis.")]
	public Vector2 maxDirectionChangeAngle = new Vector2(10f, 20f);
	[Tooltip("Dimensions of the level are adjusted according to the generated track dimensions with some padding around.")]
	public int levelPadding = 200;
	[Tooltip("How far in front of the first point the player should be placed.")]
	public float playerStartDistance = 60f;

	// Minimum and maximum position of the track points (computed while centering the track around world origin, used for changing level dimensions)
	private Vector2 minPosition;
	private Vector2 maxPosition;

	protected abstract void GenerateTrackPoints(LevelRepresentation level);

	public override void Generate(LevelRepresentation level) {
		GenerateTrackPoints(level);
		CenterTrackPoints(level);

		// Update level dimensions so that the track is not out of bounds
		level.ChangeDimensions(new Vector2(2 * (maxPosition.x + levelPadding), 2 * (maxPosition.y + levelPadding))); // added some padding around

		SnapPointsToGrid(level);

		// Select player's start position in front of the first point
		level.playerStartPosition = level.track[0].position + Vector3.back * playerStartDistance;
	}

	// Adds track point with the given position to the level
	protected void AddTrackPoint(LevelRepresentation level, Vector3 position) {
		TrackPoint trackPoint = new TrackPoint();
		trackPoint.position = position;
		trackPoint.isCheckpoint = (level.track.Count) % (numberOfHoopsBetween + 1) == 0;
		level.track.Add(trackPoint);
		Debug.Log($"Y = {position.y}");
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

}
