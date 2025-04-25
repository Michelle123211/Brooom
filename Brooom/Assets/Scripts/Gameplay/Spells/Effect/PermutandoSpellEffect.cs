using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell for switching places with an opponent.
/// The racer who cast the spell is pulled to the place where the target opponent is located, and vice versa.
/// </summary>
public class PermutandoSpellEffect : DurativeSpellEffect {

	/// <summary>
	/// Moves both racers a bit closer to their switched positions.
	/// </summary>
	/// <param name="time">Number between 0 and 1 (indicating how far we are in the total duration).</param>
	protected override void ApplySpellEffect_OneIteration(float time) {
		// TODO: Move both racers a bit closer to their final position
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
