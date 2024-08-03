using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ShopUI : MonoBehaviour
{

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
		PlayerState.Instance.onCoinsAmountChanged += UpdateUI; // some spell/upgrade may have been purchased
		// Initialize
		InitializeSpells();
		InitializeBroomUpgrades();
		UpdateUI(0, PlayerState.Instance.coins);
	}

	private void OnDisable() {
		// Unregister callbacks
		PlayerState.Instance.onCoinsAmountChanged -= UpdateUI;
	}

	private void InitializeSpells() {
		// Remove all existing spell slots
		UtilsMonoBehaviour.RemoveAllChildren(spellsParent);
		// TODO: Instantiate new spell slots - check that the following is working
		spells.Clear();
		foreach (var spell in SpellManager.Instance.AllSpells) {
			ShopSpellSlotUI spellSlot = Instantiate<ShopSpellSlotUI>(spellSlotPrefab, spellsParent);
			spellSlot.Initialize(spell);
		}
	}

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
