using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentSpellEffectTest : ExperimentSpellEffect {

	public int effectParameter;

	public override void ApplyEffect(GameObject target) {
		Debug.Log("Effect applied.");
	}
}
