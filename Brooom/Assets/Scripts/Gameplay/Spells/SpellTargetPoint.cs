using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class SpellTargetPoint : RelativeOffsetComponent {

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(GetAbsolutePosition(), 0.05f);
	}

}
