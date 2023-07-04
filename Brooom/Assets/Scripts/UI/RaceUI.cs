using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class RaceUI : MonoBehaviour {

    [Header("Basic info")]
    [Tooltip("A GameObject containing all time elements which will be hidden during training.")]
    [SerializeField] GameObject timeObject;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI timePenalizationText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI altitudeText;

    [Header("Race state")]
    [SerializeField] TextMeshProUGUI checkpointsPassedText;
    [SerializeField] TextMeshProUGUI checkpointsTotalText;
    [SerializeField] TextMeshProUGUI hoopsPassedText;
    [SerializeField] TextMeshProUGUI hoopsTotalText;
    [SerializeField] TextMeshProUGUI hoopsMissedText;
    [Tooltip("A GameObject containing all parts of the place UI element which will be hidden during training.")]
    [SerializeField] GameObject placeObject;
    [SerializeField] TextMeshProUGUI placeText;

    [Header("Spells")]
    [SerializeField] ProgressBar manaBar;
    [SerializeField] VerticalLayoutGroup spellSlotsGroup;
    [SerializeField] HorizontalLayoutGroup effectsGroup;

    [Header("Temporary text fields")]
    [SerializeField] TextMeshProUGUI startRaceText;
    [SerializeField] TextMeshProUGUI enteringRaceCountdownText;

	string enteringRaceString;

    #region Basic information
    public void UpdateTime(float timeInSeconds) {
        timeText.text = Utils.FormatTime(timeInSeconds);
    }
    public void UpdateTimePenalization(int penalizationInSeconds) {
        timePenalizationText.text = $"(+{penalizationInSeconds} s)";
        timePenalizationText.gameObject.SetActive(true);
    }
    public void UpdatePlayerState(float speed, float altitude) {
        speedText.text = Math.Round(speed, 1).ToString();
        altitudeText.text = Math.Round(altitude, 1).ToString();
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


	public void StartRace() {
        // TODO: Hide all elements which are visible only during the training
        // TODO: Uncomment when finished debugging
        //enteringRaceCountdownText.gameObject.SetActive(false);
        //startRaceText.gameObject.SetActive(false);
        // ...
        // TODO: Initialize all elements
        // TODO: Show elements visible only during the race
    }

	private void Start() {
        enteringRaceString = LocalizationManager.Instance.GetLocalizedString("RaceLabelEntering");
        // TODO: Hide all elements which should not visible during training
        // TODO: Uncomment when finished debugging
        //timeObject.SetActive(false);
        //placeObject.SetActive(false);
        // ...
    }
}
