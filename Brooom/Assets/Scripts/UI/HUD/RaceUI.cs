using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RaceUI : MonoBehaviour {

    [Header("Basic info")]
    [Tooltip("A GameObject containing all time elements which will be hidden during training.")]
    [SerializeField] GameObject timeObject;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI timePenalizationText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI altitudeText;

    [Header("Race state")]
    [Tooltip("A RectTransform which is a parent of all the checkpoints labels.")]
    [SerializeField] RectTransform checkpointsLayout;
    [SerializeField] TextMeshProUGUI checkpointsPassedText;
    [SerializeField] TextMeshProUGUI checkpointsTotalText;
    [Tooltip("A RectTransform which is a parent of all the hoops labels.")]
    [SerializeField] RectTransform hoopsLayout;
    [SerializeField] TextMeshProUGUI hoopsPassedText;
    [SerializeField] TextMeshProUGUI hoopsTotalText;
    [SerializeField] TextMeshProUGUI hoopsMissedText;
    [Tooltip("A GameObject containing all parts of the place UI element which will be hidden during training.")]
    [SerializeField] GameObject placeObject;
    [SerializeField] Image placeBackground;
    [SerializeField] TextMeshProUGUI placeText;

    [Header("Spells")]
    [SerializeField] RaceSpellsUI spellsUI;

    [Header("Temporary text fields")]
    [SerializeField] TextMeshProUGUI startRaceText;
    [SerializeField] TextMeshProUGUI enteringRaceCountdownText;

	string enteringRaceString;
    Color[] placeColors = new Color[] { // TODO: Move to a separate color palette
        Utils.ColorFromRBG256(243, 217, 81), // gold
        Utils.ColorFromRBG256(164, 164, 164), // silver
        Utils.ColorFromRBG256(203, 128, 83), // bronz
        Utils.ColorFromRBG256(126, 92, 80) };

    #region Basic information
    public void UpdateTime(float timeInSeconds) {
        timeText.text = Utils.FormatTime(timeInSeconds);
    }
    public void UpdatePlayerState(float speed, float altitude) {
        speedText.text = Math.Round(speed, 1).ToString();
        altitudeText.text = Math.Round(altitude, 1).ToString();
    }
    #endregion

    #region Race state
    public void InitializeCheckpointsAndHoops(int checkpointsTotal, int hoopsTotal) {
        checkpointsPassedText.text = "0";
        checkpointsTotalText.text = checkpointsTotal.ToString();
        hoopsPassedText.text = "0";
        hoopsTotalText.text = hoopsTotal.ToString();
        hoopsMissedText.text = "";
    }
    public void UpdatePlayerPositionWithinRace(int checkpointsPassed, int hoopsPassed) {
        checkpointsPassedText.text = checkpointsPassed.ToString();
        hoopsPassedText.text = hoopsPassed.ToString();
    }
    public void UpdateTimePenalization(int penalizationInSeconds, int missedHoops) {
        timePenalizationText.text = $"(+{penalizationInSeconds} s)";
        hoopsMissedText.text = missedHoops.ToString();
    }
    public void UpdatePlace(int place) {
        placeText.text = place.ToString();
        // Change color according to the place
        int colorIndex = place - 1;
        if (colorIndex >= placeColors.Length) colorIndex = placeColors.Length - 1;
        placeBackground.color = placeColors[colorIndex];
    }
    #endregion

    #region Entering race countdown
    public void StartEnteringRaceCountdown(float initialValue) {
        UpdateEnteringRaceCountdown(initialValue);
        enteringRaceCountdownText.gameObject.SetActive(true);
    }
    public void StopEnteringRaceCountdown() {
        enteringRaceCountdownText.gameObject.SetActive(false);
    }
    public void UpdateEnteringRaceCountdown(float value) {
        enteringRaceCountdownText.text = string.Format(enteringRaceString, value.ToString());
    }
    #endregion

	private void ResetRaceState() {
        UpdatePlace(1);

        checkpointsPassedText.text = "0";
        hoopsPassedText.text = "0";
        hoopsMissedText.text = "";
        timePenalizationText.text = "";
    }

	public void StartRace() {
        // TODO: Hide all elements which are visible only during the training
        // TODO: Uncomment when finished debugging
        //enteringRaceCountdownText.gameObject.SetActive(false);
        //startRaceText.gameObject.SetActive(false);
        // ...
        // TODO: Initialize all elements
        ResetRaceState();
        spellsUI.ResetState();
        // TODO: Show elements visible only during the race
        //timeObject.SetActive(true);
        //placeObject.SetActive(true);
        //spellsObject.SetActive(true);
    }

    private void RegisterCallbacks() {
        // Register necessary callbacks
        PlayerState.Instance.raceState.onPlayerPlaceChanged += UpdatePlace;
        PlayerState.Instance.raceState.onPlayerPositionWithinRaceChanged += UpdatePlayerPositionWithinRace;
    }

    private void UnregisterCallbacks() {
        PlayerState.Instance.raceState.onPlayerPlaceChanged -= UpdatePlace;
        PlayerState.Instance.raceState.onPlayerPositionWithinRaceChanged -= UpdatePlayerPositionWithinRace;
    }

    private void Start() {
        // Cache localized strings
        enteringRaceString = LocalizationManager.Instance.GetLocalizedString("RaceLabelEntering");
        // ...
        // Initialize everything
        ResetRaceState();
        // Register necessary callbacks
        RegisterCallbacks();
        // TODO: Hide all elements which should not visible during training
        // TODO: Uncomment when finished debugging
        timePenalizationText.gameObject.SetActive(false);
        //timeObject.SetActive(false);
        //placeObject.SetActive(false);
        //spellsObject.SetActive(false);
    }

	private void OnDestroy() {
        UnregisterCallbacks();
	}
}
