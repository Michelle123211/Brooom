using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a single row of results displayed at the end of the race.
/// </summary>
public class RaceResultRowUI : MonoBehaviour {

    [Header("Individual parts")]
    [Tooltip("An Image used as a background under a label containing place in race. Its color is changed based on the place.")]
    [SerializeField] Image placeImage;
    [Tooltip("A label displaying a place in race.")]
    [SerializeField] TextMeshProUGUI placeText;
    [Tooltip("An Image displaying color corresponding to the racer.")]
    [SerializeField] Image colorImage;
    [Tooltip("A label displaying the racer's name.")]
    [SerializeField] TextMeshProUGUI nameText;
    [Tooltip("A label displaying the finish time.")]
    [SerializeField] TextMeshProUGUI timeText;
    [Tooltip("A label displaying the time penalization.")]
    [SerializeField] TextMeshProUGUI penalizationText;
    [Tooltip("A tooltip explaining time penalization for missed hoops.")]
    [SerializeField] SimpleTooltip timeTooltip;
    [Tooltip("A label displaying the coin reward.")]
    [SerializeField] TextMeshProUGUI rewardText;
    [Tooltip("A tooltip explaining individual components of the coin reward.")]
    [SerializeField] SimpleTooltip rewardTooltip;
    [Tooltip("An object containing everything related to coin reward. Is set to inactive when there is no reward.")]
    [SerializeField] GameObject rewardObject;

    [Header("Row highlight")]
    [Tooltip("Background of the row. Its color will be changed for the player.")]
    [SerializeField] Image background;

    /// <summary>
    /// Initializes this row with the given data.
    /// </summary>
    /// <param name="place">Place to which this results row corresponds.</param>
    /// <param name="data">Race results data to be displayed (e.g., color, name, finish time, reward).</param>
    public void Initialize(int place, RaceResultData data) {
        nameText.text = data.name;
        colorImage.color = data.color;
        SetPlace(place);
        SetTime(data.time, Mathf.RoundToInt(data.timePenalization));
        SetReward(data.coinsReward, data.coinsPenalization);
    }

    /// <summary>
    /// Changes the background color, scales the row up and hides image containing the racer's color,
    /// to indicate that this row shows the player's results.
    /// </summary>
    public void HighlightPlayer() {
        // Change the background color, scale the row up, hide color
        background.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_HighlightColor);
        transform.localScale = transform.localScale * 1.2f;
        colorImage.gameObject.SetActive(false);
    }

    // Updates label containing place in race and its background color
    private void SetPlace(int place) {
        placeText.text = place.ToString();
        placeImage.color = ColorPalette.Instance.GetLeaderboardPlaceColor(place);
    }

    // Updates labels containing finish time and time penalization, tooltip explaining time penalization for missed hoops
    private void SetTime(float time, int timePenalization) {
        if (time < 0) timeText.text = "DNF";
        else timeText.text = Utils.FormatTime(time);

        if (timePenalization > 0) {
            penalizationText.text = $"(+{timePenalization} s)";
            timeTooltip.text = string.Format(LocalizationManager.Instance.GetLocalizedString("ResultsTooltipHoops"), timePenalization);
        } else {
            penalizationText.gameObject.SetActive(false);
            timeTooltip.enabled = false; // hide tooltip explaining penalization
        }
    }

    // Updates label containing coin reward and tooltip explaining it
    private void SetReward(int coinsReward, int coinsPenalization) {
        // Coins
        int coinsDelta = coinsReward - coinsPenalization;
        if (coinsDelta != 0) {
            if (coinsDelta > 0) { // reward
                rewardText.text = $"+{coinsDelta}";
                rewardText.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_PositiveColor);
            } else { // penalization
                rewardText.text = coinsDelta.ToString();
                rewardText.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
            }
            rewardObject.SetActive(true);
        } else {
            rewardObject.SetActive(false);
        }
        // Coins tooltip
        rewardTooltip.text = string.Empty;
        if (coinsReward != 0)
            rewardTooltip.text = string.Format(LocalizationManager.Instance.GetLocalizedString("ResultsTooltipReward"), coinsReward);
        if (coinsPenalization != 0) {
            if (string.IsNullOrEmpty(rewardTooltip.text))
                rewardTooltip.text = string.Format(LocalizationManager.Instance.GetLocalizedString("ResultsTooltipExposure"), coinsPenalization);
            else
                rewardTooltip.text += "\n" + string.Format(LocalizationManager.Instance.GetLocalizedString("ResultsTooltipExposure"), coinsPenalization);
        }
        if (string.IsNullOrEmpty(rewardTooltip.text)) rewardTooltip.enabled = false;
    }

}
