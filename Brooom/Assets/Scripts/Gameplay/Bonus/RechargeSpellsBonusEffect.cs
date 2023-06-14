using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Immediately recharges all equipped spells
public class RechargeSpellsBonusEffect : BonusEffect {
	public override void ApplyBonusEffect(PlayerController player) {
		PlayerState.Instance.raceState.RechargeAllSpells();
	}
}
