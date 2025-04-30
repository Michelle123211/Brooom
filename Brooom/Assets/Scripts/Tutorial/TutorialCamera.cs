using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class providing useful methods for working with camera during tutorial,
/// e.g. to look at an object from a given distance and in a given direction.
/// </summary>
public class TutorialCamera : MonoBehaviour {

	[Tooltip("Virtual camera which is moved around and rotated to focus on a specific object as part of the tutorial.")]
	[SerializeField] CutsceneCamera cutsceneCamera;


	/// <summary>
	/// Moves the camera to look at the target object in the given direction and from the given distance.
	/// An offset can be specified to look at a different point than the target's origin.
	/// If the target object moves, the camera will move as well.
	/// </summary>
	/// <param name="target">Target object to look at.</param>
	/// <param name="distance">Distance between the camera and the target object.</param>
	/// <param name="direction">Direction in which to look at the given target object.</param>
	/// <param name="lookAtOffset">A relative offset from the target object's origin to look at.</param>
	public void MoveCameraToLookAt(Transform target, float distance, Vector3 direction, Vector3 lookAtOffset) {
		// Initialize position and rotation of camera and enable it
		Vector3 relativePosition = lookAtOffset - direction * distance;
		cutsceneCamera.SetToFollowObject(target, relativePosition, lookAtOffset);
		cutsceneCamera.gameObject.SetActive(true);
	}

	/// <summary>
	/// Moves the camera to look at the target object in the given direction and from the given distance.
	/// If the target object moves, the camera will move as well.
	/// </summary>
	/// <param name="target">Target object to look at.</param>
	/// <param name="distance">Distance between the camera and the target object.</param>
	/// <param name="direction">Direction in which to look at the given target object.</param>
	public void MoveCameraToLookAt(Transform target, float distance, Vector3 direction) {
		MoveCameraToLookAt(target, distance, direction, Vector3.zero); // zero offset
	}

	/// <summary>
	/// Moves the camera to look at the target object from the given position relative to it.
	/// An offset can be specified to look at a different point than the target's origin.
	/// If the target object moves, the camera will move as well.
	/// </summary>
	/// <param name="target">Target object to look at.</param>
	/// <param name="relativePosition">Position of the camera relative to the target.</param>
	/// <param name="lookAtOffset">A relative offset from the target object's origin to look at.</param>
	public void MoveCameraToLookAt(Transform target, Vector3 relativePosition, Vector3 lookAtOffset) {
		// Initialize position and rotation of camera and enable it
		cutsceneCamera.SetToFollowObject(target, relativePosition, lookAtOffset);
		cutsceneCamera.gameObject.SetActive(true);
	}

	/// <summary>
	/// Moves the camera to look at the target object from the given position relative to it.
	/// If the target object moves, the camera will move as well.
	/// </summary>
	/// <param name="target">Target object to look at.</param>
	/// <param name="relativePosition">Position of the camera relative to the target.</param>
	public void MoveCameraToLookAt(Transform target, Vector3 relativePosition) {
		MoveCameraToLookAt(target, relativePosition, Vector3.zero); // zero offset
	}

	/// <summary>
	/// Disables the tutorial camera to reset the view back to the player's camera.
	/// </summary>
	public void ResetView() {
		// Disable camera
		cutsceneCamera.gameObject.SetActive(false);
	}

}
