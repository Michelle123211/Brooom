using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpellController : MonoBehaviour {

	// Mana
	public int CurrentMana { get; private set; }
	public int MaxMana { get; private set; }

	// Spells
	public SpellInRace[] spellSlots { get; private set; }
	public int selectedSpell { get; private set; }

	// Callbacks
	public event Action<int> onManaAmountChanged; // parameter: new mana value
	public event Action<int> onSelectedSpellChanged; // parameter: index of the currently selected spell
	public event Action<int> onSpellCasted; // parameter: index of the spell

	[Tooltip("Component derived from SpellTargetSelection which is responsible for selecting a target for currently seelcted spell.")]
	[SerializeField] SpellTargetSelection spellTargetSelection;

	private bool isPlayer = false;

	// direction ... Decreasing = lower index, Increasing = higher index
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
		if (selectedSpell != previouslySelectedSpell) onSelectedSpellChanged?.Invoke(selectedSpell);
		return selectedSpell;
	}

	public Spell GetCurrentlySelectedSpell() {
		if (selectedSpell != -1) {
			if (spellSlots[selectedSpell] != null) return spellSlots[selectedSpell].Spell;
		}
		return null;
	}

	public bool IsCurrentlySelectedSpellReady() {
		if (selectedSpell == -1) return false;
		return IsSpellInSlotReady(selectedSpell);
	}

	public bool IsSpellInSlotReady(int spellSlotIndex) {
		if (spellSlotIndex < 0 || spellSlotIndex > spellSlots.Length - 1) throw new IndexOutOfRangeException();
		SpellInRace spell = spellSlots[spellSlotIndex];
		if (spell == null) return false;
		return spell.IsSpellAvailable(CurrentMana);
	}

	public void CastCurrentlySelectedSpell() {
		if (IsCurrentlySelectedSpellReady()) { // spell is selected, it is charged and there is enough mana
			SpellInRace currentSpell = spellSlots[selectedSpell];
			// Cast spell and pass correct parameters (source, target)
			SpellTarget spellTarget = spellTargetSelection.GetCurrentTarget();
			if (!spellTarget.HasTargetAssigned) {
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
			AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.Events.Game.SpellCast, gameObject);
			ChangeManaAmount(-currentSpell.Spell.ManaCost);
			// Notify anyone interested that a spell has been casted
			onSpellCasted?.Invoke(selectedSpell);
			if (isPlayer) {
				Messaging.SendMessage("SpellCasted", currentSpell.Spell.Identifier);
				PlayerState.Instance.MarkSpellAsUsed(currentSpell.Spell.Identifier);
			}
		}
	}

	public bool HasEquippedSpells() {
		if (spellSlots == null) return false;
		foreach (var spell in spellSlots) {
			if (spell != null && spell.Spell != null && !string.IsNullOrEmpty(spell.Spell.Identifier)) return true;
		}
		return false;
	}

	// Equips racer with a random set of spells selected from those which are already available for player + one more
	public void RandomizeEquippedSpells() {
		if (PlayerState.Instance.availableSpellCount == 0) return;
		// Create a list of spells to choose from
		List<Spell> spellsToChooseFrom = GetSpellsForRandomization();
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

	public void ChangeManaAmount(int delta) {
		CurrentMana = Mathf.Clamp(CurrentMana + delta, 0, MaxMana);
		onManaAmountChanged?.Invoke(CurrentMana);
	}

	public void RechargeAllSpells() {
		foreach (var spell in spellSlots) {
			if (spell != null) spell.Recharge();
		}
	}

	private List<Spell> GetSpellsForRandomization() {
		// Get all spells already unlocked for player + deterministically add one more spell which has not been unlocked yet
		List<Spell> selectedSpells = new List<Spell>();
		Spell interestingUnavailableSpell = null;
		int interestingUnavailableSpellPrice = int.MaxValue;
		foreach (var spell in SpellManager.Instance.AllSpells) {
			if (PlayerState.Instance.IsSpellPurchased(spell.Identifier))
				// Add available spell to the list
				selectedSpells.Add(spell);
			else {
				// Find the cheapest interesting unavailable spell (interesting ~ from a specific category, not self-cast)
				if (interestingUnavailableSpell == null && (
					spell.Category == SpellCategory.OpponentCurse ||
						spell.Category == SpellCategory.EnvironmentManipulation ||
						spell.Category == SpellCategory.ObjectApparition) &&
					spell.CoinsCost < interestingUnavailableSpellPrice) {
					interestingUnavailableSpellPrice = spell.CoinsCost;
					interestingUnavailableSpell = spell;
				}
			}
		}
		if (interestingUnavailableSpell != null) selectedSpells.Add(interestingUnavailableSpell);
		return selectedSpells;
	}

	private void Update() {
		// Update spells charge and availability
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

public struct SpellCastParameters {

	public Spell Spell { get; set; }

	private SpellCastPoint castPoint;
	private GameObject sourceObject;
	public GameObject SourceObject {
		get => sourceObject;
		set {
			sourceObject = value;
			castPoint = sourceObject.GetComponentInChildren<SpellCastPoint>();
		}
	}

	public SpellTarget Target { get; set; }

	public Vector3 castDirection; // direction in which the spell is casted

	public Vector3 GetCastPosition() {
		if (castPoint != null) return castPoint.GetAbsolutePosition();
		else return sourceObject.transform.position;
	}

	public Vector3 GetTargetPosition() {
		return Target.GetTargetPosition();
	}

}

public struct SpellTarget {

	private SpellTargetPoint targetPoint;
	private GameObject targetObject;
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
	public Vector3 TargetPosition {
		get => targetPosition;
		set {
			targetPosition = value;
			targetObject = null;
			HasTargetAssigned = true;
		}
	}

	public bool HasTargetAssigned { get; private set; }

	public Vector3 GetTargetPosition() {
		if (targetObject != null) {
			if (targetPoint != null) return targetPoint.GetAbsolutePosition();
			else return targetObject.transform.position;
		} else return TargetPosition;
	}

}
