using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


/// <summary>
/// A component representing HUD during race.
/// It is responsible for displaying all the different parts of the HUD, e.g., race time with penalization, current speed and altitude, 
/// hoops and checkpoints, place, equipped spells, race countdown.
/// It also provides methods for updating them.
/// On top of that, it displays visual warnings when the player misses a hoop/checkpoint or is flying in a wrong direction.
/// </summary>
public class RaceUI : MonoBehaviour {

    [Header("Basic info")]
    [Tooltip("A GameObject containing all time elements which will be hidden during training.")]
    [SerializeField] GameObject timeObject;
    [Tooltip("A label displaying current race time.")]
    [SerializeField] TextMeshProUGUI timeText;
    [Tooltip("A label displaying current time penalization.")]
    [SerializeField] TextMeshProUGUI timePenalizationText;
    [Tooltip("A label displaying current speed.")]
    [SerializeField] TextMeshProUGUI speedText;
    [Tooltip("A label displaying current altitude.")]
    [SerializeField] TextMeshProUGUI altitudeText;

    [Header("Race state")]
    [Tooltip("A RectTransform which is a parent of all the checkpoints labels.")]
    [SerializeField] RectTransform checkpointsLayout;
    [Tooltip("A label displaying number of checkpoints passed so far.")]
    [SerializeField] TextMeshProUGUI checkpointsPassedText;
    [Tooltip("A label displaying total number of checkpoints in the track.")]
    [SerializeField] TextMeshProUGUI checkpointsTotalText;
    [Tooltip("A RectTransform which is a parent of all the hoops labels.")]
    [SerializeField] RectTransform hoopsLayout;
    [Tooltip("A label displaying number of hoops passed so far.")]
    [SerializeField] TextMeshProUGUI hoopsPassedText;
    [Tooltip("A label displaying total number of hoops in the track.")]
    [SerializeField] TextMeshProUGUI hoopsTotalText;
    [Tooltip("A label displaying number of hoops missed so far.")]
    [SerializeField] TextMeshProUGUI hoopsMissedText;
    [Tooltip("A GameObject containing all parts of the place UI element which will be hidden during training.")]
    [SerializeField] GameObject placeObject;
    [Tooltip("An Image used as a background for the player's place in race label.")]
    [SerializeField] Image placeBackground;
    [Tooltip("A label displaying the player's current place in race.")]
    [SerializeField] TextMeshProUGUI placeText;

    [Header("Spells")]
    [Tooltip("HUD part responsible for displaying slots with equipped spells in the race.")]
    [SerializeField] RaceSpellsUI spellsUI;

    [Header("Race countdown")]
    [Tooltip("A GameObject containing all UI elements of the countdown before the race starts.")]
    [SerializeField] GameObject countdownObject;
    [Tooltip("A label displaying the current countdown before the race starts.")]
    [SerializeField] TextMeshProUGUI countdownText;

    [Header("Screen overlays")]
    [Tooltip("An overlay over the whole screen used to e.g. color the screen red.")]
    [SerializeField] Image flashingColorOverlay;
    [Tooltip("A label warning the player that a checkpoint has been missed.")]
    [SerializeField] GameObject missedCheckpointWarning;
    [Tooltip("An image warning the player that they are flying in a wrong direction.")]
    [SerializeField] GenericTween wrongDirectionWarning;

    private CharacterRaceState playerRaceState;


    #region Coutndown
    /// <summary>
    /// Shows countdown before the race starts with the given value for a brief moment.
    /// Font size and alpha value are tweened to make the countdown appear and disappear nicely.
    /// </summary>
    /// <param name="countdownValue">Countdown value to be displayed.</param>
    public void ShowCountdown(string countdownValue) {
        countdownText.text = countdownValue;
        countdownText.DOComplete();
        countdownObject.SetActive(true);
        countdownText.color = countdownText.color.WithA(0);
        countdownText.fontSize = 0;
        DOTween.To(() => countdownText.fontSize, x => countdownText.fontSize = x, 180, 0.2f).OnComplete(() => {
            DOTween.To(() => countdownText.fontSize, x => countdownText.fontSize = x, 80, 0.6f);
        });
        countdownText.DOFade(1, 0.2f).OnComplete(() => {
            countdownText.DOFade(0, 0.6f).OnComplete(() => {
                countdownObject.SetActive(false);
            });
        });
    }
    #endregion

