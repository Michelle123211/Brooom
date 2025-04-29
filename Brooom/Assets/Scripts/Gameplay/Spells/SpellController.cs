using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// A component handling everything related to casting spells for a single racer.
/// It keeps track of current mana, spells equipped to the slots and the currently selected one.
/// It also provides methods for interacting with the spells, i.e. selecting a particular spell, and casting a currently selected spell.
/// </summary>
public class SpellController : MonoBehaviour {

	// Mana
	/// <summary>AMount of mana the racer currently has.</summary>
	public int CurrentMana { get; private set; }
	/// <summary>Maximum possible amount of mana the racer could have.</summary>
	public int MaxMana { get; private set; }

	// Spells
	/// <summary>Slots with spells equipped to the race.</summary>
	public SpellInRace[] spellSlots { get; private set; }
	/// <summary>Slot index of the currently selected spell.</summary>
	public int selectedSpell { get; private set; }

	// Callbacks
	/// <summary>Called when mana amount changes. Parameter is the new mana value.</summary>
	public event Action<int> onManaAmountChanged;
	/// <summary>Called when the currently selected spell changes. Parameter is slot index of the new currently selected spell.</summary>
	public event Action<int> onSelectedSpellChanged;
	/// <summary>Called when spell is cast. Parameter is slot index of the spell cast.</summary>
	public event Action<int> onSpellCast;

	[Tooltip("Component derived from SpellTargetSelection which is responsible for selecting a target for currently selected spell.")]
	[SerializeField] SpellTargetSelection spellTargetSelection;

	private bool isPlayer = false;

