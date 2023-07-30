using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceResultRowUI : MonoBehaviour {

    [Header("Text fields")]
    [SerializeField] TextMeshProUGUI placeText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI rewardText;
    [Tooltip("An object containing everything related to coin reward. Is set to inactive when there is no reward.")]
    [SerializeField] GameObject rewardObject;

    [Header("Row highlight")]
    [Tooltip("Background of the row. Its color will be changed for the player.")]
    [SerializeField] Image background;
    [Tooltip("Color to which the row's background will be changed.")]
    [SerializeField] Color highlightColor; // TODO: Maybe take it from a color palette

    public void Initialize(int place, RaceResultData data) {
        // Initialize all text fields
        placeText.text = place.ToString();
        nameText.text = data.name;
        timeText.text = Utils.FormatTime(data.time);
        if (data.coinsReward != 0) {
            rewardText.text = data.coinsReward.ToString();
        }
        rewardObject.SetActive(data.coinsReward != 0);
    }

    public void HighlightPlayer() {
        // Change the background color and make the name bold
        background.color = highlightColor;
        nameText.text = "<b>" + nameText.text + "</b>";
    }
}
