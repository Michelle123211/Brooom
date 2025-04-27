using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a point from which spells should be cast.
/// The point is specified as an offset relatively to the object's origin.
/// </summary>
public class SpellCastPoint : RelativeOffsetComponent {

	// Draw gizmo for the position represented by this component
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(GetAbsolutePosition(), 0.05f);
	}

}
