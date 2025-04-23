using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A bonus which immediately recharges all racer's equipped spells (so they are no longer on cooldown).
/// </summary>
public class RechargeSpellsBonusEffect : BonusEffect {

	/// <inheritdoc/>
	public override void ApplyBonusEffect(CharacterMovementController character) {
		SpellController spellController = character.GetComponent<SpellController>();
		spellController.RechargeAllSpells();
	}

	/// <inheritdoc/>
	public override bool IsAvailable() {
		// Available if at least one spell is purchased (so the opponents can use spells even when the player does not have any equipped but has the option to do so)
		return PlayerState.Instance.availableSpellCount > 0;
	}
}
