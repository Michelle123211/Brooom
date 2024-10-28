using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellTargetSelection : SpellTargetSelection {

	[Tooltip("Minimum angle in degrees which is used to offset a forward vector when casting a negative spell in some direction (so the racer does not cast it right in front of themselves).")]
	[SerializeField] float minAngleOffset = 2f;
	[Tooltip("Maximum angle in degrees which is used to offset a forward vector when casting a negative spell in some direction (so the racer does not cast it right in front of themselves).")]
	[SerializeField] float maxAngleOffset = 5f;

	protected override Vector3 GetCurrentTargetDirection() {
		// Select simply the forward direction
		if (spellController.GetCurrentlySelectedSpell().IsPositive) {
			// If the spell is positive, cast it in the direction forward
			return transform.forward;
		} else {
			// Otherwise, cast it either a bit to the side or to the back - choose randomly with probability distribution according to place in race
			int place = transform.parent.GetComponentInChildren<CharacterRaceState>().place;
			float probabilityOfCastingForward = 0.4f + 0.6f * ((float)(place - 1) / (float)(RaceController.Instance.racers.Count - 1)); // e.g. 1st --> 40 %, last --> 100 %
			if (Random.value < probabilityOfCastingForward) return ChooseForwardDirectionWithOffset(); // forward with an offset
			else return -transform.forward; // back
		}
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

	private Vector3 ChooseForwardDirectionWithOffset() {
		Vector3 direction = transform.forward;
		// Choose a random offset as two angles (one for offset from forward direction (rotation around right vector), another one for rotation (around forward vector))
		float offsetAngle = minAngleOffset + (Random.value * (maxAngleOffset - minAngleOffset)); // only positive angle is necessary, subsequent rotation may bring it to the negative part
		direction = Quaternion.AngleAxis(offsetAngle, transform.right) * direction; // offset
		float rotationAngle = Random.value * 360f;
		direction = Quaternion.AngleAxis(rotationAngle, transform.forward) * direction; // rotation
		return direction.normalized;
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
