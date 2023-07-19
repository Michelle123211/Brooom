using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BroomUpgradeRowUI : MonoBehaviour
{
    [Header("UI elements")]
    [Tooltip("A label displaying the broom upgrade's name.")]
    [SerializeField] TextMeshProUGUI nameText;
    [Tooltip("A parent object of all the level indicators.")]
    [SerializeField] Transform levelIndicatorParent;
    [Tooltip("An object containing all UI elements relevant to buying the upgrade (price, button). Will be hidden when all upgrade level were purchased.")]
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

    [Header("Colors")]
    [SerializeField] Gradient levelIndicatorGradient;
    [SerializeField] Color levelNotPurchasedColor;

    private BroomUpgrade assignedUpgrade;

    private Image[] levelIndicators;

    public void Initialize(BroomUpgrade broomUpgrade) {
        this.assignedUpgrade = broomUpgrade;
        // Update UI
        // ...name
        nameText.text = LocalizationManager.Instance.GetLocalizedString("BroomUpgrade" + broomUpgrade.Name);
        // ...level, price and buy button
        InitializeCurrentLevel();
        // ...tooltip
        tooltip.text = "BroomUpgradeTooltip" + assignedUpgrade.Name;
    }

    public void PurchaseUpgrade() {
        // Check if the player has enough coins
        if (!PlayerState.Instance.ChangeCoinsAmount(-assignedUpgrade.CoinsCostOfEachLevel[assignedUpgrade.CurrentLevel])) return;
        assignedUpgrade.LevelUp();
        // Update the UI
        UpdateCurrentLevel();
    }

    public void UpdateUI() {
        UpdateCurrentLevel();
	}

	private void InitializeCurrentLevel() {
        // Remove all previous indicators
        for (int i = levelIndicatorParent.childCount - 1; i >= 0; i--) {
            Destroy(levelIndicatorParent.GetChild(i).gameObject);
        }
        // Instantiate new ones
        levelIndicators = new Image[assignedUpgrade.MaxLevel];
        for (int i = 0; i < assignedUpgrade.MaxLevel; i++) {
            levelIndicators[i] = Instantiate<Image>(levelIndicatorPrefab, levelIndicatorParent);
        }
        // Change color according to the current level
        UpdateCurrentLevel();
    }

    private void UpdateCurrentLevel() {
        // Level indicator
        for (int i = 0; i < levelIndicators.Length; i++) {
            // Set color according to the level
            if (i < assignedUpgrade.CurrentLevel) // level purchased
                levelIndicators[i].color = levelIndicatorGradient.Evaluate((float) i / (assignedUpgrade.MaxLevel - 1));
            else // level not purchased
                levelIndicators[i].color = levelNotPurchasedColor;
        }
        // Price and buy button
        if (assignedUpgrade.CurrentLevel == assignedUpgrade.MaxLevel) { // max level reached
            buyObject.SetActive(false);
        } else {
            int price = assignedUpgrade.CoinsCostOfEachLevel[assignedUpgrade.CurrentLevel];
            priceText.text = price.ToString();
            if (PlayerState.Instance.coins < price) { // change to red if not enough coins
                buyButton.interactable = false;
                priceText.color = Color.red; // TODO: Change to using a different color (not so aggressive, e.g. from a color palette)
            }
        }
    }
}
