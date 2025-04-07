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

		GenerateBorders(level);
	}

	private void GenerateBorders(LevelRepresentation level) {

		// Start from points to the sides of the first hoop
		Vector3 previousDirection = GetSegmentDirection(level, 0);
		Vector3 orthogonalDirection = GetOrthogonalDirectionXZ(previousDirection);
		Vector3 previousPoint1 = level.Track[0].position + orthogonalDirection * (trackWidth / 2);
		Vector3 previousPoint2 = level.Track[0].position - orthogonalDirection * (trackWidth / 2);

		Vector3 nextPoint1, nextPoint2, nextDirection;

		// Generate border around start
		CloseBorder(previousPoint1, previousPoint2, -previousDirection, startPadding);
		// Generate borders for each segment between two hoops
		for (int i = 0; i < level.Track.Count - 1; i++) {
			// Get direction of next segment
			nextDirection = GetSegmentDirection(level, i + 1);
			orthogonalDirection = GetOrthogonalDirectionXZ(nextDirection);
			// Get points to the side of next hoop
			Vector3 point1 = level.Track[i + 1].position + orthogonalDirection * (trackWidth / 2);
			Vector3 point2 = level.Track[i + 1].position - orthogonalDirection * (trackWidth / 2);
			// Get end points for border
			if (i < level.Track.Count - 2) { // if not the last segment, compute intersection with the last one
				bool isIntersection1 = Utils.TryGetLineIntersectionXZ(
					previousPoint1, previousPoint1 + previousDirection,
					point1, point1 + nextDirection,
					out nextPoint1);
				bool isIntersection2 = Utils.TryGetLineIntersectionXZ(
					previousPoint2, previousPoint2 + previousDirection,
					point2, point2 + nextDirection,
					out nextPoint2);
				if (!isIntersection1 || !isIntersection2) {
					nextPoint1 = point1;
					nextPoint2 = point2;
				}
			} else { // of the last segment, simply take points to the sides of the hoop
				nextPoint1 = point1;
				nextPoint2 = point2;
			}
			// Instantiate border
			InstantiateBorder(previousPoint1, nextPoint1); // right side
			InstantiateBorder(previousPoint2, nextPoint2); // left side
			// Move to next segment
			previousDirection = nextDirection;
			previousPoint1 = nextPoint1;
			previousPoint2 = nextPoint2;
		}
		// Generate border around finish
		CloseBorder(previousPoint1, previousPoint2, previousDirection, endPadding);
	}

	private Vector3 GetOrthogonalDirectionXZ(Vector3 direction) {
		return Quaternion.Euler(0, 90, 0) * direction.WithY(0).normalized;
	}

	private Vector3 GetSegmentDirection(LevelRepresentation level, int startHoopIndex) {
		if (level.Track.Count < 2) // not enough points for a segment, return simply forward (implicit first direction when generating track)
			return Vector3.forward;
		else if (startHoopIndex == level.Track.Count - 1) // last point, there is no segment starting here so return direction of the previous one
			return (level.Track[level.Track.Count - 1].position - level.Track[level.Track.Count - 2].position).WithY(0).normalized;
		else
			return (level.Track[startHoopIndex + 1].position - level.Track[startHoopIndex].position).WithY(0).normalized;
	}

	private void CloseBorder(Vector3 point1, Vector3 point2, Vector3 direction, float padding) {
		Vector3 corner1 = point1 + direction * padding;
		Vector3 corner2 = point2 + direction * padding;
		// Instantiate borders
		InstantiateBorder(point1, corner1);
		InstantiateBorder(point2, corner2);
		InstantiateBorder(corner1, corner2);
	}

	private void InstantiateBorder(Vector3 startPoint, Vector3 endPoint) {//, Vector3 direction) {
		//Debug.Log($"Border from {startPoint} to {endPoint}");
		Vector3 direction = (endPoint - startPoint).WithY(0).normalized;
		// Border length and position
		float length = (endPoint - startPoint).magnitude;
		Vector3 center = ((startPoint + endPoint) / 2).WithY((borderHeightRange.x + borderHeightRange.y) / 2);
		// Instantiate borders
		GameObject borderInstance = Instantiate(borderPrefab, center, Quaternion.LookRotation(direction, Vector3.up), borderParent);
		borderInstance.transform.localScale = new Vector3(1, borderHeightRange.y - borderHeightRange.x, length + 0.1f);
	}
}
