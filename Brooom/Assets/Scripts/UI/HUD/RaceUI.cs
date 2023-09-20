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

    [Header("Screen overlays")]
    [Tooltip("An overlay over the whole screen used to e.g. color the screen red.")]
    [SerializeField] Image flashingColorOverlay;
    [Tooltip("A label warning the player that a checkpoint has been missed.")]
    [SerializeField] GameObject missedCheckpointWarning;
    [Tooltip("An image warning the player that they are flying in a wrong direction.")]
    [SerializeField] GenericTween wrongDirectionWarning;

    private CharacterRaceState playerRaceState;

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
    public void UpdatePassedCheckpoints(int checkpointsPassed) {
        checkpointsPassedText.text = checkpointsPassed.ToString();
    }
    public void UpdatePassedHoops(int hoopsPassed) {
        hoopsPassedText.text = hoopsPassed.ToString();
    }
    public void UpdateMissedHoops(int hoopsMissed) {
        if (hoopsMissed != 0)
            hoopsMissedText.text = $"(-{hoopsMissed})";
        // Briefly scale the text up and back down
        EnlargeTextBriefly(hoopsMissedText, 1.5f, 0.5f);
    }
    public void UpdateTimePenalization(int penalizationInSeconds) {
        timePenalizationText.text = $"(+{penalizationInSeconds} s)";
        // Briefly scale the text up and back down
        EnlargeTextBriefly(timePenalizationText, 1.5f, 0.5f);
        // Highlight the race time text - make it bigger and red
        EnlargeTextBriefly(timeText, 1.5f, 0.5f);
        ChangeTextColorBriefly(timeText, Color.red, 0.5f);
    }
    public void UpdatePlace(int place) {
        placeText.text = place.ToString();
        // Change color according to the place
        int colorIndex = place - 1;
        if (colorIndex >= placeColors.Length) colorIndex = placeColors.Length - 1;
        placeBackground.color = placeColors[colorIndex];
    }
    #endregion

    #region Screen overlays
    public void FlashScreenColor(Color color) {
        flashingColorOverlay.color = color;
        flashingColorOverlay.gameObject.TweenAwareEnable();
    }
    public void ShowMissedCheckpointWarning() {
        missedCheckpointWarning.TweenAwareEnable();
    }
    private bool wrongDirectionWarningVisible = false;
    public void ShowWrongDirectionWarning() {
        if (wrongDirectionWarningVisible) return;
        wrongDirectionWarningVisible = true;
        wrongDirectionWarning.loop = true;
        if (!wrongDirectionWarning.IsPlaying())
            wrongDirectionWarning.DoTween();
    }
    public void HideWrongDirectionWarning() {
        if (!wrongDirectionWarningVisible) return;
        wrongDirectionWarningVisible = false;
        wrongDirectionWarning.loop = false;
        wrongDirectionWarning.DoTween();
    }
    public void ShowHideWrongDirectionWarning(bool show) {
        wrongDirectionWarning.loop = show;
        if (show && !wrongDirectionWarning.IsPlaying())
            wrongDirectionWarning.DoTween();
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
        // TODO: Initialize all elements
        ResetRaceState();
        spellsUI.ResetState();
        // TODO: Show elements visible only during the race
        timeObject.SetActive(true);
        placeObject.SetActive(true);
        spellsUI.gameObject.SetActive(true); // TODO: Show only if the player has some spells unlocked
    }

    private void RegisterCallbacks() {
        // Find the player race state and register necessary callbacks
        playerRaceState = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterRaceState>("Player");
        if (playerRaceState != null) {
            playerRaceState.onPlaceChanged += UpdatePlace;
            playerRaceState.onPassedCheckpointsChanged += UpdatePassedCheckpoints;
            playerRaceState.onPassedHoopsChanged += UpdatePassedHoops;
            playerRaceState.onMissedHoopsChanged += UpdateMissedHoops;
            playerRaceState.onTimePenalizationChanged += UpdateTimePenalization;
            playerRaceState.onWrongDirectionChanged += ShowHideWrongDirectionWarning;
        }
    }

    private void UnregisterCallbacks() {
        if (playerRaceState != null) {
            playerRaceState.onPlaceChanged -= UpdatePlace;
            playerRaceState.onPassedCheckpointsChanged -= UpdatePassedCheckpoints;
            playerRaceState.onPassedHoopsChanged -= UpdatePassedHoops;
            playerRaceState.onMissedHoopsChanged -= UpdateMissedHoops;
            playerRaceState.onTimePenalizationChanged -= UpdateTimePenalization;
            playerRaceState.onWrongDirectionChanged -= ShowHideWrongDirectionWarning;
        }
    }

    private void EnlargeTextBriefly(TextMeshProUGUI text, float sizeMultiplier, float duration) {
        float originalSize = text.fontSize;
        float biggerSize = originalSize * sizeMultiplier;
        DOTween.To(() => text.fontSize, x => text.fontSize = x, biggerSize, duration/2f)
            .OnComplete(() => DOTween.To(() => text.fontSize, x => text.fontSize = x, originalSize, duration / 2f));
    }

    private void ChangeTextColorBriefly(TextMeshProUGUI text, Color color, float duration) {
        Color originalColor = text.color;
        text.DOColor(color, duration / 2f).OnComplete(() => text.DOColor(originalColor, duration / 2f));
    }

    private void Start() {
        // Initialize everything
        ResetRaceState();
        // Register necessary callbacks
        RegisterCallbacks();
        // TODO: Hide all elements which should not be visible during training
        timeObject.SetActive(false);
        placeObject.SetActive(false);
        spellsUI.gameObject.SetActive(false);
    }

	private void OnDestroy() {
        UnregisterCallbacks();
	}
}
