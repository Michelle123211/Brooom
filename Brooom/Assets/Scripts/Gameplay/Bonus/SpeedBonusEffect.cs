using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Speed the player up for a short time
public class SpeedBonusEffect : BonusEffect
{
	[Tooltip("How much speed is added on top of the normal speed.")]
	public float speedAdded = 10;
	[Tooltip("Duration of the effect in seconds.")]
	public float duration = 8;

	public override void ApplyBonusEffect(PlayerController player) {
		player.SetBonusSpeed(speedAdded, duration);
	}

	public override bool IsAvailable() {
		return true; // always available
	}
}
