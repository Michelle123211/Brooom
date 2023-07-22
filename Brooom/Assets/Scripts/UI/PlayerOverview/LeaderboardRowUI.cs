using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRowUI : MonoBehaviour {
	[Header("Basic text fields")]
	[SerializeField] TextMeshProUGUI placeText;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI averageText;

	[Header("Additional text fields")]
	[SerializeField] TextMeshProUGUI placeChangeText;
	[SerializeField] TextMeshProUGUI averageChangeText;

	[Header("Row highlight")]
	[Tooltip("Background of the row. Its color will be changed for the player.")]
	[SerializeField] Image background;
	[Tooltip("Color to which the row's background will be changed.")]
	[SerializeField] Color highlightColor; // TODO: Maybe take it from a color palette

	public void Initialize(int place, string name, float average) {
		placeText.text = place.ToString();
		nameText.text = name;
		averageText.text = average.ToString("N1");
	}

	// Shows the change in place and average, changes appearance to highlight the row
	public void SetPlayerData(int placeChange, float averageChange) {
		// TODO: Tween the values
		// Change in place
		if (placeChange < 0) {
			placeChangeText.text = placeChange.ToString();
			placeChangeText.color = Color.green; // TODO: Take color from a color palette
		} else if (placeChange > 0) {
			placeChangeText.text = "+" + placeChange.ToString();
			placeChangeText.color = Color.red; // TODO: Take color from a color palette
		}
		if (placeChange != 0) placeChangeText.gameObject.SetActive(true);
		// Change in average
		if (averageChange > 0) {
			averageChangeText.text = "+" + averageChange.ToString("N1");
			averageChangeText.color = Color.green; // TODO: Take color from a color palette
		} else if (averageChange < 0) {
			averageChangeText.text = averageChange.ToString("N1");
			averageChangeText.color = Color.red; // TODO: Take color from a color palette
		}
		if (averageChange != 0) averageChangeText.gameObject.SetActive(true);
		// Highlight
		background.color = highlightColor;
		nameText.text = "<b>" + nameText.text + "</b>";
	}
}
