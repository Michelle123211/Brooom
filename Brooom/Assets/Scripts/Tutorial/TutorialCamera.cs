using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class provides useful methods for working with camera in a tutorial - e.g. look at an object from a given distance and in a given direction
public class TutorialCamera : MonoBehaviour {

	[Tooltip("Virtual camera which is moved around and rotated to focus on a specific object as part of the tutorial.")]
	[SerializeField] CutsceneCamera cutsceneCamera;


	// Moves camera to look at the target object in the given direction and from the given distance
	// Offset could be specified to look at a different point than the origin
	public void MoveCameraToLookAt(Transform target, float distance, Vector3 direction, Vector3 lookAtOffset) {
		// Initialize position and rotation of camera and enable it
		Vector3 relativePosition = lookAtOffset - direction * distance;
		cutsceneCamera.SetToFollowObject(target, relativePosition, lookAtOffset);
		cutsceneCamera.gameObject.SetActive(true);
	}
	public void MoveCameraToLookAt(Transform target, float distance, Vector3 direction) {
		MoveCameraToLookAt(target, distance, direction, Vector3.zero); // zero offset
	}
	// Moves camera to look at the target object from the given position relative to the target
	// Offset could be specified to look at a different point than the origin
	public void MoveCameraToLookAt(Transform target, Vector3 relativePosition, Vector3 lookAtOffset) {
		// Initialize position and rotation of camera and enable it
		cutsceneCamera.SetToFollowObject(target, relativePosition, lookAtOffset);
		cutsceneCamera.gameObject.SetActive(true);
	}
	public void MoveCameraToLookAt(Transform target, Vector3 relativePosition) {
		MoveCameraToLookAt(target, relativePosition, Vector3.zero); // zero offset
	}

	public void ResetView() {
		// Disable camera
		cutsceneCamera.gameObject.SetActive(false);
	}

}
