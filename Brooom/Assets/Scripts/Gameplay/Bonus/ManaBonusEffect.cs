using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Increases the player's mana
public class ManaBonusEffect : BonusEffect {
	[Tooltip("How much mana the player gets after picking the bonus up.")]
	public int manaAmount = 30;

	public override void ApplyBonusEffect(CharacterMovementController character) {
		SpellController spellController = character.GetComponent<SpellController>();
		spellController.ChangeManaAmount(manaAmount);
	}

	public override bool IsAvailable() {
		// Available if at least one spell is purchased (so the opponents can use spells even when the player does not have any equipped but has the option to do so)
		foreach (var spell in PlayerState.Instance.spellAvailability) {
			if (spell.Value) // spell is available
				return true;
		}
		return false;
	}
}
