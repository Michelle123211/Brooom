using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell creating a shield around the racer who cast it for a short time.
/// The shield blocks any incoming spells (except for self-cast spells).
/// </summary>
public class DefensioSpellEffect : RacerAffectingSpellEffect {

	[Tooltip("Prefab of a shield object which will appear around the racer casting Defensio spell.")]
	[SerializeField] SpellShield shieldPrefab;

	private SpellShield shieldInstance;

	/// <summary>
	/// Creates a shield around the racer who cast the spell.
	/// </summary>
	protected override void StartSpellEffect_Internal() {
		// Instantiate a shield around the spell target object (~ self)
		Transform shieldParent = castParameters.Target.TargetObject.transform.Find("VisualEffects");
		shieldInstance = Instantiate<SpellShield>(shieldPrefab, shieldParent);
		shieldInstance.Appear();
	}

	/// <summary>
	/// Gradually hides and then destroys the shield around the racer.
	/// </summary>
	protected override void StopSpellEffect_Internal() {
		// Start disintegrating and then destroy the shield
		shieldInstance.Disappear();
	}

}
