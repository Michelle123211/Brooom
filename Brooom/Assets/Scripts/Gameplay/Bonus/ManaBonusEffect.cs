using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Increases the player's mana
public class ManaBonusEffect : BonusEffect {
	[Tooltip("How much mana the player gets after picking the bonus up.")]
	public int manaAmount = 30;

	public override void ApplyBonusEffect(PlayerController player) {
		PlayerState.Instance.raceState.ChangeManaAmount(manaAmount);
	}

	public override bool IsAvailable() {
		// Available if at least one spell is equipped
		// TODO: Maybe change later to at least one spell purchased (so the opponents can use spells even when the player does not have any equipped)
		foreach (var spell in PlayerState.Instance.raceState.spellSlots) {
			if (spell != null) return true;
		}
		return false;
	}
}
