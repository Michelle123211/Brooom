using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell pulling target hoop on front of the racer who cast it.
/// </summary>
public class MovereCirculisSpellEffect : DurativeSpellEffect {

	/// <summary>
	/// Moves the target hoop a bit closer in front of the racer casting the spell.
	/// </summary>
	/// <param name="time">Number between 0 and 1 (indicating how far we are in the total duration).</param>
	protected override void ApplySpellEffect_OneIteration(float time) {
		// TODO: Move the hoop a bit closer to the position in front of the racer
		// TODO: Update the current hoop position in the level/track description
		throw new System.NotImplementedException();
	}

	/// <inheritdoc/>
	protected override void FinishApplyingSpellEffect() {
		throw new System.NotImplementedException();
	}

	/// <inheritdoc/>
	protected override void StartApplyingSpellEffect() {
		throw new System.NotImplementedException();
	}

}
