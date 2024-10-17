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

	protected abstract void OnIncomingSpellAdded(SpellEffectController spell);
	protected abstract void OnIncomingSpellRemoved(SpellEffectController spell);

}
