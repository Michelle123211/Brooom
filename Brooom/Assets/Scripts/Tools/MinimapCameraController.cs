using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    [Tooltip("Camera will follow this object and take its position and rotation along the Y axis.")]
    [SerializeField] Transform objectToFollow;

	[Tooltip("Camera will be floating in this height above the followed object.")]
	[SerializeField] float heightAboveObject = 10;

	private void LateUpdate() {
		Vector3 position = objectToFollow.position;
		transform.position = position.WithY(position.y + heightAboveObject);
		transform.localEulerAngles = (Vector3.right * 90).WithY(objectToFollow.localEulerAngles.y); // rotate 90 degrees around X to look down
	}
}
