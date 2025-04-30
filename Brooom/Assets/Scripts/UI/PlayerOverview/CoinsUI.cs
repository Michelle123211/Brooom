using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;


/// <summary>
/// A component displaying current number of coins the player has.
/// </summary>
public class CoinsUI : MonoBehaviour {

	[Tooltip("A label displaying current number of coins the player has.")]
	[SerializeField] TextMeshProUGUI coinsText;

	private int lastValue = 0;
	private DG.Tweening.Core.TweenerCore<int, int, DG.Tweening.Plugins.Options.NoOptions> coinTween;

	/// <summary>
	/// Updates the displayed value based on the current game state.
	/// </summary>
	public void RefreshCoinsAmount() {
		UpdateCoinsAmount(lastValue, PlayerState.Instance.Coins);
	}

	/// <summary>
	/// Tweens the displayed value from the old value to the new value.
	/// </summary>
	/// <param name="oldValue">Original value.</param>
	/// <param name="newValue">Target value.</param>
	public void UpdateCoinsAmount(int oldValue, int newValue) {
		// Tween the value
		if (this.coinTween != null) this.coinTween.Kill();
		lastValue = oldValue;
		coinTween = DOTween.To(() => lastValue, x => { lastValue = x; coinsText.text = x.ToString(); }, newValue, 0.3f);
	}

	private void OnEnable() {
		// Register callback
		PlayerState.Instance.onCoinsAmountChanged += UpdateCoinsAmount;
		// Initialize
		UpdateCoinsAmount(0, PlayerState.Instance.Coins);
	}

	private void OnDisable() {
		// Unregister callback
		PlayerState.Instance.onCoinsAmountChanged -= UpdateCoinsAmount;
	}
}
