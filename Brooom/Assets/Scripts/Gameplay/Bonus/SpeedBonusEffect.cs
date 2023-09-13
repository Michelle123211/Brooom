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
	[Tooltip("An icon which will be used in the UI of effects affecting the player.")]
	[SerializeField] Sprite speedIcon;

	public override void ApplyBonusEffect(CharacterMovementController character) {
		character.SetBonusSpeed(speedAdded, duration);
		PlayerState.Instance.raceState.AddEffect(new PlayerEffect(speedIcon, duration));
	}

	public override bool IsAvailable() {
		return true; // always available
	}
}
