using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationBonusEffect : BonusEffect {
	public override void ApplyBonusEffect(PlayerController player) {
		Debug.Log("Navigation trail.");
	}
}
