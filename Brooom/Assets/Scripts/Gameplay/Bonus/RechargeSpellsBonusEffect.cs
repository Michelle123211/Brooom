using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Immediately recharges all equipped spells
public class RechargeSpellsBonusEffect : BonusEffect {
	public override void ApplyBonusEffect(CharacterMovementController character) {
		SpellController spellController = character.GetComponent<SpellController>();
		spellController.RechargeAllSpells();
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
