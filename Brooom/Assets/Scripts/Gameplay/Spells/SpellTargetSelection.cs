using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class SpellTargetSelection : MonoBehaviour {

	[Tooltip("SpellController component which is used for selecting and casting a spell.")]
	[SerializeField] protected SpellController spellController; // to get the currently selected spell

	[Tooltip("SpellTargetDetection component which is used to get a list of potential targets for the currently selected spell.")]
	[SerializeField] protected SpellTargetDetection spellTargetDetection; // to get a list of available targets

	[Tooltip("Distance in which the target position is placed when casting a spell with direction as a target type.")]
	[SerializeField] float targetPositionDistance = 30f;

	public SpellTarget GetCurrentTarget() {
		Spell selectedSpell = spellController.GetCurrentlySelectedSpell();
		if (selectedSpell.TargetType == SpellTargetType.Self) {
			return new SpellTarget { TargetObject = spellController.gameObject };
		} else if (selectedSpell.TargetType == SpellTargetType.Direction) {
			return new SpellTarget { TargetPosition = GetCurrentTargetPosition() };
		} else if (selectedSpell.TargetType == SpellTargetType.Opponent || selectedSpell.TargetType == SpellTargetType.Object) {
			return new SpellTarget { TargetObject = GetCurrentTargetObject() };
		} else {
			throw new NotSupportedException("SpellTargetSelection.GetCurrentTarget() was called with invalid spell target type.");
		}
	}

	// Returns a GameObject which is a target of the currently selected spell
	protected abstract GameObject GetCurrentTargetObject();

	// Returns a direction in which the currently selected spell should be cast
	protected abstract Vector3 GetCurrentTargetDirection();

	private Vector3 GetCurrentTargetPosition() {
		// Target position is a certain distance away in the target direction
		return spellController.transform.position + targetPositionDistance * GetCurrentTargetDirection();
	}

}
