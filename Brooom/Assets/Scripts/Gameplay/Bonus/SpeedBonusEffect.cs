using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A bonus which speeds the racer up for a short period of time.
/// </summary>
public class SpeedBonusEffect : BonusEffect {

	[Tooltip("How much speed is added on top of the normal speed.")]
	public float speedAdded = 5;
	[Tooltip("Duration of the effect in seconds.")]
	public float duration = 4;

	[Tooltip("An icon which will be used in the UI of effects affecting the player.")]
	[SerializeField] Sprite speedIcon;
	[Tooltip("A visual effect which is displayed around the racer while they are affected by the bonus.")]
	[SerializeField] private SelfDestructiveVisualEffect bonusInfluenceVisualEffectPrefab;

	/// <inheritdoc/>
	public override void ApplyBonusEffect(CharacterMovementController character) {
		character.SetBonusSpeed(speedAdded, duration);
		character.GetComponent<EffectibleCharacter>()?.AddEffect(new CharacterEffect(speedIcon, duration, true), bonusInfluenceVisualEffectPrefab);
	}

	/// <inheritdoc/>
	public override bool IsAvailable() {
		return true; // always available
	}
}
