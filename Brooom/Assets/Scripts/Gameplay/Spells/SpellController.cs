using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpellController : MonoBehaviour {

    [Tooltip("A parent object of all the spell slots.")]
    [SerializeField] Transform spellSlotsParent;
    [Tooltip("A prefab of a spell slot which is instantiated several times.")]
    [SerializeField] RaceSpellSlotUI spellSlotPrefab;
    [Tooltip("An image used as a border highlighting the selected spell.")]
    [SerializeField] Transform highlightBorder;

    private RaceSpellSlotUI[] spellSlots;
    private int selectedSpell = -1;
    private int highlightedSpell = -1;


    public void Initialize() {
        InitializeSpellSlots();
        InitializeSelectedSpell();
    }

    private void InitializeSpellSlots() {
        // Remove all existing slots
        for (int i = spellSlotsParent.childCount - 1; i >= 0; i--) {
            Destroy(spellSlotsParent.GetChild(i).gameObject);
        }
        // Instantiate new slots
        spellSlots = new RaceSpellSlotUI[PlayerState.Instance.raceState.spellSlots.Length];
        for (int i = 0; i < spellSlots.Length; i++) {
            spellSlots[i] = Instantiate<RaceSpellSlotUI>(spellSlotPrefab, spellSlotsParent);
            spellSlots[i].Initialize(PlayerState.Instance.raceState.spellSlots[i]);
        }
    }

    private void InitializeSelectedSpell() {
        // Use the first non-empty slot as the selected one
        for (int i = 0; i < spellSlots.Length; i++) {
            if (!spellSlots[i].isEmpty) {
                selectedSpell = i;
                break;
            }
        }
        // If there is no equipped spell, hide the highlight border
        highlightBorder.gameObject.SetActive(selectedSpell >= 0);
    }

    // direction ... Decreasing = lower index, Increasing = higher index
    private int ChangeSelectedSpell(IterationDirection direction) {
        // Choose the first non-empty slot in the given direction
        int increment = (int)direction;
        for (int i = selectedSpell + increment; i >= 0 && i < spellSlots.Length; i = i + increment) {
            if (!spellSlots[i].isEmpty) return i;
        }
        return selectedSpell;
    }

    // TODO: Tween the scale of the selected spell slot (pulse)

    private void Start() {
        Initialize();
    }

	private void Update() {
        // Detect mouse click and cast the currently selected spell
        if (Input.GetMouseButtonDown(0)) {
            spellSlots[selectedSpell].assignedSpell.CastSpell();
        }
        // Detect mouse wheel and change the currently selected spell
        // TODO: Try it with other HW if it just like that everywhere (or it is necessary to e.g. accumulate value over frames, change spell if over some threshold, use mouse sensitivity)
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0) {
            if (scroll > 0) selectedSpell = ChangeSelectedSpell(IterationDirection.Decreasing); // up
            else            selectedSpell = ChangeSelectedSpell(IterationDirection.Increasing); // down
        }

        // Highlight the selected slot - move the highlight border
        if (highlightedSpell != selectedSpell) { // the spell to highlight changed
            if (highlightedSpell >= 0) spellSlots[highlightedSpell].Deselect();
            highlightedSpell = selectedSpell;
            if (highlightedSpell >= 0) {
                highlightBorder.DOKill();
                highlightBorder.DOMove(spellSlots[highlightedSpell].transform.position, 0.2f);
                spellSlots[highlightedSpell].Select();
            }
        }
    }
}
