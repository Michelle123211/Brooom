using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for cancelling all negative effects currently affecting the racer
public class RelaxatioSpellEffect : OneShotSpellEffect {

	protected override void ApplySpellEffect_Internal() {
		// Reset all negative effects currently affecting the racer
		EffectibleCharacter racer = spellTarget.source.GetComponent<EffectibleCharacter>();
		if (racer == null)
			throw new System.NotSupportedException($"{nameof(RelaxatioSpellEffect)} may be used only on racer with {nameof(EffectibleCharacter)} component.");
		racer.ResetAllNegativeEffects();
	}

}
