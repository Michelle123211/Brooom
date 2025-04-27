using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component identifying a possible spell target while also representing a point to which spells should be cast.
/// The point is specified as an offset relatively to the object's origin.
/// This component should be on an object with collider, so that the spell target can be detected.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SpellTargetPoint : RelativeOffsetComponent {

	// Draw gizmo for the position represented by this component
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(GetAbsolutePosition(), 0.05f);
	}

}
