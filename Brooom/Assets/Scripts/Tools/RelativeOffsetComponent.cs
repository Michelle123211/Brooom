using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component for specifying a point as an offset relatively to the object's origin.
/// It provides a method to get the final point as an absolute (world) position.
/// </summary>
public abstract class RelativeOffsetComponent : MonoBehaviour {

	[Tooltip("Relative offset from the origin of the GameObject this component is attached to.")]
	[SerializeField] Vector3 relativeOffset;

	/// <summary>
	/// Computes a world position out of the object's origin and the relative offset specified in <c>relativeOffset</c>.
	/// </summary>
	/// <returns>World position represented by the component.</returns>
	public Vector3 GetAbsolutePosition() {
		return transform.TransformPoint(relativeOffset);
	}

}
