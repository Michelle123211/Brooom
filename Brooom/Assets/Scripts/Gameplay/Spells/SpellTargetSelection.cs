using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// A base class for selecting a target for the currently selected spell.
/// </summary>
public abstract class SpellTargetSelection : MonoBehaviour {

	[Tooltip("SpellController component which is used for selecting and casting a spell.")]
	[SerializeField] protected SpellController spellController;

	[Tooltip("SpellTargetDetection component which is used to get a list of potential targets for the currently selected spell.")]
	[SerializeField] protected SpellTargetDetection spellTargetDetection;

	[Tooltip("Distance in which the target position is placed when casting a spell with direction as a target type.")]
	[SerializeField] float targetPositionDistance = 30f;

	/// <summary>
	/// Selects a suitable spell target for the currently selected spell based on its target type.
	/// </summary>
	/// <returns>Current spell target as a <c>SpellTarget</c>.</returns>
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

	/// <summary>
	/// Selects a suitable target object for the currently selected spell based on some strategy.
	/// </summary>
	/// <returns>A GameObject which is a target of the currently selected spell.</returns>
	protected abstract GameObject GetCurrentTargetObject();

	/// <summary>
	/// Selects a direction in which the currently selected spell should be cast based on some strategy.
	/// </summary>
	/// <returns>A direction in which the currently selected spell should be cast.</returns>
	protected abstract Vector3 GetCurrentTargetDirection();

	/// <summary>
	/// Selects a target position of the currently selected spell by first calling <c>GetCurrentTargetDirection()</c> to get target direction
	/// and then moving from current racer position a certain distance (stored in <c>targetPositionDistance</c>) in that direction.
	/// </summary>
	/// <returns>Target position of the currently selected spell.</returns>
	private Vector3 GetCurrentTargetPosition() {
		// Target position is a certain distance away in the target direction
		return spellController.transform.position + targetPositionDistance * GetCurrentTargetDirection();
	}

}
