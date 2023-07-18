using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ShopUI : MonoBehaviour
{
	[Tooltip("A label displaying current number of coins the player has.")]
	[SerializeField] TextMeshProUGUI coinsText;

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

	private List<ShopSpellSlotUI> spells = new List<ShopSpellSlotUI>();
	private List<BroomUpgradeRowUI> broomUpgrades = new List<BroomUpgradeRowUI>();

	public void StartTestingTrack() {
        SceneLoader.Instance.LoadScene(Scene.TestingTrack);
    }

	private void OnEnable() {
		// Register callbacks
		PlayerState.Instance.onCoinsAmountChanged += UpdateCoinsAmount;
		// Initialize
		UpdateCoinsAmount(0, PlayerState.Instance.coins);
		InitializeSpells();
		InitializeBroomUpgrades();
	}

	private void OnDisable() {
		// Unregister callbacks
		PlayerState.Instance.onCoinsAmountChanged -= UpdateCoinsAmount;
	}

	private void InitializeSpells() {
		// Remove all existing spell slots
		for (int i = spellsParent.childCount - 1; i >= 0; i--) {
			Destroy(spellsParent.GetChild(i).gameObject);
		}
		// TODO: Instantiate new spell slots
		spells.Clear();
	}

	private void InitializeBroomUpgrades() {
		broom.UpdateState();
		// Remove all existing broom upgrade rows
		for (int i = broomUpgradesParent.childCount - 1; i >= 0; i--) {
			Destroy(broomUpgradesParent.GetChild(i).gameObject);
		}
		// Instantiate new broom upgrade rows
		broomUpgrades.Clear();
		BroomUpgrade[] upgrades = broom.GetAvailableUpgrades();
		foreach (var upgrade in upgrades) {
			BroomUpgradeRowUI upgradeRow = Instantiate<BroomUpgradeRowUI>(broomUpgradePrefab, broomUpgradesParent);
			upgradeRow.Initialize(upgrade);
			broomUpgrades.Add(upgradeRow);
		}
	}

	private void UpdateCoinsAmount(int oldValue, int newValue) {
		// TODO: Tween the value
		coinsText.text = newValue.ToString();
		// TODO: Update spell slots
		foreach (var spell in spells) {
			spell.UpdateUI();
		}
		// TODO: Update broom upgrades
		broom.UpdateState();
		foreach (var upgrade in broomUpgrades) {
			upgrade.UpdateUI();
		}
	}
}
