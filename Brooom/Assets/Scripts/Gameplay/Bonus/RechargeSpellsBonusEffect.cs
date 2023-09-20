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
		// Available if at least one spell is equipped
		// TODO: Maybe change later to at least one spell purchased (so the opponents can pick the bonus up even when the player does not have any spell equipped)
		foreach (var spell in PlayerState.Instance.equippedSpells) {
			if (spell != null) return true;
		}
		return false;
	}
}
