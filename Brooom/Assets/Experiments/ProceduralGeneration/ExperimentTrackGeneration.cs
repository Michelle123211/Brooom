using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExperimentTrackGeneration : MonoBehaviour {

	[SerializeField] int pointCount = 37;
	[SerializeField] int pointDistance = 100;
	[SerializeField] float maxAngle = 30;
	[SerializeField] float minHeight = 0;
	[SerializeField] float maxHeight = 10;

	[SerializeField] List<Vector3> trackPoints = new List<Vector3>();

	public void Generate() {
		trackPoints.Clear();

		Vector3 lastPosition = Vector3.zero;
		Vector3 lastDirection = Vector3.right; //Vector3.forward;
		trackPoints.Add(lastPosition);
		while (trackPoints.Count < pointCount) {
			lastDirection = GetNextDirection(lastDirection, lastPosition);
			lastPosition += lastDirection * pointDistance;
			trackPoints.Add(lastPosition);
		}

		// Negative angle - up, positive angle - down
	}

	private Vector3 GetNextDirection(Vector3 lastDirection, Vector3 lastPosition) {
		// TODO: Don't let it go below zero
		// Find minimum and maximum angle so that minimum and maximum heights are satisfied
		Vector3 startPosition = lastPosition;
		Vector3 endPosition = lastPosition + Vector3.right * pointDistance; //lastPosition + Vector3.forward * pointDistance;
		Vector3 lowestPosition = endPosition.WithY(minHeight);
		Vector3 highestPosition = endPosition.WithY(maxHeight);

		float downAngle = Vector3.SignedAngle((endPosition - startPosition), (lowestPosition - startPosition), Vector3.back); //Vector3.right); // positive
		float upAngle = Vector3.SignedAngle((endPosition - startPosition), (highestPosition - startPosition), Vector3.back); //Vector3.right); // negative
		//Debug.Log($"Start position {startPosition}, end position {endPosition}, lowest position {lowestPosition}, highest position {highestPosition}. Minimum angle {downAngle}, Maximum angle {upAngle}");


		float angle = Random.Range(Mathf.Max(-maxAngle, upAngle), Mathf.Min(maxAngle, downAngle));
		return Quaternion.Euler(0, 0, -angle) * Vector3.right; //Quaternion.Euler(angle, 0, 0) * Vector3.forward;
	}

	void Start() {
		Generate();
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space))
			Generate();
	}

	private void OnDrawGizmos() {
		for (int i = 0; i < trackPoints.Count - 1; i++) {
			Gizmos.DrawCube(trackPoints[i], Vector3.one * 0.3f);
			if (i < trackPoints.Count - 2) {
				Gizmos.DrawLine(trackPoints[i], trackPoints[i + 1]);
			}
		}
	}

}
