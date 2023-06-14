using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBonusEffect : BonusEffect
{
	[Tooltip("How much speed is added on top of the normal speed.")]
	public float speedAdded = 50;
	[Tooltip("Duration of the effect in seconds.")]
	public float duration = 8;

	public override void ApplyBonusEffect(PlayerController player) {
		Debug.Log("Increased speed.");
		player.SetBonusSpeed(speedAdded, duration);
	}
}
