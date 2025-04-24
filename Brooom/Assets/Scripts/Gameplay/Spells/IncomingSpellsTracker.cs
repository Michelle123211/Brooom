using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class managing a list of spells which are being cast at the racer.
/// </summary>
public abstract class IncomingSpellsTracker : MonoBehaviour {

	public List<IncomingSpellInfo> IncomingSpells { get; protected set; } = new List<IncomingSpellInfo>();
	public event Action<IncomingSpellInfo> onIncomingSpellAdded;

	/// <summary>
	/// Adds a new incoming spell into a list of all currently incoming spells.
	/// </summary>
	/// <param name="spell">Incoming spell to be added.</param>
	public void AddIncomingSpell(SpellEffectController spell) {
		IncomingSpellInfo spellInfo = new IncomingSpellInfo(spell, transform);
		IncomingSpells.Add(spellInfo);
		spell.onSpellCastFinished += RemoveIncomingSpell;
		OnIncomingSpellAdded(spellInfo);
		onIncomingSpellAdded?.Invoke(spellInfo);
	}

	/// <summary>
	/// Removes an incoming spell from a list of all currently incoming spells (e.g. because it hit the the target racer).
	/// </summary>
	/// <param name="spell">Incoming spell to be removed.</param>
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

	/// <summary>
	/// Finds out whether a specific spell is being cast at the racer.
	/// </summary>
	/// <param name="spellIdentifier">Identifier of the spell to look for.</param>
	/// <returns><c>true</c> if the given spell is already being cast at the racer, <c>false</c> otherwise.</returns>
	public bool IsSpellIncoming(string spellIdentifier) {
		foreach (var incomingSpell in IncomingSpells) {
			if (incomingSpell.SpellObject.Spell.Identifier == spellIdentifier)
				return true;
		}
		return false;
	}

	/// <summary>
	/// Called when a new incoming spell has been added into a list of all currently incoming spells.
	/// </summary>
	/// <param name="spellInfo">Infomration about the incoming spell to be added.</param>
	protected abstract void OnIncomingSpellAdded(IncomingSpellInfo spellInfo);
	/// <summary>
	/// Called when an incoming spell has been removed from a list of all currently incoming spells.
	/// </summary>
	/// <param name="spellInfo">Information about the incoming spell which has been removed.</param>
	protected abstract void OnIncomingSpellRemoved(IncomingSpellInfo spellInfo);
	/// <summary>
	/// This method replaces <c>Update()</c> method in derived classes (in fact, it is called from the inherited <c>Update()</c> after the parent class has performed its code).
	/// </summary>
	protected abstract void UpdateAfterParent();

	private void Update() {
		// Update state (direction and distance) of all incoming spells
		foreach (var incomingSpell in IncomingSpells) {
			incomingSpell.UpdateState();
		}
		UpdateAfterParent();
	}

}

/// <summary>
/// Information about a spell being cast at a racer, including e.g., the spell object, target object, distance from the target.
/// </summary>
public class IncomingSpellInfo {

	/// <summary>Target object of this spell.</summary>
	public Transform TargetObject { get; private set; }
	/// <summary>An object representing the spell being cast at the target object.</summary>
	public SpellEffectController SpellObject { get; private set; }

	/// <summary>Current direction from which the spell is coming to the target object.</summary>
	public Vector3 Direction { get; private set; }
	/// <summary>Current distance of the spell from the target object.</summary>
	public float Distance { get; private set; }
	/// <summary> Current distance of the spell from the target object normalized (mapped from (0, initial distance) to (0, 1)).</summary>
	public float DistanceNormalized => (Mathf.Clamp(Distance, 0f, initialDistance) / initialDistance);

	private float initialDistance;


	public IncomingSpellInfo(SpellEffectController spell, Transform target) {
		TargetObject = target;
		SpellObject = spell;
		UpdateState();
		initialDistance = Distance;
	}
	
	/// <summary>
	/// Computes angle at which the incoming spell indicator is coming to the target object (in the XZ plane, relatively to the target's forward vector).
	/// </summary>
	/// <param name="inRadians">Whether the angle should be computed in radians (otherwise in degrees).</param>
	/// <returns>Angle between 0 and 2*pi, if <c>inRadians</c> is <c>true</c>, otherwise an angle between 0 and 360.</returns>
	public float GetAngleFromDirection(bool inRadians = true) {
		float angle = Vector3.SignedAngle(Direction.WithY(0), Vector3.forward, Vector3.up);
		if (angle < 0) angle += 360;
		if (inRadians) return angle * Mathf.Deg2Rad; // convert from degrees to radians
		else return angle;
	}

	/// <summary>
	/// Updates spell's current distance and direction.
	/// </summary>
	public void UpdateState() { 
		Distance = Vector3.Distance(SpellObject.transform.position, TargetObject.position);
		Direction = TargetObject.InverseTransformPoint(SpellObject.transform.position).normalized;
	}

}
