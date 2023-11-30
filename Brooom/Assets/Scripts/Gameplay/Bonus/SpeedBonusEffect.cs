using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Speed the player up for a short time
public class SpeedBonusEffect : BonusEffect
{
	[Tooltip("How much speed is added on top of the normal speed.")]
	public float speedAdded = 5;
	[Tooltip("Duration of the effect in seconds.")]
	public float duration = 4;
	[Tooltip("An icon which will be used in the UI of effects affecting the player.")]
	[SerializeField] Sprite speedIcon;

	public override void ApplyBonusEffect(CharacterMovementController character) {
		character.SetBonusSpeed(speedAdded, duration);
		character.GetComponent<EffectibleCharacter>()?.AddEffect(new CharacterEffect(speedIcon, duration));
	}

	public override bool IsAvailable() {
		return true; // always available
	}
}
