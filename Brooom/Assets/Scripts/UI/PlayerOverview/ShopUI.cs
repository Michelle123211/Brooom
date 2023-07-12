using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ShopUI : MonoBehaviour
{
	[Tooltip("A label displaying current number of coins the player has.")]
	[SerializeField] TextMeshProUGUI coinsText;

	[Header("Spells for sell")]
	[Tooltip("A prefab of a spell slot which is instantiated multiple times.")]
	[SerializeField] ShopSpellSlotUI spellSlotPrefab;
	[Tooltip("A Transform which is a parent of all the spell slots.")]
	[SerializeField] Transform spellsParent;

	[Header("Equipped spells")]
	[Tooltip("A prefab of an equipped spell slot which is instantiated multiple times.")]
	[SerializeField] EquippedSpellSlotUI equippedSpellSlotPrefab;
	[Tooltip("A Transform which is a parent of all the equipped spell slots.")]
	[SerializeField] Transform equippedSpellsParent;

	[Header("Broom upgrades for sell")]
	[Tooltip("A Transform which is a parent of all the broom upgrades.")]
	[SerializeField] Transform broomUpgradesParent;

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
		// Remove all existing equipped spell slots
		// TODO: Instantiate new equipped spell slots
	}

	private void InitializeBroomUpgrades() {
		// Remove all existing broom upgrade rows
		for (int i = broomUpgradesParent.childCount - 1; i >= 0; i--) {
			Destroy(broomUpgradesParent.GetChild(i).gameObject);
		}
		// TODO: Instantiate new broom upgrade rows
	}

	private void UpdateCoinsAmount(int oldValue, int newValue) {
		// TODO: Tween the value
		coinsText.text = newValue.ToString();
	}
}
