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
			float probabilityOfCastingForward = 0.4f + 0.6f * ((float)(place - 1) / (float)(RaceControllerBase.Instance.racers.Count - 1)); // e.g. 1st --> 40 %, last --> 100 %
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
			// Choose opponent with a probability based on distance from the racer (the closest, the higher probability)
			return ChooseOpponentBasedOnDistance(potentialTargets, spell);
		} else {
			// Choose randomly from the two closest targets - so there is a chance Attractio would not be cast at the bonus the racer is currently flying to, which is better
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

	// Chooses a target opponent with probability based on distance (the closest one has the highest probability to be chosen)
	// If the same spell is already being cast at them, selects a different target
	private GameObject ChooseOpponentBasedOnDistance(List<GameObject> potentialTargets, Spell spell) {
		// Sort targets based on distance from the racer
		potentialTargets.Sort(
			(a, b) => 
			Vector3.Distance(a.transform.position, transform.position).CompareTo(Vector3.Distance(b.transform.position, transform.position))
		);
		// Choose one with probability based on distance
		// ... sum of sequence 1, 2, ..., potentialTargets.Count ==> probabilities will then be 1/sum (farthest), 2/sum, ..., potentialTargets.Count/sum (closest)
		int sum = (int)((potentialTargets.Count + 1) / 2f * potentialTargets.Count); // sum of sequence 1, 2, ..., potentialTargets.Count
		// ... get random number from 0 to sum-1 and find opponent to whose interval it belongs
		int randomValue = Random.Range(0, sum);
		int intervalEnd = 0;
		int targetIndex = 0;
		for (int i = 0; i < potentialTargets.Count; i++) {
			if (randomValue <= intervalEnd) {
				targetIndex = (potentialTargets.Count - 1 - i); // we are starting from the lowest probability belonging to the farthest opponent
				break;
			}
			intervalEnd += (i + 2);
		}
		// ... if the same spell is already being cast at that target, try to choose a different target (but don't check again for the spell)
		GameObject targetOpponent = potentialTargets[targetIndex];
		IncomingSpellsTracker incomingSpellTracker = targetOpponent.GetComponentInChildren<IncomingSpellsTracker>();
		if (incomingSpellTracker.IsSpellIncoming(spell.Identifier)) { // already being hit by the same spell
			if (targetIndex > 0) targetIndex--; // choose the next one which is closer
			else if (targetIndex < potentialTargets.Count - 1) targetIndex++; // choose the next one which is farther
		}

		return potentialTargets[targetIndex];
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

	#region Originally used methods for selecting target opponent (not used anymore but could still prove useful)

	// Tries to find an opponent who is the furthest in the race and the same spell is not already being cast at them
	// If no such opponent exists, then just the furthest one is chosen
	private GameObject ChooseOpponentFurthestWithoutOvercast(List<GameObject> potentialTargets, Spell spell) {
		// Choose opponent the furthest in the race but make sure the same spell is not already being cast at him
		GameObject bestTarget = null;
		int bestPlace = int.MaxValue;
		foreach (var opponent in potentialTargets) {
			int place = opponent.GetComponentInChildren<CharacterRaceState>().place;
			if (place < bestPlace) { // further in race than the best one found so far
				IncomingSpellsTracker incomingSpellTracker = opponent.GetComponentInChildren<IncomingSpellsTracker>();
				if (!incomingSpellTracker.IsSpellIncoming(spell.Identifier)) { // not already being hit by the same spell
					bestPlace = place;
					bestTarget = opponent;
				}
			}
		}
		// If there is no suitable target, choose simply the one who is the furthest in the race
		if (bestTarget != null) return bestTarget;
		else return ChooseOpponentFurthestInRace(potentialTargets);
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

	#endregion

	private void Start() {
		
	}

}
