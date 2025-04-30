using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a single broom upgrade in a shop.
/// It displays its localized name, current level, price of the next level, and also a button to purchase the upgrade.
/// On mouse hover, a tooltip is displayed with additional information.
/// </summary>
public class BroomUpgradeRowUI : MonoBehaviour {

    [Header("UI elements")]
    [Tooltip("A label displaying the broom upgrade's name.")]
    [SerializeField] TextMeshProUGUI nameText;
    [Tooltip("A parent object of all the level indicators.")]
    [SerializeField] Transform levelIndicatorParent;
    [Tooltip("An object containing all UI elements relevant to buying the upgrade (price, button). Will be hidden when all upgrade levels were purchased.")]
    [SerializeField] GameObject buyObject;
    [Tooltip("A button used to purchase the upgrade.")]
    [SerializeField] Button buyButton;
    [Tooltip("A label for displaying the cost.")]
    [SerializeField] TextMeshProUGUI priceText;

    [Header("Components")]
    [Tooltip("The tooltip component displaying additional information about the broom upgrade.")]
    [SerializeField] SimpleTooltip tooltip;

    [Header("Prefabs")]
    [Tooltip("A prefab of a level indicator which is instantiated multiple times.")]
    [SerializeField] Image levelIndicatorPrefab;

    private BroomUpgrade assignedUpgrade;

    private Image[] levelIndicators;


    /// <summary>
    /// Initializes the broom upgrade's row in a shop with the given broom upgrade (i.e. sets name, level, price, tooltip content).
    /// </summary>
    /// <param name="broomUpgrade">Broom upgrade to be assigned.</param>
    public void Initialize(BroomUpgrade broomUpgrade) {
        this.assignedUpgrade = broomUpgrade;
        // Update UI
        // ...name
        nameText.text = LocalizationManager.Instance.GetLocalizedString("BroomUpgrade" + broomUpgrade.UpgradeName);
        // ...level, price and buy button
        InitializeCurrentLevel();
        // ...tooltip
        tooltip.text = "BroomUpgradeTooltip" + assignedUpgrade.UpgradeName;
    }

    /// <summary>
    /// Check if the player has enough coins to purchase the upgrade, and if so, purchases it and levels it up.
    /// Called when the button for purchasing the broom upgrade is pressed.
    /// </summary>
    public void PurchaseUpgrade() {
        // Check if the player has enough coins
        if (!PlayerState.Instance.ChangeCoinsAmount(-assignedUpgrade.CoinsCostOfEachLevel[assignedUpgrade.CurrentLevel])) {
            Debug.Log("Not enough coins for the given broom upgrade.");
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PurchaseDenied);
            return;
        }
        // Level the upgrade up
		assignedUpgrade.LevelUp();
        PlayerState.Instance.SetBroomUpgradeLevel(assignedUpgrade.UpgradeName, assignedUpgrade.CurrentLevel, assignedUpgrade.MaxLevel);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.Purchase);
        Analytics.Instance.LogEvent(AnalyticsCategory.Shop, $"Broom upgrade {assignedUpgrade.UpgradeName} leveled up to {assignedUpgrade.CurrentLevel}/{assignedUpgrade.MaxLevel}.");
        // Update the UI
        UpdateCurrentLevel();
    }

    /// <summary>
    /// Updates the broom upgrade's UI (i.e. current level indicator, price and buy button)
    /// </summary>
    public void UpdateUI() {
        UpdateCurrentLevel();
	}

    // Instantiates level indicators based on the maximum level and current level
	private void InitializeCurrentLevel() {
        // Remove all previous indicators
        UtilsMonoBehaviour.RemoveAllChildren(levelIndicatorParent);
        // Instantiate new ones
        levelIndicators = new Image[assignedUpgrade.MaxLevel];
        for (int i = 0; i < assignedUpgrade.MaxLevel; i++) {
            levelIndicators[i] = Instantiate<Image>(levelIndicatorPrefab, levelIndicatorParent);
        }
        // Change color according to the current level
        UpdateCurrentLevel();
    }

    // Sets price and color of level indicators according to the current level, hides the price and the buy button if the upgrade is already at maximum level
    private void UpdateCurrentLevel() {
        // Level indicator
        for (int i = 0; i < levelIndicators.Length; i++) {
            // Set color according to the level
            if (i < assignedUpgrade.CurrentLevel) { // level purchased
                Color lightColor = ColorPalette.Instance.GetColor(ColorFromPalette.Shop_BroomUpgradeLevelActiveLight);
                Color darkColor = ColorPalette.Instance.GetColor(ColorFromPalette.Shop_BroomUpgradeLevelActiveDark);
                float blend = (float)i / (assignedUpgrade.MaxLevel - 1);
                levelIndicators[i].color = blend * darkColor + (1 - blend) * lightColor;
            } else { // level not purchased
                levelIndicators[i].color = ColorPalette.Instance.GetColor(ColorFromPalette.Shop_BroomUpgradeLevelInactive);
            }
        }
        // Price and buy button
        if (assignedUpgrade.CurrentLevel == assignedUpgrade.MaxLevel) { // max level reached
            buyObject.SetActive(false);
        } else {
            int price = assignedUpgrade.CoinsCostOfEachLevel[assignedUpgrade.CurrentLevel];
            priceText.text = price.ToString();
            if (PlayerState.Instance.Coins < price) { // change to red if not enough coins
                buyButton.interactable = false;
                priceText.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
            }
        }
    }

}
