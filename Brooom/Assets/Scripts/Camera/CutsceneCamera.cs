using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component which can be used to control camera during cutscenes.
/// It provides several different types of predefined behaviours, e.g., 
/// <c>Static</c> (fixed position and rotation), 
/// <c>StaticRelativeToObject</c> (position and rotation relative to a given object's position and rotation), 
/// <c>Moving</c> (from an initial position, in a given direction and with a given speed), 
/// <c>MovingRelativeToObject</c> (from an initial position and rotation relative to a given object's position and rotation, in a given direction and with a given speed), 
/// <c>FollowingObject</c> (always staying at the same position relative to a given object, looking at the object).
/// </summary>
public class CutsceneCamera : MonoBehaviour {
	[Tooltip("The type of camera behaviour.")]
	public CutsceneCameraBehaviour behaviourType;

	[Tooltip("Starting position of the camera (either absolute or relative to a target object, based on selected behaviour).")]
	public Vector3 position;
	[Tooltip("Starting rotation of the camera (either absolute or relative to a target object, based on selected behaviour), in Euler angles.")]
	public Vector3 rotation;

	[Tooltip("The position and rotation are given relatively to this object (as offsets).")]
	public Transform target;

	[Tooltip("The camera will look at point offset from the target object's origin by these values.")]
	public Vector3 lookAtOffset;

	[Tooltip("Direction in which the camera moves relatively to its rotation.")]
	public Vector3 direction;
	[Tooltip("Speed with which the camera moves in the given direction.")]
	public float speed;


	/// <summary>
	/// Sets the camera's behaviour to <c>CutsceneCameraBehaviour.Static</c>, i.e. fixed position and rotation.
	/// Also sets all parameters and initializes the camera's position and rotation accordingly.
	/// </summary>
	/// <param name="position">Absolute position of the camera.</param>
	/// <param name="rotation">Absolute rotation of the camera in Euler angles.</param>
	public void SetToStatic(Vector3 position, Vector3 rotation) {
		// Set parameters
		this.behaviourType = CutsceneCameraBehaviour.Static;
		this.position = position;
		this.rotation = rotation;
		// Initialize
		InitializePositionAndRotation();
	}

	/// <summary>
	/// Sets the camera's behaviour to <c>CutsceneCameraBehaviour.StaticRelativeToObject</c>, i.e. position and rotation relative to a given object's position and rotation.
	/// Also sets all parameters and initializes the camera's position and rotation accordingly.
	/// </summary>
	/// <param name="target">Target object relatively to which the camera's position and rotation are set (as offsets).</param>
	/// <param name="relativePosition">Position of the camera relative to the target's position.</param>
	/// <param name="relativeRotation">Rotation of the camera in Euler angles relative to the target's rotation.</param>
	public void SetToStaticRelativeToObject(Transform target, Vector3 relativePosition, Vector3 relativeRotation) {
		// Set parameters
		this.behaviourType = CutsceneCameraBehaviour.StaticRelativeToObject;
		this.target = target;
		this.position = relativePosition;
		this.rotation = relativeRotation;
		// Initialize
		InitializePositionAndRotation();
	}

	/// <summary>
	/// Sets the camera's behaviour to <c>CutsceneCameraBehaviour.Moving</c>, i.e. moving from an initial position, in a given direction and with a given speed.
	/// Also sets all parameters and initialized the camera's position and rotation accordingly.
	/// </summary>
	/// <param name="position">Starting position of the camera, absolute.</param>
	/// <param name="rotation">Absolute rotation of the camera in Euler angles.</param>
	/// <param name="direction">Direction in which the camera moves relatively to its rotation.</param>
	/// <param name="speed">Speed with which the camera moves.</param>
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

	/// <summary>
	/// Sets the camera's behaviour to <c>CutsceneCameraBehaviour.MovingRelativeToObject</c>, 
	/// i.e. moving from an initial position and rotation relative to a given object's position and rotation, in a given direction and with a given speed.
	/// Also sets all parameters and initialized the camera's position and rotation accordingly.
	/// </summary>
	/// <param name="target">Target object relatively to which the camera's position and rotation are set (as offsets).</param>
	/// <param name="relativePosition">Starting position of the camera relative to the target's position.</param>
	/// <param name="relativeRotation">Rotation of the camera in Euler angles relative to the target's rotation.</param>
	/// <param name="direction">Direction in which the camera moves relatively to its rotation.</param>
	/// <param name="speed">Speed with which the camera moves.</param>
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

	/// <summary>
	/// Sets the camera's behaviour to <c>CutsceneCameraBehaviour.FollowingObject</c>, i.e. always staying at the same position relative to a given object, looking at the object.
	/// Also sets all parameters and initializes the camera's position and rotation accordingly.
	/// </summary>
	/// <param name="target">Target object which is being followed by the camera.</param>
	/// <param name="relativePosition">Position of the camera relative to the target's position.</param>
	/// <param name="lookAtOffset">The camera will look at point offset from the target object's origin by these values.</param>
	public void SetToFollowObject(Transform target, Vector3 relativePosition, Vector3 lookAtOffset) {
		this.behaviourType = CutsceneCameraBehaviour.FollowingObject;
		this.position = relativePosition;
		this.lookAtOffset = lookAtOffset;
		this.target = target;
		InitializePositionAndRotation();
	}

	// Initializes position and rotation based on selected behaviour
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


/// <summary>
/// Differet options for <c>CutsceneCamera</c> component's behaviour.
/// </summary>
public enum CutsceneCameraBehaviour { 
	Static,
	StaticRelativeToObject,
	Moving,
	MovingRelativeToObject,
	FollowingObject
}