	/// <summary>
	/// Starting from the currently selected spell, it chooses the first non-empty spell slot in the given direction as the new currently selected spell.
	/// It is not possible to loop over the edge. If there is no spell in the given direction, the selected spell doesn't change.
	/// </summary>
	/// <param name="direction"><c>Decreasing</c> means going to lower index, <c>Increasing</c> means going to higher index.</param>
	/// <returns>Slot index of the new currently selected spell.</returns>
	public int ChangeSelectedSpell(IterationDirection direction) {
		int previouslySelectedSpell = selectedSpell;
		// Choose the first non-empty slot in the given direction
		int increment = (int)direction;
		for (int i = selectedSpell + increment; i >= 0 && i < spellSlots.Length; i = i + increment) {
			if (spellSlots[i] != null) {
				selectedSpell = i;
				break;
			}
		}
		if (selectedSpell != previouslySelectedSpell) {
			if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.SpellSwapped);
			onSelectedSpellChanged?.Invoke(selectedSpell);
		}
		return selectedSpell;
	}

	/// <summary>
	/// Gets <c>Spell</c> representation of the currently selected spell, containing all information about the spell.
	/// </summary>
	/// <returns>Currently selected spell as <c>Spell</c>, or <c>null</c> if no spell is selected.</returns>
	public Spell GetCurrentlySelectedSpell() {
		if (selectedSpell != -1) {
			if (spellSlots[selectedSpell] != null) return spellSlots[selectedSpell].Spell;
		}
		return null;
	}

	/// <summary>
	/// Checks whether the currently selected spell is ready to be used, i.e. it is not on cooldown and the racer has enough mana to cast it.
	/// </summary>
	/// <returns><c>true</c> if the spell can be used right now, <c>false</c> otherwise.</returns>
	public bool IsCurrentlySelectedSpellReady() {
		if (selectedSpell == -1) return false;
		return IsSpellInSlotReady(selectedSpell);
	}

	/// <summary>
	/// Checks whether the spell assigned to a slot with the given index is ready to be used, i.e. it is not on cooldown and the racer has enough mana to cast it.
	/// </summary>
	/// <param name="spellSlotIndex">Index of the slot to look at.</param>
	/// <returns><c>true</c> if the spell in the slot can be used right now, <c>false</c> otherwise (also if the slot is empty).</returns>
	public bool IsSpellInSlotReady(int spellSlotIndex) {
		if (spellSlotIndex < 0 || spellSlotIndex > spellSlots.Length - 1) throw new IndexOutOfRangeException();
		SpellInRace spell = spellSlots[spellSlotIndex];
		if (spell == null) return false;
		return spell.IsSpellAvailable(CurrentMana);
	}

	/// <summary>
	/// Casts a spell assigned to the currently selected slot, if it is ready to be used and there is a suitable target for it (found using <c>SpellTargetSelection</c> component).
	/// </summary>
	public void CastCurrentlySelectedSpell() {
		if (IsCurrentlySelectedSpellReady()) { // spell is selected, it is charged and there is enough mana
			SpellInRace currentSpell = spellSlots[selectedSpell];
			// Cast spell and pass correct parameters (source, target)
			SpellTarget spellTarget = spellTargetSelection.GetCurrentTarget();
			if (!spellTarget.HasTargetAssigned) {
				if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.SpellCastFailed);
				Debug.Log("No suitable spell target was found.");
				return;
			}
			currentSpell.CastSpell(
				new SpellCastParameters {
					Spell = currentSpell.Spell,
					SourceObject = gameObject,
					Target = spellTarget,
					castDirection = (spellTarget.GetTargetPosition() - transform.position).normalized
				});
			ChangeManaAmount(-currentSpell.Spell.ManaCost);
			// Notify anyone interested that a spell has been cast
			onSpellCast?.Invoke(selectedSpell);
			if (isPlayer) {
				Messaging.SendMessage("SpellCast", currentSpell.Spell.Identifier);
				PlayerState.Instance.MarkSpellAsUsed(currentSpell.Spell.Identifier);
			}
		} else {
			if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.SpellCastFailed);
		}
	}

	/// <summary>
	/// Checks whether the racer (to whom this component belongs) has any equipped spells.
	/// </summary>
	/// <returns><c>true</c> if there is at least one spell assigned to a slot, <c>false</c> otherwise.</returns>
	public bool HasEquippedSpells() {
		if (spellSlots == null) return false;
		foreach (var spell in spellSlots) {
			if (spell != null && spell.Spell != null && !string.IsNullOrEmpty(spell.Spell.Identifier)) return true;
		}
		return false;
	}

	/// <summary>
	/// Equips the racer, to whom this component belongs, with a random set of spells.
	/// The spells are selected from those which are already available to the player, but also one unavailable one (deterministically chosen).
	/// All spell slots will be filled, if possible.
	/// </summary>
	public void RandomizeEquippedSpells() {
		if (PlayerState.Instance.availableSpellCount == 0) return;
		// Create a list of spells to choose from
		GetSpellsForRandomization(out List<Spell> spellsToChooseFrom, out Spell interestingUnavailableSpell);
		// With a small probability, add interesting unavailable spell to the spells to choose from
		if (interestingUnavailableSpell != null && UnityEngine.Random.value < 0.3f) spellsToChooseFrom.Add(interestingUnavailableSpell);
		// Randomly select at most 4 different spells and equip them in a continuous block starting at index 0
		//	- According to Magic statistic only a subset of equipped spells may be used so we can equip them all
		int spellCount = Mathf.Min(spellsToChooseFrom.Count, 4);
		while (spellCount > 0) {
			int randomSpellIndex = UnityEngine.Random.Range(0, spellsToChooseFrom.Count);
			spellSlots[spellCount - 1] = new SpellInRace(spellsToChooseFrom[randomSpellIndex]);
			spellsToChooseFrom.RemoveAt(randomSpellIndex);
			spellCount--;
		}
		selectedSpell = 0;
	}

	/// <summary>
	/// Changes the amount of mana for the associated racer.
	/// </summary>
	/// <param name="delta">Amount of change, positive (to increase mana) or negative (to decrease mana).</param>
	public void ChangeManaAmount(int delta) {
		CurrentMana = Mathf.Clamp(CurrentMana + delta, 0, MaxMana);
		onManaAmountChanged?.Invoke(CurrentMana);
	}

	/// <summary>
	/// Recharges all equipped spells almost instantly.
	/// </summary>
	public void RechargeAllSpells() {
		foreach (var spell in spellSlots) {
			if (spell != null) spell.Recharge();
		}
	}

	// Returns a list of spells which are already unlocked for player and one bonus spell (i.e. deterministically chosen spell which has not been unlocked yet)
	private void GetSpellsForRandomization(out List<Spell> selectedSpells, out Spell bonusSpell) {
		// Get all spells already unlocked for player + deterministically add one more spell which has not been unlocked yet
		selectedSpells = new List<Spell>();
		bonusSpell = null;
		int interestingUnavailableSpellPrice = int.MaxValue;
		foreach (var spell in SpellManager.Instance.AllSpells) {
			if (PlayerState.Instance.IsSpellPurchased(spell.Identifier))
				// Add available spell to the list
				selectedSpells.Add(spell);
			else {
				// Find the cheapest interesting unavailable spell (interesting ~ from a specific category, not self-cast)
				if ((spell.Category == SpellCategory.EnvironmentManipulation || spell.Category == SpellCategory.ObjectApparition) 
					&& spell.CoinsCost < interestingUnavailableSpellPrice) {
					interestingUnavailableSpellPrice = spell.CoinsCost;
					bonusSpell = spell;
				}
			}
		}
	}

	private void Update() {
		// Update spells' charge and availability
		foreach (var spell in spellSlots) {
			if (spell != null) {
				spell.UpdateCharge(Time.deltaTime);
				spell.UpdateAvailability(CurrentMana);
			}
		}
	}

	private void Awake() {
		isPlayer = CompareTag("Player");
		// Initialize data fields
		MaxMana = PlayerState.Instance.MaxManaAmount;
		CurrentMana = 0;
		spellSlots = new SpellInRace[PlayerState.Instance.equippedSpells.Length];
		selectedSpell = -1;
		if (isPlayer) {
			// Initialize equipped spells from PlayerState
			for (int i = 0; i < spellSlots.Length; i++) {
				if (PlayerState.Instance.equippedSpells[i] == null || string.IsNullOrEmpty(PlayerState.Instance.equippedSpells[i].Identifier)) {
					spellSlots[i] = null;
				} else {
					spellSlots[i] = new SpellInRace(PlayerState.Instance.equippedSpells[i]);
					spellSlots[i].Reset();
					// Use the first non-empty slot as the selected one
					if (selectedSpell == -1) selectedSpell = i;
				}
			}
		}
	}
}

