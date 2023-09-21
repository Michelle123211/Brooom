using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class RaceController : MonoBehaviour {

    // Simple singleton
    public static RaceController Instance;

    [Header("Level length (Endurance)")]
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 0.")]
    public int initialNumberOfCheckpoints = 4;
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 100.")]
    public int finalNumberOfCheckpoints = 20;

    [Header("Direction change (Dexterity)")]
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 0.")]
    public Vector2 initialDirectionChange = new Vector2(10, 20);
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 100.")]
    public Vector2 finalDirectionChange = new Vector2(30, 45);

    [Header("Hoop scale (Precision)")]
    [Tooltip("Scale of hoops when the player's Precision stat is 0.")]
    public float initialHoopScale = 1f;
    [Tooltip("Scale of hoops when the player's Precision stat is 100.")]
    public float finalHoopScale = 0.4f;

    [Header("Hoop distance (Speed)")]
    [Tooltip("The approximate minimum and maximum distance between two hoops when the player's Speed stat is 0.")]
    public Vector2 initialHoopDistanceRange = new Vector2(40, 50);
    [Tooltip("The approximate minimum and maximum distance between two hoops when the player's Speed stat is 100.")]
    public Vector2 finalHoopDistanceRange = new Vector2(80, 100);

    [Header("Penalizations")]
    [Tooltip("How many seconds are added to the time when player misses a hoop.")]
    public int missedHoopPenalization = 5;

    [Header("Regions")]
    public List<LevelRegionType> defaultRegions;
    public List<RegionUnlockValue> regionsUnlockedByEndurance;
    public List<RegionUnlockValue> regionsUnlockedByAltitude;


    // Related objects
    private RaceUI raceHUD;
    private LevelGenerationPipeline levelGenerator;
    private TrackPointsGenerationRandomWalk trackGenerator;
    private TrackObjectsPlacement hoopsPlacement;
    private MaximumAngleCorrection angleCorrection;
    private OpponentsGeneration opponentsGenerator;


    // Level - to get access to track points and record racers' position within the track
    public LevelRepresentation level;

    private Transform bonusParent;
    private Transform opponentParent;

    private List<RacerRepresentation> racers;
    private int playerIndex = 0;


    private bool raceStarted = false; // distinguish between training and race
    private float raceTime = 0;


    // Called when entering the race
    public void StartRace() {
        // TODO: Add everything related to the start of the race
        raceStarted = true;
        raceTime = 0;
        raceHUD.StartRace(); // also resets e.g. hoop progress and mana
        // Show bonuses
        if (bonusParent != null) bonusParent.gameObject.SetActive(true);
        // Activate the hoops
        for (int i = 0; i < level.track.Count; i++) {
            level.track[i].assignedHoop.Activate(i);
        }
        // Highlight the first hoop
        level.track[0].assignedHoop.StartHighlighting();
        // Place the player + disable actions
        racers[playerIndex].characterController.ResetPosition(level.playerStartPosition);
        racers[playerIndex].characterController.ActionsEnabled = false;
        // Show the opponents
        if (opponentParent != null) opponentParent.gameObject.SetActive(true);
        // TODO: Start animation sequence
        // TODO: At the end of the sequence, show the race countdown
        // TODO: At the end of the countdown, enable player actions and enable opponents actions
        foreach (var racer in racers) {
            racer.characterController.ActionsEnabled = true;
            racer.state.SetRaceStarted(true);
        }
    }

    private void StartTraining() {
        // Hide bonuses
        bonusParent = levelGenerator.transform.Find("Bonus");
        if (bonusParent != null) bonusParent.gameObject.SetActive(false);
        // Hide opponents
        opponentParent = levelGenerator.transform.Find("Opponents");
        if (opponentParent != null) opponentParent.gameObject.SetActive(false);
        // Place the player + enable actions
        racers[playerIndex].characterController.ResetPosition(level.playerStartPosition);
        racers[playerIndex].characterController.ActionsEnabled = true;
    }

    void Start()
    {
        raceHUD = FindObjectOfType<RaceUI>();
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
        trackGenerator = levelGenerator.GetComponent<TrackPointsGenerationRandomWalk>();
        hoopsPlacement = levelGenerator.GetComponent<TrackObjectsPlacement>();
        angleCorrection = levelGenerator.GetComponent<MaximumAngleCorrection>();
        opponentsGenerator = levelGenerator.GetComponent<OpponentsGeneration>();
        // Generate level (terrain + track)
        SetLevelGeneratorParameters();
        level = levelGenerator.GenerateLevel();
        // Get references to the characters
        List<CharacterMovementController> characters = Utils.FindObject<CharacterMovementController>();
        racers = new List<RacerRepresentation>();
        for (int i = 0; i < characters.Count; i++) {
            RacerRepresentation racer = new RacerRepresentation {
                characterName = characters[i].GetComponentInChildren<CharacterAppearance>().characterName,
                characterController = characters[i],
                state = characters[i].GetComponent<CharacterRaceState>()
            };
            racer.state.Initialize(level.track.Count);
            racers.Add(racer);
            if (racer.characterController.isPlayer) playerIndex = i;
        }
        // Initialize HUD
        int checkpointsTotal = 0, hoopsTotal = 0;
        foreach (var trackPoint in level.track) {
            if (trackPoint.isCheckpoint) checkpointsTotal++;
            else hoopsTotal++;
        }
        raceHUD.InitializeCheckpointsAndHoops(checkpointsTotal, hoopsTotal);
        // Register callbacks on player race state changes
        racers[playerIndex].state.onHoopAdvance += HighlightNextHoop;
        racers[playerIndex].state.onCheckpointMissed += ReactOnCheckpointMissed;
        racers[playerIndex].state.onHoopMissed += ReactOnHoopMissed;
        // Initialize training
        StartTraining();
    }

	private void Awake() {
        Instance = this;
	}

	private void OnDestroy() {
        Instance = null;
        // Unregister callbacks on player race state changes
        racers[playerIndex].state.onHoopAdvance -= HighlightNextHoop;
        racers[playerIndex].state.onCheckpointMissed -= ReactOnCheckpointMissed;
        racers[playerIndex].state.onHoopMissed -= ReactOnHoopMissed;
    }

	private void SetLevelGeneratorParameters() {
        // Compute parameters based on player's stats
        // ... number of checkpoints from Endurance
        int numOfCheckpoints = Mathf.RoundToInt(Mathf.Lerp(initialNumberOfCheckpoints, finalNumberOfCheckpoints, PlayerState.Instance.CurrentStats.endurance / 100f));
        // ... maximum direction change from Dexterity
        Vector2 directionChange = Vector2.Lerp(initialDirectionChange, finalDirectionChange, PlayerState.Instance.CurrentStats.dexterity / 100f);
        // ... hoop scale from Precision
        float hoopScale = Mathf.Lerp(initialHoopScale, finalHoopScale, PlayerState.Instance.CurrentStats.precision / 100f);
        // ... distance between adjacent hoops from Speed
        Vector2 distanceRange = Vector2.Lerp(initialHoopDistanceRange, finalHoopDistanceRange, PlayerState.Instance.CurrentStats.speed / 100f);
        // ... available regions from Endurance
        foreach (var region in defaultRegions)
            PlayerState.Instance.regionsAvailability[region] = true;
        foreach (var regionWithValue in regionsUnlockedByEndurance)
            PlayerState.Instance.SetRegionAvailability(
                regionWithValue.region, 
                PlayerState.Instance.CurrentStats.endurance >= regionWithValue.minValue ? true : false
            );
        // ... available regions from max altitude
        foreach (var regionWithValue in regionsUnlockedByAltitude)
            PlayerState.Instance.SetRegionAvailability(
                regionWithValue.region,
                PlayerState.Instance.maxAltitude >= regionWithValue.minValue ? true : false
            );
        // And set them
        levelGenerator.regionsAvailability = PlayerState.Instance.regionsAvailability;
        if (trackGenerator != null) {
            trackGenerator.numberOfCheckpoints = numOfCheckpoints;
            trackGenerator.maxDirectionChangeAngle = directionChange;
            trackGenerator.distanceRange = distanceRange;
        }
        if (hoopsPlacement != null)
            hoopsPlacement.hoopScale = hoopScale;
        if (angleCorrection != null)
            angleCorrection.maxAngle = directionChange.x;
        if (opponentsGenerator != null)
            opponentsGenerator.opponentsCount = 5; // TODO: Change this number if necessary, in the future
    }

    // Highlights the next hoop for the player
    private void HighlightNextHoop() {
        int nextHoopIndex = racers[playerIndex].state.trackPointToPassNext;
        level.track[nextHoopIndex - 1].assignedHoop.StopHighlighting();
        if (nextHoopIndex < level.track.Count)
            level.track[nextHoopIndex].assignedHoop.StartHighlighting();
    }

    private void Update() {
        if (raceStarted) { // during race
            // Time from start of the race
            raceTime += Time.deltaTime;
            raceHUD.UpdateTime(raceTime + racers[playerIndex].state.timePenalization);
            // TODO: Update player's place
            // Compare player's next hoop with other racers, then compare distance to the next hoop
        } else { // during training
            if (InputManager.Instance.GetBoolValue("Restart")) {
                racers[playerIndex].characterController.ResetPosition(level.playerStartPosition);
            }
        }

        // Update UI
        if (raceHUD != null) {
            raceHUD.UpdatePlayerState(
                racers[playerIndex].characterController.GetCurrentSpeed(),
                racers[playerIndex].characterController.GetCurrentAltitude());
        }
    }

    private void ReactOnCheckpointMissed() {
        // Make the screen red briefly
        raceHUD.FlashScreenColor(Color.red);
        // Warn the player that they must return to the checkpoint
        raceHUD.ShowMissedCheckpointWarning();
    }

    private void ReactOnHoopMissed() {
        // Make the screen red briefly
        raceHUD.FlashScreenColor(Color.red);

    }
}

[System.Serializable]
public class RegionUnlockValue {
    [Tooltip("Region unlocked by a specific value of some stat (e.g. max altitude, endurance).")]
    public LevelRegionType region;
    [Tooltip("If the stat is greater then this value, the region becomes available.")]
    public int minValue;
}

// Everything the RaceController needs for a character
public class RacerRepresentation {
    public string characterName = string.Empty;
    public CharacterMovementController characterController;
    public CharacterRaceState state;
}