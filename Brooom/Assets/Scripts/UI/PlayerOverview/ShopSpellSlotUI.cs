using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a single spell which can be purchased from the shop's catalogue.
/// It uses <c>SpellSlotUI</c> for the basic spell slot functionality,
/// but adds behaviour for displaying the spell's price, for changing the appearance based on whether the spell has been purchased or not,
/// and for purchasing the spell by clicking on it.
/// </summary>
public class ShopSpellSlotUI : MonoBehaviour {

    [Header("UI Elements")]
    [Tooltip("An object containing all UI elements for the spell price. Its children will be hidden if the spell has been already purchased.")]
    [SerializeField] Transform priceParent;
    [Tooltip("A label used to display price of the spell.")]
    [SerializeField] TextMeshProUGUI priceText;
    [Tooltip("An overlay object used to change appearance when the spell has not been purchased yet.")]
    [SerializeField] GameObject unavailableOverlay;

    private SpellSlotUI spellSlotUI;

    private Spell assignedSpell;


    /// <summary>
    /// Initializes the shop spell slot with the given spell (i.e. sets the icon, tooltip content, price, appearance).
    /// </summary>
    /// <param name="spell"></param>
    public void Initialize(Spell spell) {
        assignedSpell = spell;
        // Set icon and tooltip content
        spellSlotUI?.Initialize(spell);
        UpdateUI();
    }

    /// <summary>
    /// Checks if the player has enough coins to purchase the spel, and if so, purchases it and unlocks it.
    /// Called when the shop spell slot is clicked.
    /// </summary>
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
        Analytics.Instance.LogEvent(AnalyticsCategory.Shop, $"Spell {assignedSpell.SpellName} purchased.");
        // Update the UI
        ChangeToUnlocked();
    }

    /// <summary>
    /// Updates the shop spell slot's UI (i.e. price, appearance).
    /// </summary>
	public void UpdateUI() {
        if (PlayerState.Instance.IsSpellPurchased(assignedSpell.Identifier)) { 
            ChangeToUnlocked();
        } else {
            ChangeToLocked();
        }
    }

    // Shows the spell as unlocked (price is hidden, there is no overlay over the icon, button is not interactable)
	private void ChangeToUnlocked() {
        // Hide price + overlay
        ShowOrHidePrice(false);
        unavailableOverlay.SetActive(false);
        GetComponent<Button>().interactable = false;
    }

    // Shows the spell as locked (price is displayed with color corresponding to whether the player has enough coins or not,
    // there is an overlay over the icon, button is interactable)
    private void ChangeToLocked() {
        // Show price + overlay
        priceText.text = assignedSpell.CoinsCost.ToString();
        if (PlayerState.Instance.Coins < assignedSpell.CoinsCost) // change to red if not enough coins
            priceText.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
        ShowOrHidePrice(true);
        unavailableOverlay.SetActive(true);
        GetComponent<Button>().interactable = true;
    }

    // Shows or hides all UI elements related to the spell price
    private void ShowOrHidePrice(bool show = true) {
        for (int i = 0; i < priceParent.transform.childCount; i++) {
            priceParent.GetChild(i).gameObject.SetActive(show);
        }
    }

    private void Awake() {
        spellSlotUI = GetComponentInChildren<SpellSlotUI>();
    }
}
