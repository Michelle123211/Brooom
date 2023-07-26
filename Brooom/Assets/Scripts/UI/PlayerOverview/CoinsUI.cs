using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CoinsUI : MonoBehaviour
{
	[Tooltip("A label displaying current number of coins the player has.")]
	[SerializeField] TextMeshProUGUI coinsText;

	private int lastValue = 0;

	public void RefreshCoinsAmount() {
		UpdateCoinsAmount(lastValue, PlayerState.Instance.coins);
	}

	private void UpdateCoinsAmount(int oldValue, int newValue) {
		// TODO: Tween the value
		lastValue = newValue;
		coinsText.text = newValue.ToString();
	}

	private void OnEnable() {
		// Register callback
		PlayerState.Instance.onCoinsAmountChanged += UpdateCoinsAmount;
		// Initialize
		UpdateCoinsAmount(0, PlayerState.Instance.coins);
	}

	private void OnDisable() {
		// Unregister callback
		PlayerState.Instance.onCoinsAmountChanged -= UpdateCoinsAmount;
	}
}
