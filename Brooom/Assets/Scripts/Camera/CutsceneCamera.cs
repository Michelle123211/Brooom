using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneCamera : MonoBehaviour {
	[Tooltip("The type of camera behaviour.")]
	[SerializeField] CutsceneCameraBehaviour behaviourType;

	[Tooltip("Starting position of the camera.")]
	[SerializeField] Vector3 position;
	[Tooltip("Starting rotation of the camera.")]
	[SerializeField] Vector3 rotation;

	[Tooltip("The position and rotation are given relatively to this object (as offsets).")]
	[SerializeField] Transform target;

	[Tooltip("Direction in which the camera moves relatively to its rotation.")]
	[SerializeField] Vector3 direction;
	[Tooltip("Speed with which the camera moves in the given direction.")]
	[SerializeField] float speed;


	private void SetAbsolutePosition() {
		transform.position = position;
	}

	private void SetRelativePosition() {
		// Offset the camera relatively to the target object and its orientation
		transform.position = target.position + Quaternion.FromToRotation(Vector3.forward, target.forward) * position;
	}

	private void SetAbsoluteRotation() {
		transform.eulerAngles = rotation;
	}

	private void SetRelativeRotation() {
		// Rotate the camera relatively to the rotation of the target object
		transform.eulerAngles = target.rotation.eulerAngles + rotation;
	}

	private void Move() {
		// Move in the given direction relatively to the camera's orientation
		transform.position += transform.TransformDirection(direction) * speed * Time.deltaTime;
	}

	private void LookAt() {
		Vector3 lookDirection = target.position - transform.position;
		transform.rotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
	}

	private void Follow() {
		SetRelativePosition();
		LookAt();
	}

	private void Awake() {
		// Normalize the direction
		direction.Normalize();
		// Set initial position
		switch (behaviourType) {
			case CutsceneCameraBehaviour.Static:
			case CutsceneCameraBehaviour.Moving:
				SetAbsolutePosition();
				break;
			case CutsceneCameraBehaviour.StaticRelativeToObject:
			case CutsceneCameraBehaviour.MovingRelativeToObject:
			case CutsceneCameraBehaviour.FollowingObject:
				SetRelativePosition();
				break;
		}
		// Set initial rotation
		switch (behaviourType) {
			case CutsceneCameraBehaviour.Static:
			case CutsceneCameraBehaviour.Moving:
				SetAbsoluteRotation();
				break;
			case CutsceneCameraBehaviour.StaticRelativeToObject:
			case CutsceneCameraBehaviour.MovingRelativeToObject:
				SetRelativeRotation();
				break;
			case CutsceneCameraBehaviour.FollowingObject:
				LookAt();
				break;
		}
	}

	private void Update() {
		// Move the camera in the given direction with the given speed
		switch (behaviourType) {
			case CutsceneCameraBehaviour.Moving:
			case CutsceneCameraBehaviour.MovingRelativeToObject:
				Move();
				break;
		}
	}

	private void LateUpdate() {
		// Follow the target object
		switch (behaviourType) {
			case CutsceneCameraBehaviour.FollowingObject:
				Follow();
				break;
		}
	}

}


public enum CutsceneCameraBehaviour { 
	Static,
	StaticRelativeToObject,
	Moving,
	MovingRelativeToObject,
	FollowingObject
}