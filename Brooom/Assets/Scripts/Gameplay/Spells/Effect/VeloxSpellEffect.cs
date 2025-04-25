using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell temporarily speeding up the racer who cast it.
/// </summary>
public class VeloxSpellEffect : RacerAffectingSpellEffect {

	[Tooltip("How much speed is added on top of the normal speed.")]
	public float speedAdded = 5;

	/// <summary>
	/// Temporarily adds bonus speed to the racer who cast the spell.
	/// </summary>
	/// <exception cref="System.NotSupportedException">Throws <c>NotSupportedException</c> when the racer doesn't have a <c>CharacterMovementController</c> component.</exception>
	protected override void StartSpellEffect_Internal() {
		CharacterMovementController targetRacer = castParameters.Target.TargetObject.GetComponent<CharacterMovementController>();
		if (targetRacer == null)
			throw new System.NotSupportedException($"{nameof(VeloxSpellEffect)} may be used only on opponents with {nameof(CharacterMovementController)} component.");
		targetRacer.SetBonusSpeed(speedAdded, effectDuration);
	}

	/// <inheritdoc/>
	protected override void StopSpellEffect_Internal() {
	}
}
