using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;


/// <summary>
/// A component representing UI of the Shop screen.
/// It provides methods for different buttons in the screen and makes sure all UI elements are initialized correctly when the screen is displayed.
/// </summary>
public class ShopUI : MonoBehaviour {

	[Header("Spells")]
	[Tooltip("A prefab of a spell slot which is instantiated multiple times.")]
	[SerializeField] ShopSpellSlotUI spellSlotPrefab;
	[Tooltip("A Transform which is a parent of all the spell slots.")]
	[SerializeField] Transform spellsParent;

	[Header("Broom upgrades")]
	[Tooltip("A prefab of a broom upgrade row which is instantiated multiple times.")]
	[SerializeField] BroomUpgradeRowUI broomUpgradePrefab;
	[Tooltip("A Transform which is a parent of all the broom upgrades.")]
	[SerializeField] Transform broomUpgradesParent;
	[Tooltip("A broom object in the scene which provides preview of the changes after upgrade.")]
	[SerializeField] Broom broom;

	private List<ShopSpellSlotUI> spells = new(); // instantiated spell slots
	private List<BroomUpgradeRowUI> broomUpgrades = new(); // instantiated broom upgrade rows


	/// <summary>
	/// Loads the Testing Track scene.
	/// </summary>
	public void StartTestingTrack() {
		Analytics.Instance.LogEvent(AnalyticsCategory.TestingTrack, "Testing track entered.");
        SceneLoader.Instance.LoadScene(Scene.TestingTrack);
    }

	private void OnEnable() {
		// Register callbacks
		PlayerState.Instance.onCoinsAmountChanged += UpdateUI; // some spell/upgrade may have been purchased
		// Initialize
		InitializeSpells();
		InitializeBroomUpgrades();
		UpdateUI(0, PlayerState.Instance.Coins);
		Analytics.Instance.LogEvent(AnalyticsCategory.Shop, "Shop opened.");
	}

	private void OnDisable() {
		// Unregister callbacks
		PlayerState.Instance.onCoinsAmountChanged -= UpdateUI;
		Analytics.Instance.LogEvent(AnalyticsCategory.Shop, "Shop closed.");
	}

	// Instantiates and initializes shop spell slots for all spells in a grid
	private void InitializeSpells() {
		// Remove all existing spell slots
		UtilsMonoBehaviour.RemoveAllChildren(spellsParent);
		// Instantiate new spell slots
		spells.Clear();
		int spellCount = SpellManager.Instance.AllSpells.Count;
		if (spellCount > 12) { // Increase number of columns
			spellsParent.GetComponent<GridLayoutGroup>().constraintCount = Mathf.CeilToInt(spellCount / 3f);
		}
		foreach (var spell in SpellManager.Instance.AllSpells) {
			ShopSpellSlotUI spellSlot = Instantiate<ShopSpellSlotUI>(spellSlotPrefab, spellsParent);
			spellSlot.Initialize(spell);
			spells.Add(spellSlot);
		}
	}

	// Instantiates and initializes broom upgrade rows
	private void InitializeBroomUpgrades() {
		broom.UpdateFromPlayerState();
		// Remove all existing broom upgrade rows
		UtilsMonoBehaviour.RemoveAllChildren(broomUpgradesParent);
		// Instantiate new broom upgrade rows
		broomUpgrades.Clear();
		BroomUpgrade[] upgrades = broom.GetAvailableUpgrades();
		foreach (var upgrade in upgrades) {
			BroomUpgradeRowUI upgradeRow = Instantiate<BroomUpgradeRowUI>(broomUpgradePrefab, broomUpgradesParent);
			upgradeRow.Initialize(upgrade);
			broomUpgrades.Add(upgradeRow);
		}
	}

	// Updates all UI elements - spell slots, broom upgrades
	private void UpdateUI(int oldValue, int newValue) {
		// Update spell slots
		foreach (var spell in spells) {
			spell.UpdateUI();
		}
		// Update broom upgrades
		broom.UpdateFromPlayerState();
		foreach (var upgrade in broomUpgrades) {
			upgrade.UpdateUI();
		}
	}

}
