using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for confusing an opponent
public class ConfusioneSpellEffect : RacerAffectingSpellEffect
{
	protected override void StartSpellEffect_Internal() {
		// TODO: Disable the racer's controls
		throw new System.NotImplementedException();
	}

	protected override void StopSpellEffect_Internal() {
		// TODO: Enable the racer's controls again
		throw new System.NotImplementedException();
	}
}
