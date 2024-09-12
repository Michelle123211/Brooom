using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RaceSpellsUI : MonoBehaviour {

    [Header("Spells")]
    [Tooltip("A parent object of all the spell slots.")]
    [SerializeField] Transform spellSlotsParent;
    [Tooltip("A prefab of a spell slot which is instantiated several times.")]
    [SerializeField] RaceSpellSlotUI spellSlotPrefab;
    [Tooltip("An image used as a border highlighting the selected spell.")]
    [SerializeField] Transform highlightBorder;

    [Header("Mana")]
    [SerializeField] Slider manaBar;
    [SerializeField] TextMeshProUGUI manaText;


    private RaceSpellSlotUI[] spellSlots;
    private int highlightedSpell = -1;

    private SpellController playerSpellController;

    public void Initialize(GameObject playerObject) {
        this.playerSpellController = playerObject.GetComponent<SpellController>();
        // Show only if the player has some spells equipped
        if (playerSpellController.HasEquippedSpells()) {
            gameObject.SetActive(true);
            StartCoroutine(InitializeSpellSlots());
            InitializeMana();
            // Register callbacks
            playerSpellController.onManaAmountChanged += UpdateManaAmount;
            playerSpellController.onSelectedSpellChanged += HighlightSelectedSpell;
        }
    }

    public void UpdateManaAmount(int amount) {
        manaBar.DOKill();
        manaBar.DOValue(amount, 0.75f, true);
    }
    public void UpdateManaAmountText(float amount) {
        manaText.text = amount.ToString();
    }

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

    private void InitializeMana() {
        manaBar.maxValue = playerSpellController.MaxMana;
        manaBar.value = playerSpellController.CurrentMana;
        manaText.text = manaBar.value.ToString();
    }

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
        // Update mana bar
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
