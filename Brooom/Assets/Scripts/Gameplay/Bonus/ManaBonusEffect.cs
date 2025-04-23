using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A bonus which increases the racer's mana.
/// </summary>
public class ManaBonusEffect : BonusEffect {

	[Tooltip("How much mana the player gets after picking the bonus up.")]
	public int manaAmount = 30;

	/// <inheritdoc/>
	public override void ApplyBonusEffect(CharacterMovementController character) {
		SpellController spellController = character.GetComponent<SpellController>();
		spellController.ChangeManaAmount(manaAmount);
	}

	/// <inheritdoc/>
	public override bool IsAvailable() {
		// Available if at least one spell is purchased (so the opponents can use spells even when the player does not have any equipped but has the option to do so)
		return PlayerState.Instance.availableSpellCount > 0;
	}
}
