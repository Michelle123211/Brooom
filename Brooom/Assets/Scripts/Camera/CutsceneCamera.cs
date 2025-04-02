using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneCamera : MonoBehaviour {
	[Tooltip("The type of camera behaviour.")]
	public CutsceneCameraBehaviour behaviourType;

	[Tooltip("Starting position of the camera.")]
	public Vector3 position;
	[Tooltip("Starting rotation of the camera.")]
	public Vector3 rotation;

	[Tooltip("The position and rotation are given relatively to this object (as offsets).")]
	public Transform target;

	[Tooltip("The camera will look at point offset from the target object origin by these values.")]
	public Vector3 lookAtOffset;

	[Tooltip("Direction in which the camera moves relatively to its rotation.")]
	public Vector3 direction;
	[Tooltip("Speed with which the camera moves in the given direction.")]
	public float speed;


	// Sets the camera's behaviour to CutsceneCameraBehaviour.Static
	// Sets all parameters and initializes the camera's position and rotation accordingly
	public void SetToStatic(Vector3 position, Vector3 rotation) {
		// Set parameters
		this.behaviourType = CutsceneCameraBehaviour.Static;
		this.position = position;
		this.rotation = rotation;
		// Initialize
		InitializePositionAndRotation();
	}

	// Sets the camera's behaviour to CutsceneCameraBehaviour.StaticRelativeToObject
	// Sets all parameters and initializes the camera's position and rotation accordingly
	public void SetToStaticRelativeToObject(Transform target, Vector3 relativePosition, Vector3 relativeRotation) {
		// Set parameters
		this.behaviourType = CutsceneCameraBehaviour.StaticRelativeToObject;
		this.target = target;
		this.position = relativePosition;
		this.rotation = relativeRotation;
		// Initialize
		InitializePositionAndRotation();
	}

	// Sets the camera's behaviour to CutsceneCameraBehaviour.Moving
	// Sets all parameters and initializes the camera's position and rotation accordingly
	public void SetToMoving(Vector3 position, Vector3 rotation, Vector3 direction, float speed) {
		// Set parameters
		this.behaviourType = CutsceneCameraBehaviour.Moving;
		this.position = position;
		this.rotation = rotation;
		this.direction = direction;
		this.speed = speed;
		// Initialize
		InitializePositionAndRotation();
	}

	// Sets the camera's behaviour to CutsceneCameraBehaviour.MovingRelativeToObject
	// Sets all parameters and initializes the camera's position and rotation accordingly
	public void SetToMovingRelativeToObject(Transform target, Vector3 relativePosition, Vector3 relativeRotation, Vector3 direction, float speed) {
		// Set parameters
		this.behaviourType = CutsceneCameraBehaviour.MovingRelativeToObject;
		this.target = target;
		this.position = relativePosition;
		this.rotation = relativeRotation;
		this.direction = direction;
		this.speed = speed;
		// Initialize
		InitializePositionAndRotation();
	}

	// Sets the camera's behaviour to CutsceneCameraBehaviour.FollowingObject
	// Sets all parameters and initializes the camera's position and rotation accordingly
	public void SetToFollowObject(Transform target, Vector3 relativePosition, Vector3 lookAtOffset) {
		this.behaviourType = CutsceneCameraBehaviour.FollowingObject;
		this.position = relativePosition;
		this.lookAtOffset = lookAtOffset;
		this.target = target;
		InitializePositionAndRotation();
	}

	private void InitializePositionAndRotation() {
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
		Vector3 lookDirection = target.TransformPoint(lookAtOffset) - transform.position;
		transform.rotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
		transform.eulerAngles = transform.eulerAngles.WithZ(0);
	}

	private void Follow() {
		SetRelativePosition();
		LookAt();
	}

	private void Awake() {
		InitializePositionAndRotation();
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