using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


/// <summary>
/// A component displaying mana bar and spell slots with spells equipped in race as a part of HUD.
/// It uses <c>SpellSlotUI</c> for the basic spell slot functionality,
/// but adds behaviour for 
/// </summary>
public class RaceSpellsUI : MonoBehaviour {

    [Header("Spells")]
    [Tooltip("A parent object of all the spell slots.")]
    [SerializeField] Transform spellSlotsParent;
    [Tooltip("A prefab of a spell slot which is instantiated several times.")]
    [SerializeField] RaceSpellSlotUI spellSlotPrefab;
    [Tooltip("An image used as a border highlighting the selected spell.")]
    [SerializeField] Transform highlightBorder;

    [Header("Mana")]
    [Tooltip("Slider displaying the current mana amount.")]
    [SerializeField] Slider manaBar;
    [Tooltip("TextMesh Pro displaying the current mana amount.")]
    [SerializeField] TextMeshProUGUI manaText;


    private RaceSpellSlotUI[] spellSlots;
    private int highlightedSpell = -1;

    private SpellController playerSpellController;

    /// <summary>
    /// Initializes all UI elements based on the player's <c>SpellController</c>, i.e. initializes spell slots and mana bar.
    /// Also registers necessary callbacks.
    /// The UI is displayed only if the player has at least one equipped spell.
    /// </summary>
    /// <param name="playerObject">The player's object from which <c>SpellController</c> component can be obtained.</param>
    public void Initialize(GameObject playerObject) {
        gameObject.SetActive(false);
        this.playerSpellController = playerObject.GetComponent<SpellController>();
        // Show only if the player has some spells equipped
        if (playerSpellController.HasEquippedSpells()) {
            gameObject.SetActive(true);
            StartCoroutine(InitializeSpellSlots()); // started as a coroutine so that currently selected spell can be highlighted in the next frame, after the layout is rebuilt correctly
            InitializeMana();
            // Register callbacks
            playerSpellController.onManaAmountChanged += UpdateManaAmount;
            playerSpellController.onSelectedSpellChanged += HighlightSelectedSpell;
        }
    }

    /// <summary>
    /// Updates mana amount displayed in the mana bar to the given value using a tween.
    /// </summary>
    /// <param name="amount">Current mana amount.</param>
    public void UpdateManaAmount(int amount) {
        manaBar.DOKill();
        manaBar.DOValue(amount, 0.75f, true);
    }
    /// <summary>
    /// Updates mana amount displayed as a text to the given value.
    /// </summary>
    /// <param name="amount">Mana amount to display.</param>
    public void UpdateManaAmountText(float amount) {
        manaText.text = amount.ToString();
    }

    // Instantiates spell slots and initializes them with the player's equipped spells
	private IEnumerator InitializeSpellSlots() {
        // Remove all existing slots
        UtilsMonoBehaviour.RemoveAllChildren(spellSlotsParent);
        // Instantiate new slots
        spellSlots = new RaceSpellSlotUI[playerSpellController.spellSlots.Length];
        for (int i = 0; i < spellSlots.Length; i++) {
            spellSlots[i] = Instantiate<RaceSpellSlotUI>(spellSlotPrefab, spellSlotsParent);
            spellSlots[i].Initialize(playerSpellController.spellSlots[i]);
        }
        // Wait 1 frame before highlighting the selected slot (so the LayoutGroup is rebuild)
        yield return null;
        HighlightSelectedSpell(playerSpellController.selectedSpell);
    }

    // Initializes mana bar and mana label to show current values
    private void InitializeMana() {
        manaBar.maxValue = playerSpellController.MaxMana;
        manaBar.value = playerSpellController.CurrentMana;
        manaText.text = manaBar.value.ToString();
    }

    // Highlights the selected spell slot
    private void HighlightSelectedSpell(int selectedSpell) {
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

    // TODO: Tween the scale of the selected spell slot (pulse)

	private void Update() {
        // Update mana label based on value in mana bar
        UpdateManaAmountText(manaBar.value);
    }

	private void OnDestroy() {
        // Unregister callbacks
        if (playerSpellController != null && playerSpellController.HasEquippedSpells()) {
            playerSpellController.onManaAmountChanged -= UpdateManaAmount;
            playerSpellController.onSelectedSpellChanged -= HighlightSelectedSpell;
        }
    }
}
