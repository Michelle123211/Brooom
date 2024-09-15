using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class SpellTargetSelection : MonoBehaviour {

	[Tooltip("SpellController component which is used for selecting and casting a spell.")]
	[SerializeField] protected SpellController spellController; // to get the currently selected spell

	[Tooltip("SpellTargetDetection component which is used to get a list of potential targets for the currently selected spell.")]
	[SerializeField] protected SpellTargetDetection spellTargetDetection; // to get a list of available targets

	public SpellTarget GetCurrentTarget() {
		Spell selectedSpell = spellController.GetCurrentlySelectedSpell();
		if (selectedSpell.TargetType == SpellTargetType.Self) {
			return new SpellTarget { TargetObject = spellController.gameObject };
		} else if (selectedSpell.TargetType == SpellTargetType.Direction) {
			return new SpellTarget { TargetObject = GetCurrentTargetObject() };
		} else if (selectedSpell.TargetType == SpellTargetType.Opponent || selectedSpell.TargetType == SpellTargetType.Object) {
			return new SpellTarget { TargetPosition = GetCurrentTargetPosition() };
		} else {
			throw new NotSupportedException("SpellTargetSelection.GetCurrentTarget() was called with invalid spell target type.");
		}
	}

	protected abstract GameObject GetCurrentTargetObject();

	protected abstract Vector3 GetCurrentTargetPosition();

}