    #region Basic information
    /// <summary>
    /// Updates the race time displayed in the HUD to the given value.
    /// </summary>
    /// <param name="timeInSeconds">Current race time in seconds.</param>
    public void UpdateTime(float timeInSeconds) {
        timeText.text = Utils.FormatTime(timeInSeconds);
    }

    /// <summary>
    /// Updates the current speed and altitude values which are displayed in the HUD.
    /// </summary>
    /// <param name="speed">Current speed.</param>
    /// <param name="altitude">Current altitude.</param>
    public void UpdatePlayerState(float speed, float altitude) {
        speedText.text = Math.Round(speed, 1).ToString();
        altitudeText.text = Math.Round(altitude, 1).ToString();
    }
    #endregion

    #region Race state
    /// <summary>
    /// Initializes labels displaying total number of checkpoints and hoops in the track and number of checkpoints and hoops passed.
    /// </summary>
    /// <param name="checkpointsTotal">Total number of checkpoints in the track.</param>
    /// <param name="hoopsTotal">Total number of hoops in the track (excluding checkpoints).</param>
    public void InitializeCheckpointsAndHoops(int checkpointsTotal, int hoopsTotal) {
        checkpointsPassedText.text = "0";
        checkpointsTotalText.text = checkpointsTotal.ToString();
        hoopsPassedText.text = "0";
        hoopsTotalText.text = hoopsTotal.ToString();
        hoopsMissedText.text = "";
    }

    /// <summary>
    /// Updates number of passed checkpoints displayed in the HUD.
    /// </summary>
    /// <param name="checkpointsPassed">Current number of checkpoints passed.</param>
    public void UpdatePassedCheckpoints(int checkpointsPassed) {
        checkpointsPassedText.text = checkpointsPassed.ToString();
    }

    /// <summary>
    /// Updates number of passed hoops displayed in the HUD.
    /// </summary>
    /// <param name="hoopsPassed">Current number of hoops passed.</param>
    public void UpdatePassedHoops(int hoopsPassed) {
        hoopsPassedText.text = hoopsPassed.ToString();
    }

    /// <summary>
    /// Updates number of missed hoops displayed in the HUD.
    /// </summary>
    /// <param name="hoopsMissed">Current number of hoops missed.</param>
    public void UpdateMissedHoops(int hoopsMissed) {
        if (hoopsMissed != 0)
            hoopsMissedText.text = $"(-{hoopsMissed})";
        // Briefly scale the text up and back down
        EnlargeTextBriefly(hoopsMissedText, 1.5f, 0.5f);
    }

    /// <summary>
    /// Updates time penalization displayed in the HUD.
    /// </summary>
    /// <param name="penalizationInSeconds">Current time penalization in seconds.</param>
    public void UpdateTimePenalization(int penalizationInSeconds) {
        timePenalizationText.text = $"(+{penalizationInSeconds} s)";
        // Briefly scale the text up and back down
        EnlargeTextBriefly(timePenalizationText, 1.5f, 0.5f);
        // Highlight the race time text - make it bigger and red
        EnlargeTextBriefly(timeText, 1.5f, 0.5f);
        ChangeTextColorBriefly(timeText, Color.red, 0.5f);
    }

    /// <summary>
    /// Updates the current place in race displayed in the HUD to the given value.
    /// </summary>
    /// <param name="place">The player's current place in race.</param>
    public void UpdatePlace(int place) {
        placeText.text = place.ToString();
        // Change color according to the place
        placeBackground.color = ColorPalette.Instance.GetLeaderboardPlaceColor(place);
    }
    #endregion

    #region Screen overlays
    /// <summary>
    /// Briefly colors the screen with the given color to show a visual warning for something.
    /// </summary>
    /// <param name="color">Color used to color the screen.</param>
    public void FlashScreenColor(Color color) {
        flashingColorOverlay.color = color;
        flashingColorOverlay.gameObject.TweenAwareEnable();
    }

    /// <summary>
    /// Shows warning that a checkpoint has been missed.
    /// </summary>
    public void ShowMissedCheckpointWarning() {
        missedCheckpointWarning.TweenAwareEnable();
    }

