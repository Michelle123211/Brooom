using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellTargetSelection : SpellTargetSelection {

	protected override Vector3 GetCurrentTargetDirection() {
		// Select simply the forward direction
		// TODO: Make the decision better
		return transform.forward;
	}

	protected override GameObject GetCurrentTargetObject() {
		// Select random target from all possible ones
		// TODO: Make the decision better
		List<GameObject> potentialTargets = spellTargetDetection.GetPotentialTargetsForSelectedSpell();
		if (potentialTargets == null || potentialTargets.Count == 0) return null;
		return potentialTargets[Random.Range(0, potentialTargets.Count)];
	}

	private void Start() {
		
	}

}
