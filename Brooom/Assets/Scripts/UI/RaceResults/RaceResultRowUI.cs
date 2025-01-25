using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceResultRowUI : MonoBehaviour
{
    [Header("Text fields")]
    [SerializeField] Image placeImage;
    [SerializeField] TextMeshProUGUI placeText;
    [SerializeField] Image colorImage;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI penalizationText;
    [SerializeField] SimpleTooltip timeTooltip;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] SimpleTooltip rewardTooltip;
    [Tooltip("An object containing everything related to coin reward. Is set to inactive when there is no reward.")]
    [SerializeField] GameObject rewardObject;

    [Header("Row highlight")]
    [Tooltip("Background of the row. Its color will be changed for the player.")]
    [SerializeField] Image background;

    public void Initialize(int place, RaceResultData data) {
        nameText.text = data.name;
        colorImage.color = data.color;
        SetPlace(place);
        SetTime(data.time, Mathf.RoundToInt(data.timePenalization));
        SetReward(data.coinsReward, data.coinsPenalization);
    }

    public void HighlightPlayer() {
        // Change the background color, scale the row up, hide color
        background.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_HighlightColor);
        transform.localScale = transform.localScale * 1.2f;
        colorImage.gameObject.SetActive(false);
    }

    private void SetPlace(int place) {
        placeText.text = place.ToString();
        placeImage.color = ColorPalette.Instance.GetLeaderboardPlaceColor(place);
    }

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
