using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBorderGeneration : LevelGeneratorModule {
	[Tooltip("What is the distance between two parallel borders limiting the track between two consecutive track points.")]
	[SerializeField] float trackWidth = 80f;

	[Tooltip("The border will be covering exactly this height range.")]
	[SerializeField] Vector2 borderHeightRange = new Vector2(-10f, 70f);

	[Tooltip("The border is closed in front of the first track pointat a distance specified by this parameter.")]
	[SerializeField] float startPadding = 150f;
	[Tooltip("The border is closed behind the finish line at a distance specified by this parameter.")]
	[SerializeField] float endPadding = 200f;

	[Tooltip("Prefab of the object used as a border.")]
	[SerializeField] GameObject borderPrefab;
	[Tooltip("An object which will be parent of all the border objects in the hierarchy.")]
	public Transform borderParent;

	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated borders
		UtilsMonoBehaviour.RemoveAllChildren(borderParent);

		Vector3 previousPoint1 = Vector3.zero, previousPoint2 = Vector3.zero;
		Vector3 nextPoint1, nextPoint2;
		// Generate borders for each segment
		for (int i = 0; i < level.track.Count - 1; i++) {
			// Track points around the current segment
			Vector3 segmentStart = level.track[i].position;
			Vector3 segmentEnd = level.track[i + 1].position;
			Vector3 segmentDirection = (segmentEnd - segmentStart).WithY(0);
			Vector3 segmentMidpoint = (segmentStart + segmentEnd) / 2;
			// Find orthogonal vector - in 2D, vector (y, -x) is orthogonal to vector (x, y)
			Vector3 segmentOrthogonal = new Vector3(segmentDirection.z, 0, -segmentDirection.x).normalized;
			// Initialize previous end points
			if (i == 0) {
				previousPoint1 = segmentStart + segmentOrthogonal * trackWidth / 2;
				previousPoint2 = segmentStart - segmentOrthogonal * trackWidth / 2;
			}
			// Get a point on each border
			Vector3 borderPoint1 = segmentMidpoint + segmentOrthogonal * trackWidth / 2;
			Vector3 borderPoint2 = segmentMidpoint - segmentOrthogonal * trackWidth / 2;
			// Find vector orthogonal to the next hoop direction
			Vector3 hoopDirection = GetTrackPointDirection(level, i + 1);
			Vector3 orthogonalHoopDirection = new Vector3(hoopDirection.z, 0, -hoopDirection.x).normalized;
			// Get two points on the orthogonal vector
			Vector3 orthogonalHoopDirectionPoint1 = segmentEnd + orthogonalHoopDirection;
			Vector3 orthogonalHoopDirectionPoint2 = segmentEnd - orthogonalHoopDirection;
			// Find next end points (from intersections of lines)
			Utils.TryGetLineIntersectionXZ(
				previousPoint1, borderPoint1,
				orthogonalHoopDirectionPoint1, orthogonalHoopDirectionPoint2,
				out nextPoint1);
			Utils.TryGetLineIntersectionXZ(
				previousPoint2, borderPoint2,
				orthogonalHoopDirectionPoint1, orthogonalHoopDirectionPoint2,
				out nextPoint2);
			// Instantiate borders
			InstantiateBorder(previousPoint1, nextPoint1);//, segmentDirection);
			InstantiateBorder(previousPoint2, nextPoint2);//, segmentDirection);
			// Move to the next track point
			previousPoint1 = nextPoint1;
			previousPoint2 = nextPoint2;
		}
		// Close the start
		CloseBorder(level.track[0].position, Vector3.back, startPadding);
		// Close the end
		Vector3 endDirection = Vector3.forward;
		if (level.track.Count > 1) endDirection = level.track[level.track.Count - 1].position - level.track[level.track.Count - 2].position;
		CloseBorder(level.track[level.track.Count - 1].position, endDirection, endPadding);
	}

	private void InstantiateBorder(Vector3 startPoint, Vector3 endPoint) {//, Vector3 direction) {
		Vector3 direction = (endPoint - startPoint).WithY(0).normalized;
		// Border length and position
		float length = (endPoint - startPoint).magnitude;
		Vector3 center = ((startPoint + endPoint) / 2).WithY((borderHeightRange.x + borderHeightRange.y) / 2);
		// Instantiate borders
		GameObject borderInstance = Instantiate(borderPrefab, center, Quaternion.LookRotation(direction, Vector3.up), borderParent);
		borderInstance.transform.localScale = new Vector3(1, borderHeightRange.y - borderHeightRange.x, length + 0.1f);
	}

	private void CloseBorder(Vector3 position, Vector3 direction, float padding) {
		Vector3 direction2D = direction.WithY(0).normalized;
		// Get orthogonal vector
		Vector3 orthogonal = new Vector3(direction2D.z, 0, -direction2D.x).normalized;
		// Get first pair of corners
		Vector3 corner1 = position + orthogonal * trackWidth / 2;
		Vector3 corner4 = position - orthogonal * trackWidth / 2;
		// Get second pair of corners
		Vector3 end = position + direction2D * padding;
		Vector3 corner2 = end + orthogonal * trackWidth / 2;
		Vector3 corner3 = end - orthogonal * trackWidth / 2;
		// Instantiate borders
		InstantiateBorder(corner1, corner2);
		InstantiateBorder(corner2, corner3);
		InstantiateBorder(corner4, corner3);
	}

	private Vector3 GetTrackPointDirection(LevelRepresentation level, int trackPointIndex) {
		TrackPoint point = level.track[trackPointIndex];
		// Orientation is given by the vector from the previous hoop to the next hoop
		Vector3 previousPosition = point.position, nextPosition = point.position;
		if (trackPointIndex > 0)
			previousPosition = level.track[trackPointIndex - 1].position;
		if (trackPointIndex < level.track.Count - 1)
			nextPosition = level.track[trackPointIndex + 1].position;
		return (nextPosition - previousPosition).WithY(0); // Y = 0 to rotate only around the Y axis
	}
}
