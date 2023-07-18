using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSpellSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("An object containing all UI elements for the spell price. Will be hidden if the spell has been already purchased.")]
    [SerializeField] GameObject priceParent;
    [Tooltip("A label used to display price of the spell.")]
    [SerializeField] TextMeshProUGUI priceText;
    [Tooltip("An overlay object used to change visuals when the spell has not been purchased yet.")]
    [SerializeField] GameObject unavailableOverlay;

    private SpellSlotUI spellSlotUI;

    private Spell assignedSpell;

    public void Initialize(Spell spell) {
        assignedSpell = spell;
        // Set icon
        spellSlotUI?.Initialize(spell);
        UpdateUI();
    }

    public void PurchaseSpell() {
        // Check if the player has enough coins
        if (!PlayerState.Instance.ChangeCoinsAmount(-assignedSpell.coinsCost)) return;
        assignedSpell.isUnlocked = true;
        // Update the UI
        ChangeToUnlockedd();
        // TODO: If it is the first purchased spell, start a tutorial
    }

	public void UpdateUI() {
        if (assignedSpell.isUnlocked) {
            ChangeToUnlockedd();
        } else {
            ChangeToLocked();
        }
    }

	private void ChangeToUnlockedd() {
        // Hide price + overlay
        priceParent.SetActive(false);
        unavailableOverlay.SetActive(false);
    }

    private void ChangeToLocked() {
        // Show price + overlay
        priceText.text = assignedSpell.coinsCost.ToString();
        if (PlayerState.Instance.coins < assignedSpell.coinsCost) // change to red if not enough coins
            priceText.color = Color.red; // TODO: Change to using a different color (not so aggressive, e.g. from a color palette)
        unavailableOverlay.SetActive(true);
    }

    private void Awake() {
        spellSlotUI = GetComponentInChildren<SpellSlotUI>();
    }
}
