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
		// Select a single target from all possible ones
		List<GameObject> potentialTargets = spellTargetDetection.GetPotentialTargetsForSelectedSpell();
		if (potentialTargets == null || potentialTargets.Count == 0) return null;
		// Decide based on spell target type
		Spell spell = spellController.GetCurrentlySelectedSpell();
		if (spell.TargetType == SpellTargetType.Opponent) {
			// Choose opponent the furthest in the race
			return ChooseOpponentFurthestInRace(potentialTargets);
		} else {
			// Choose randomly from the two closest targets - so there is a chance Attractio would not be casted at the bonus the racer is currently flying to, which is better
			return ChooseClosestOrSecondClosestTarget(potentialTargets);
		}
	}

	private GameObject ChooseOpponentFurthestInRace(List<GameObject> potentialTargets) {
		// Choose opponent the furthest in the race
		int bestPlace = int.MaxValue;
		GameObject bestRacer = null;
		foreach (var opponent in potentialTargets) {
			int place = opponent.GetComponentInChildren<CharacterRaceState>().place;
			if (place < bestPlace) {
				bestPlace = place;
				bestRacer = opponent;
			}
		}
		return bestRacer;
	}

	private GameObject ChooseClosestOrSecondClosestTarget(List<GameObject> potentialTargets) {
		// Choose randomly from the two closest targets (if available)
		float minDistance = float.MaxValue;
		GameObject closestTarget = null;
		float secondMinDistance = float.MaxValue;
		GameObject secondClosestTarget = null;
		foreach (var target in potentialTargets) {
			float distance = Vector3.Distance(target.transform.position, transform.position);
			if (distance < minDistance) {
				secondMinDistance = minDistance;
				secondClosestTarget = closestTarget;
				minDistance = distance;
				closestTarget = target;
			} else if (distance < secondMinDistance) {
				secondMinDistance = distance;
				secondClosestTarget = target;
			}
		}
		if (secondClosestTarget == null) return closestTarget;
		return (Random.value <= 0.5f ? closestTarget : secondClosestTarget);
	}

	private void Start() {
		
	}

}