    private bool wrongDirectionWarningVisible = false;
    /// <summary>
    /// Shows warning that the player is flying in a wrong direction.
    /// </summary>
    public void ShowWrongDirectionWarning() {
        if (wrongDirectionWarningVisible) return;
        wrongDirectionWarningVisible = true;
        // Start tweening the wrong direction warning in a loop
        wrongDirectionWarning.loop = true;
        if (!wrongDirectionWarning.IsPlaying())
            wrongDirectionWarning.DoTween();
    }

    /// <summary>
    /// Hides warning that the player is flying in a wrong direction.
    /// </summary>
    public void HideWrongDirectionWarning() {
        if (!wrongDirectionWarningVisible) return;
        wrongDirectionWarningVisible = false;
        // Stop tweening the wrong direction warning - disable loop and make sure the last iteration finishes
        wrongDirectionWarning.loop = false;
        wrongDirectionWarning.DoTween();
    }

    /// <summary>
    /// Shows or hides warning that the player is flying in a wrong direction based on the given value.
    /// </summary>
    /// <param name="show"><c>true</c> to show the wrong direction warning, <c>false</c> to hide it.</param>
    public void ShowHideWrongDirectionWarning(bool show) {
        wrongDirectionWarning.loop = show; // tweened in a loop
        if (show && !wrongDirectionWarning.IsPlaying())
            wrongDirectionWarning.DoTween();
        // If it should be hidden, loop is stopped and the tween ends naturally in time (making sure it is in a consistent state afterwards)
    }
    #endregion

    /// <summary>
    /// Initializes all HUD elements and shows elements which are visible only during the race (e.g. race time, place, equipped spells).
    /// </summary>
    public void StartRace() {
        // Initialize all elements
        ResetRaceState();
        // Show elements visible only during the race
        timeObject.SetActive(true);
        placeObject.SetActive(true);
        spellsUI.Initialize(playerRaceState.gameObject); // initialize and show
    }

    // Resets displayed values
    private void ResetRaceState() {
        UpdatePlace(1);

        checkpointsPassedText.text = "0";
        hoopsPassedText.text = "0";
        hoopsMissedText.text = "";
        timePenalizationText.text = "";
    }

    // Registers callbacks for all events to which it needs to react
    private void RegisterCallbacks() {
        if (playerRaceState != null) {
            playerRaceState.onPlaceChanged += UpdatePlace;
            playerRaceState.onPassedCheckpointsChanged += UpdatePassedCheckpoints;
            playerRaceState.onPassedHoopsChanged += UpdatePassedHoops;
            playerRaceState.onMissedHoopsChanged += UpdateMissedHoops;
            playerRaceState.onTimePenalizationChanged += UpdateTimePenalization;
            playerRaceState.onWrongDirectionChanged += ShowHideWrongDirectionWarning;
        }
    }

    // Unregisters all registered callbacks
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

    // Briefly increases font size of the given TextMesh Pro
    private void EnlargeTextBriefly(TextMeshProUGUI text, float sizeMultiplier, float duration) {
        float originalSize = text.fontSize;
        float biggerSize = originalSize * sizeMultiplier;
        DOTween.To(() => text.fontSize, x => text.fontSize = x, biggerSize, duration/2f)
            .OnComplete(() => DOTween.To(() => text.fontSize, x => text.fontSize = x, originalSize, duration / 2f));
    }

    // Briefly changes color of the given TextMesh Pro
    private void ChangeTextColorBriefly(TextMeshProUGUI text, Color color, float duration) {
        Color originalColor = text.color;
        text.DOColor(color, duration / 2f).OnComplete(() => text.DOColor(originalColor, duration / 2f));
    }

    private void Start() {
        // Initialize everything
        ResetRaceState();
        // Find the player race state
        playerRaceState = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterRaceState>("Player");
        // Register necessary callbacks
        RegisterCallbacks();
        // Hide all elements which should not be visible during training
        timeObject.SetActive(false);
        placeObject.SetActive(false);
        spellsUI.gameObject.SetActive(false);
    }

	private void OnDestroy() {
        UnregisterCallbacks();
	}
}
