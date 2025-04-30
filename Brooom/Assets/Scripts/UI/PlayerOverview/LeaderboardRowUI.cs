using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


/// <summary>
/// A component representing a single row in the global leaderboard of racers, displayed in Player Overview.
/// </summary>
public class LeaderboardRowUI : MonoBehaviour {

	[Header("Basic text fields")]
	[Tooltip("Image used as a background under a label containing place in the leaderboard. Its color is changed based on the place.")]
	[SerializeField] Image placeBackground;
	[Tooltip("Label displaying a place in the leaderboard.")]
	[SerializeField] TextMeshProUGUI placeText;
	[Tooltip("Label displaying the racer's name.")]
	[SerializeField] TextMeshProUGUI nameText;
	[Tooltip("Label displaying the racer's stats average.")]
	[SerializeField] TextMeshProUGUI averageText;

	[Header("Additional text fields")]
	[Tooltip("Parent of everything related to displaying change in rank. Is set to inactive when there is no change.")]
	[SerializeField] GameObject placeChangeLabel;
	[Tooltip("Label displaying change in rank since the last time.")]
	[SerializeField] TextMeshProUGUI placeChangeText;
	[Tooltip("Image used as a background under a label containing change in rank. Its color is changed based on whether the change is positive or negative.")]
	[SerializeField] Image placeChangeBackground;
	[Tooltip("Parent of everything related to displaying change in stats average. Is set to inactive when there is no change.")]
	[SerializeField] GameObject averageChangeLabel;
	[Tooltip("Label displaying change in stats average since the last time.")]
	[SerializeField] TextMeshProUGUI averageChangeText;
	[Tooltip("Image used as a background under a label containing change in stats average. Its color is changed based on whether the change is positive or negative.")]
	[SerializeField] Image averageChangeBackground;
	[Tooltip("Duration (in seconds) of a tween used when displaying changes in rank and in stats average.")]
	[SerializeField] float tweenDuration = 0.5f;
	[Tooltip("Delay after which the changes in rank and in stats average are displayed.")]
	[SerializeField] float tweenDelay = 1f;

	[Header("Row highlight")]
	[Tooltip("Background of the row. Its color will be changed for the player.")]
	[SerializeField] Image background;

	private int currentPlaceChange = 0;
	private float currentAverageChange = 0f;

	/// <summary>
	/// Initializes UI elements based on the given values.
	/// </summary>
	/// <param name="place">Place in the leaderboard to be displayed in the row.</param>
	/// <param name="name">Racer's name to be displayed in the leaderboard row.</param>
	/// <param name="average">Stats average to be displayed in the leaderboard row.</param>
	public void Initialize(int place, string name, float average) {
		placeText.text = place.ToString();
		placeBackground.color = ColorPalette.Instance.GetLeaderboardPlaceColor(place);
		nameText.text = name;
		averageText.text = average.ToString("N1");
	}

	/// <summary>
	/// Changes the row's appearance to indicate it contains the player's data.
	/// It shows the changes in rank and stats average, and also highlights the row.
	/// </summary>
	/// <param name="placeChange">The player's change in rank since the last time.</param>
	/// <param name="averageChange">The player's change in stats average since the last time.</param>
	public void SetPlayerData(int placeChange, float averageChange) {
		// Initialize values
		currentPlaceChange = 0;
		SetPlaceChange(currentPlaceChange);
		currentAverageChange = 0;
		SetAverageChange(currentAverageChange);
		// Highlight
		background.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_HighlightColor);
		// Tween the values
		DOTween.To(() => currentPlaceChange, x => { currentPlaceChange = x; SetPlaceChange(x); }, placeChange, tweenDuration).SetDelay(tweenDelay);
		DOTween.To(() => currentAverageChange, x => { currentAverageChange = x; SetAverageChange(x); }, averageChange, tweenDuration).SetDelay(tweenDelay);
	}

	// Displays the change in rank (shows the value in label and changes background color based on whether the change is positive or negative)
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

	// Displays the change in stats average (shows the value in label and changes background color based on whether the change is positive or negative)
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
