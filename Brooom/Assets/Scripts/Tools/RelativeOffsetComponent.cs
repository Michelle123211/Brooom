using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RelativeOffsetComponent : MonoBehaviour {

	[Tooltip("Relative offset from the origin of the GameObject this component is attached to.")]
	[SerializeField] Vector3 relativeOffset;

	public Vector3 GetAbsolutePosition() {
		return transform.TransformPoint(relativeOffset);
	}

}
