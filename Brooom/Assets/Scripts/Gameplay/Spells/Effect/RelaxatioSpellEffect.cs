using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell effect cancelling all negative effects currently affecting the racer.
/// </summary>
public class RelaxatioSpellEffect : OneShotSpellEffect {

	/// <summary>
	/// Resets all negative effects in the <c>EffectibleCharacter</c> component of the spell source object (i.e. self).
	/// </summary>
	/// <exception cref="System.NotSupportedException">Throws <c>NotSupportedException</c> when the source object (the racer themselves) doesn't have <c>EffectibleCharacter</c> component.</exception>
	protected override void ApplySpellEffect_Internal() {
		// Reset all negative effects currently affecting the racer
		EffectibleCharacter racer = castParameters.SourceObject.GetComponent<EffectibleCharacter>();
		if (racer == null)
			throw new System.NotSupportedException($"{nameof(RelaxatioSpellEffect)} may be used only on racer with {nameof(EffectibleCharacter)} component.");
		racer.ResetAllNegativeEffects();
	}

}
