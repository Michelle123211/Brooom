using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExperimentSpellEffect : MonoBehaviour {

	public abstract void ApplyEffect(GameObject target);

	public static void ApplyEffectStatic(GameObject target) {
		Debug.Log("Static effect applied.");
	}

}
