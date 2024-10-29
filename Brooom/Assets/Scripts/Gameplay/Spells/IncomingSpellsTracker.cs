using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IncomingSpellsTracker : MonoBehaviour {

	public List<IncomingSpellInfo> IncomingSpells { get; protected set; } = new List<IncomingSpellInfo>();

	public void AddIncomingSpell(SpellEffectController spell) {
		IncomingSpellInfo spellInfo = new IncomingSpellInfo(spell, transform);
		IncomingSpells.Add(spellInfo);
		spell.onSpellCastFinished += RemoveIncomingSpell;
		OnIncomingSpellAdded(spellInfo);
			
	}

	public void RemoveIncomingSpell(SpellEffectController spell) {
		IncomingSpellInfo spellInfo = null;
		for (int i = IncomingSpells.Count - 1; i >= 0; i--) {
			if (IncomingSpells[i].SpellObject == spell) {
				spellInfo = IncomingSpells[i];
				IncomingSpells.RemoveAt(i);
				break;
			}
		}
		OnIncomingSpellRemoved(spellInfo);
	}

	// TODO: Add any API methods which could be used with spell casting AI

	// Find out whether a specific spell is being casted at this racer
	public bool IsSpellIncoming(string spellIdentifier) {
		foreach (var incomingSpell in IncomingSpells) {
			if (incomingSpell.SpellObject.Spell.Identifier == spellIdentifier)
				return true;
		}
		return false;
	}

	protected abstract void OnIncomingSpellAdded(IncomingSpellInfo spellInfo);
	protected abstract void OnIncomingSpellRemoved(IncomingSpellInfo spellInfo);
	protected abstract void UpdateAfterParent();

	private void Update() {
		// Update state (direction and distance) of all incoming spells
		foreach (var incomingSpell in IncomingSpells) {
			incomingSpell.UpdateState();
		}
		UpdateAfterParent();
	}

}

public class IncomingSpellInfo {

	public Transform TargetObject { get; private set; }
	public SpellEffectController SpellObject { get; private set; }

	public Vector3 Direction { get; private set; }
	public float Distance { get; private set; }
	public float DistanceNormalized => (Mathf.Clamp(Distance, 0f, initialDistance) / initialDistance);

	private float initialDistance;


	public IncomingSpellInfo(SpellEffectController spell, Transform target) {
		TargetObject = target;
		SpellObject = spell;
		UpdateState();
		initialDistance = Distance;
	}
	
	// Returns angle between 0 and 2*pi if inRadians is true, otherwise an angle between 0 and 360
	public float GetAngleFromDirection(bool inRadians = true) {
		float angle = Vector3.SignedAngle(Direction.WithY(0), Vector3.forward, Vector3.up);
		if (angle < 0) angle += 360;
		if (inRadians) return angle * Mathf.Deg2Rad; // convert from degrees to radians
		else return angle;
	}

	// Computes spell distance and direction
	public void UpdateState() { 
		Distance = Vector3.Distance(SpellObject.transform.position, TargetObject.position);
		Direction = TargetObject.InverseTransformPoint(SpellObject.transform.position).normalized;
	}

}
