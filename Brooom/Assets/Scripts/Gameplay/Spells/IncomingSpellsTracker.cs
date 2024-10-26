using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IncomingSpellsTracker : MonoBehaviour {

	public List<SpellEffectController> IncomingSpells { get; protected set; } = new List<SpellEffectController>();

	public void AddIncomingSpell(SpellEffectController spell) {
		IncomingSpells.Add(spell);
		spell.onSpellCastFinished += RemoveIncomingSpell;
		OnIncomingSpellAdded(spell);
			
	}

	public void RemoveIncomingSpell(SpellEffectController spell) {
		IncomingSpells.Remove(spell);
		OnIncomingSpellRemoved(spell);
	}

	// Returns angle between 0 and 2*pi if inRadians is true, otherwise an angle between 0 and 360
	public float GetAngleFromDirection(Vector3 direction, bool inRadians = true) {
		float angle = Vector3.SignedAngle(direction.WithY(0), Vector3.forward, Vector3.up);
		if (angle < 0) angle += 360;
		if (inRadians) return angle * Mathf.Deg2Rad; // convert from degrees to radians
		else return angle;
	}

	protected abstract void OnIncomingSpellAdded(SpellEffectController spell);
	protected abstract void OnIncomingSpellRemoved(SpellEffectController spell);

}