/// <summary>
/// A struct containing parameters for casting a spell, e.g., source object (i.e. racer casting the spell), target object, direction.
/// It also provides methods to get start position (from where the spell is cast) and target position, considering also <c>SpellCastPoint</c> and <c>SpellTargetPoint</c> components.
/// </summary>
public struct SpellCastParameters {

	/// <summary>The spell which is being cast.</summary>
	public Spell Spell { get; set; }

	private SpellCastPoint castPoint;
	private GameObject sourceObject;
	/// <summary>Object casting the spell (i.e. a racer). May contain <c>SpellCastPoint</c> component to specify spell cast position.</summary>
	public GameObject SourceObject {
		get => sourceObject;
		set {
			sourceObject = value;
			castPoint = sourceObject.GetComponentInChildren<SpellCastPoint>();
		}
	}

	/// <summary>Target of the spell (direction/position or object).</summary>
	public SpellTarget Target { get; set; }

	/// <summary>Direction in which the spell is being cast.</summary>
	public Vector3 castDirection;

	/// <summary>
	/// Gets position from which the spell is being cast, based on the source object while considering also <c>SpellCastPoint</c> component.
	/// </summary>
	/// <returns>Start position the spell is being cast from.</returns>
	public Vector3 GetCastPosition() {
		if (castPoint != null) return castPoint.GetAbsolutePosition();
		else return sourceObject.transform.position;
	}

	/// <summary>
	/// Gets the target position the spell should be cast at (either from direction or an object). If it is cast at an object, <c>SpellTargetPoint</c> component is also considered.
	/// </summary>
	/// <returns>Target position the spell should be cast at.</returns>
	public Vector3 GetTargetPosition() {
		return Target.GetTargetPosition();
	}

}

/// <summary>
/// A struct describing a target of a spell, regardless of whether it is a position or object (including self).
/// </summary>
public struct SpellTarget {

	private SpellTargetPoint targetPoint; // offset from object's origin
	private GameObject targetObject;
	/// <summary>Target object of the spell. Is <c>null</c>, if the spell is cast in a general direction.</summary>
	public GameObject TargetObject {
		get => targetObject;
		set {
			targetObject = value;
			if (value != null) {
				targetPoint = targetObject.GetComponentInChildren<SpellTargetPoint>();
				HasTargetAssigned = true;
			} else {
				HasTargetAssigned = false;
			}
		}
	}

	private Vector3 targetPosition;
	/// <summary>Target position of the spell, if it is cast in a general direction (not at an object).</summary>
	public Vector3 TargetPosition {
		get => targetPosition;
		set {
			targetPosition = value;
			targetObject = null;
			HasTargetAssigned = true;
		}
	}

	/// <summary>Whether the spell has a target assigned (either object or direction/position), or not.</summary>
	public bool HasTargetAssigned { get; private set; }

	/// <summary>
	/// Gets the target position the spell should be cast at (either from direction or an object). If it is cast at an object, <c>SpellTargetPoint</c> component is also considered.
	/// </summary>
	/// <returns>Target position the spell should be cast at.</returns>
	public Vector3 GetTargetPosition() {
		if (targetObject != null) {
			if (targetPoint != null) return targetPoint.GetAbsolutePosition();
			else return targetObject.transform.position;
		} else return TargetPosition;
	}

}


/// <summary>
/// Possible directions of iteration over some elements.
/// </summary>
public enum IterationDirection {
	Decreasing = -1,
	Increasing = 1
}
