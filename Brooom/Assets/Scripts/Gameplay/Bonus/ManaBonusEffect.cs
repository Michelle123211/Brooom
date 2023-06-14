using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBonusEffect : BonusEffect {
	public override void ApplyBonusEffect(PlayerController player) {
		Debug.Log("Increased mana.");
	}
}
