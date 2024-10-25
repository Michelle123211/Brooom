using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensioSpellEffect : RacerAffectingSpellEffect {

	[Tooltip("Prefab of a shield object which will appear around the racer casting Defensio spell.")]
	[SerializeField] SpellShield shieldPrefab;

	private SpellShield shieldInstance;

	protected override void StartSpellEffect_Internal() {
		// Instantiate a shield around the spell target object (~ self)
		Transform shieldParent = castParameters.Target.TargetObject.transform.Find("VisualEffects");
		shieldInstance = Instantiate<SpellShield>(shieldPrefab, shieldParent);
		shieldInstance.Appear();
	}

	protected override void StopSpellEffect_Internal() {
		// Start disintegrating and then destroy the shield
		shieldInstance.Disappear();
	}

}
