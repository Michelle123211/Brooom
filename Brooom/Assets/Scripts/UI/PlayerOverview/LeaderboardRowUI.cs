using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LeaderboardRowUI : MonoBehaviour {
	[Header("Basic text fields")]
	[SerializeField] Image placeBackground;
	[SerializeField] TextMeshProUGUI placeText;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI averageText;

	[Header("Additional text fields")]
	[SerializeField] GameObject placeChangeLabel;
	[SerializeField] TextMeshProUGUI placeChangeText;
	[SerializeField] Image placeChangeBackground;
	[SerializeField] GameObject averageChangeLabel;
	[SerializeField] TextMeshProUGUI averageChangeText;
	[SerializeField] Image averageChangeBackground;
	[SerializeField] float tweenDuration = 0.5f;
	[SerializeField] float tweenDelay = 1f;

	[Header("Row highlight")]
	[Tooltip("Background of the row. Its color will be changed for the player.")]
	[SerializeField] Image background;

	private int currentPlaceChange = 0;
	private float currentAverageChange = 0f;

	public void Initialize(int place, string name, float average) {
		placeText.text = place.ToString();
		placeBackground.color = ColorPalette.Instance.GetLeaderboardPlaceColor(place);
		nameText.text = name;
		averageText.text = average.ToString("N1");
	}

	// Shows the change in place and average, changes appearance to highlight the row
	public void SetPlayerData(int placeChange, float averageChange) {
		// Initialize values
		currentPlaceChange = 0;
		SetPlaceChange(currentPlaceChange);
		currentAverageChange = 0;
		SetAverageChange(currentAverageChange);
		// Highlight
		background.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_HighlightColor);
		nameText.text = "<b>" + nameText.text + "</b>";
		// Tween the values
		DOTween.To(() => currentPlaceChange, x => { currentPlaceChange = x; SetPlaceChange(x); }, placeChange, tweenDuration).SetDelay(tweenDelay);
		DOTween.To(() => currentAverageChange, x => { currentAverageChange = x; SetAverageChange(x); }, averageChange, tweenDuration).SetDelay(tweenDelay);
	}

	private void SetPlaceChange(int placeChange) {
		placeChangeBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_PositiveColor);
		if (placeChange < 0) {
			placeChangeText.text = placeChange.ToString();
		} else if (placeChange > 0) {
			placeChangeText.text = "+" + placeChange.ToString();
			placeChangeBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
		}
		if (placeChange != 0) placeChangeLabel.SetActive(true);
	}

	private void SetAverageChange(float averageChange) {
		averageChangeBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_PositiveColor);
		if (averageChange > 0) {
			averageChangeText.text = "+" + averageChange.ToString("N1");
		} else if (averageChange < 0) {
			averageChangeText.text = averageChange.ToString("N1");
			averageChangeBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
		}
		if (averageChange != 0) averageChangeLabel.SetActive(true);
	}
}
