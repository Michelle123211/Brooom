using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSpellSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("An object containing all UI elements for the spell price. Its children will be hidden if the spell has been already purchased.")]
    [SerializeField] Transform priceParent;
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
        // Check the spell is not already purchased
        if (PlayerState.Instance.IsSpellPurchased(assignedSpell.Identifier)) return;
        // Check if the player has enough coins
        if (!PlayerState.Instance.ChangeCoinsAmount(-assignedSpell.CoinsCost)) {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PurchaseDenied);
            return;
        }
        PlayerState.Instance.UnlockSpell(assignedSpell.Identifier);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.Purchase);
        // Update the UI
        ChangeToUnlocked();
    }

	public void UpdateUI() {
        if (PlayerState.Instance.IsSpellPurchased(assignedSpell.Identifier)) { 
            ChangeToUnlocked();
        } else {
            ChangeToLocked();
        }
    }

	private void ChangeToUnlocked() {
        // Hide price + overlay
        ShowOrHidePrice(false);
        unavailableOverlay.SetActive(false);
        GetComponent<Button>().interactable = false;
    }

    private void ChangeToLocked() {
        // Show price + overlay
        priceText.text = assignedSpell.CoinsCost.ToString();
        if (PlayerState.Instance.Coins < assignedSpell.CoinsCost) // change to red if not enough coins
            priceText.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
        ShowOrHidePrice(true);
        unavailableOverlay.SetActive(true);
        GetComponent<Button>().interactable = true;
    }

    private void ShowOrHidePrice(bool show = true) {
        for (int i = 0; i < priceParent.transform.childCount; i++) {
            priceParent.GetChild(i).gameObject.SetActive(show);
        }
    }

    private void Awake() {
        spellSlotUI = GetComponentInChildren<SpellSlotUI>();
    }
}
