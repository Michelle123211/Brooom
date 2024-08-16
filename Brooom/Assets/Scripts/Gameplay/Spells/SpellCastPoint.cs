using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCastPoint : RelativeOffsetComponent {

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(GetAbsolutePosition(), 0.05f);
	}

}
