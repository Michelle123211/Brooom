using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component controlling a minimap camera, which is placed above a particular object and follows it at a constant height.
/// </summary>
public class MinimapCameraController : MonoBehaviour
{
    [Tooltip("Camera will follow this object and take its position and rotation along the Y axis.")]
    [SerializeField] Transform objectToFollow;

	[Tooltip("Camera will be floating in this height above the followed object.")]
	[SerializeField] float heightAboveObject = 100;

	private void LateUpdate() {
		Vector3 position = objectToFollow.position;
		transform.position = position.WithY(position.y + heightAboveObject);
		transform.rotation = Quaternion.Euler(90, objectToFollow.eulerAngles.y, 0); // rotate 90 degrees around X to look down
	}
}
