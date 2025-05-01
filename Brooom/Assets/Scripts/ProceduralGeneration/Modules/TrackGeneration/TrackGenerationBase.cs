using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base for a level generator module responsible for generating a list of track points (i.e. points in which hoops or checkpoints will be placed).
/// This class provides a common base like parameters and useful methods.
/// Different derived classes may adopt different approaches.
/// </summary>
public abstract class TrackGenerationBase : LevelGeneratorModule {

	[Tooltip("Maximum altitude (Y coordinate) the player can fly up to (no track element can be higher than that).")]
	public float maxAltitude = 15f;
	[Tooltip("How many checkpoints should be generated.")]
	public int numberOfCheckpoints = 4;
	[Tooltip("How many hoops should be generated between two consecutive checkpoints.")]
	public int numberOfHoopsBetween = 3;
	[Tooltip("The minimum and maximum distance between two points (final distance may be a bit smaller/larger because track points are snapped to terrain grid).")]
	public Vector2 distanceRange = new Vector2(40f, 50f);
	[Tooltip("Maximum angle between two consecutive points in the X (up/down) and Y (left/right) axes.")]
	public Vector2 maxDirectionChangeAngle = new Vector2(10f, 20f);
	[Tooltip("Dimensions of the level (i.e. terrain) are adjusted to cover the generated track dimensions with some padding around.")]
	public int levelPadding = 200;
	[Tooltip("How far in front of the first point the player should be placed.")]
	public float playerStartDistance = 60f;

	// Minimum and maximum position of the track points (computed while centering the track around world origin, used for changing level dimensions)
	private Vector2 minPosition;
	private Vector2 maxPosition;

	/// <summary>
	/// Generates a list of track points using some algorithm and sets it in the level representation.
	/// </summary>
	/// <param name="level">Initial level representation to be modified during the generation process.</param>
	protected abstract void GenerateTrackPoints(LevelRepresentation level);

	/// <summary>
	/// Generates a list of track points, moves them so that the track is centered around the world origin, and then changes level dimensions to cover the whole track with some padding.
	/// Also sets the player's start position to be in front of the first track point.
	/// </summary>
	/// <param name="level"><inheritdoc/></param>
	public override void Generate(LevelRepresentation level) {
		GenerateTrackPoints(level);
		CenterTrackPoints(level);

		// Update level dimensions so that the track is not out of bounds
		level.ChangeDimensions(new Vector2(2 * (maxPosition.x + levelPadding), 2 * (maxPosition.y + levelPadding))); // added some padding around

		SnapPointsToGrid(level);

		// Select player's start position in front of the first point
		level.playerStartPosition = level.Track[0].position + Vector3.back * playerStartDistance;
	}

	/// <summary>
	/// Creates a new <c>TrackPoint</c> with the given position and adds it to the level.
	/// </summary>
	/// <param name="level">Level representation to which a new track point will be added.</param>
	/// <param name="position">World position of the track point to be added.</param>
	protected void AddTrackPoint(LevelRepresentation level, Vector3 position) {
		TrackPoint trackPoint = new TrackPoint {
			position = position,
			isCheckpoint = (level.Track.Count) % (numberOfHoopsBetween + 1) == 0
		};
		level.Track.Add(trackPoint);
	}

	// Moves all track points so that the final track is centered around the world origin
	private void CenterTrackPoints(LevelRepresentation level) {
		// Compute min and max positions
		minPosition = new Vector2(float.MaxValue, float.MaxValue);
		maxPosition = new Vector2(float.MinValue, float.MinValue);
		foreach (var trackPoint in level.Track) {
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
		foreach (var point in level.Track) {
			point.position.x += offsetX;
			point.position.z += offsetZ;
		}
		// Update the minimum and maximum positions
		minPosition.x = -newMaxPositionX;
		maxPosition.x = newMaxPositionX;
		minPosition.y = -newMaxPositionZ;
		maxPosition.y = newMaxPositionZ;
	}

	// Goes through the track points and snaps each of them to the closest point on the underlying terrain grid
	private void SnapPointsToGrid(LevelRepresentation level) {
		int i, j;
		Vector3 topleft = level.Terrain[0, 0].position; // position of the top-left grid point
		float offset = level.pointOffset; // distance between adjacent grid points
		foreach (var trackPoint in level.Track) {
			// Snap the point to the underlying terrain grid (to the closest grid point)
			i = Mathf.RoundToInt(Mathf.Abs(trackPoint.position.x - topleft.x) / offset);
			j = Mathf.RoundToInt(Mathf.Abs(trackPoint.position.z - topleft.z) / offset);
			trackPoint.position = level.Terrain[i, j].position.WithY(trackPoint.position.y);
			// Set grid coordinates of the point
			trackPoint.gridCoords = new Vector2Int(i, j);
		}
	}

}
