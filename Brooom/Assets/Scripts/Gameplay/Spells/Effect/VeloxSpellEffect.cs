using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for speeding up temporarily
public class VeloxSpellEffect : RacerAffectingSpellEffect {

	[Tooltip("How much speed is added on top of the normal speed.")]
	public float speedAdded = 5;

	protected override void StartSpellEffect_Internal() {
		CharacterMovementController targetRacer = castParameters.Target.TargetObject.GetComponent<CharacterMovementController>();
		if (targetRacer == null)
			throw new System.NotSupportedException($"{nameof(VeloxSpellEffect)} may be used only on opponents with {nameof(CharacterMovementController)} component.");
		targetRacer.SetBonusSpeed(speedAdded, effectDuration);
	}

	protected override void StopSpellEffect_Internal() {
	}
}
