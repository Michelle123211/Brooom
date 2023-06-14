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
}
