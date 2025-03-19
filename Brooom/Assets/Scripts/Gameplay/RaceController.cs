using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.Playables;

public class RaceController : MonoBehaviourLongInitialization {

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
    public int missedHoopPenalization = 10;

    [Header("Coins")]
    [Tooltip("Curve describing first place reward depending on the track difficulty.")]
    public AnimationCurve firstPlaceReward;
    [Tooltip("Minimum and maximum reward for the first place.")]
    public Vector2Int firstPlaceRewardRange = new Vector2Int(100, 5000);

    [Header("Regions")]
    public List<LevelRegionType> defaultRegions;
    public List<RegionUnlockTutorialStage> regionsUnlockedByTutorial;
    public List<RegionUnlockValue> regionsUnlockedByEndurance;
    public List<RegionUnlockValue> regionsUnlockedByAltitude;

    // Level - to get access to track points and record racers' position within the track
    public LevelRepresentation level;
    protected float levelDifficulty; // number between 0 and 1
    // Current time elapsed in the race
    [HideInInspector] public float raceTime = 0;

    public List<RacerRepresentation> racers;
    public RacerRepresentation playerRacer;

    // Related objects
    protected RaceUI raceHUD;
    protected RaceResultsUI raceResults;
    protected LevelGenerationPipeline levelGenerator;
    protected StatsComputer statsComputer;

    protected Transform bonusParent;
    protected Transform opponentParent;

    public RaceState State { get; protected set; } = RaceState.Training; // distinguish between training and race

    private int restartCountInTraining = 0;


    // Called when entering the race
    public virtual void StartRace() {
        Messaging.SendMessage("TrainingEnded", restartCountInTraining);
        // Start animation sequence
        StartCoroutine(PlayRaceStartSequence());
	}

    // Called when player finishes the race
    public virtual void EndRace() {
        GamePause.DisableGamePause();
        // Start animation sequence
        StartCoroutine(PlayRaceEndSequence());
        // Note down visited regions
        DetectVisitedRegions();
	}

    public virtual void GiveUpRace() {
        // Decrease all stats
        statsComputer.LowerAllStatsOnRaceGivenUp();
		// Send message
		Messaging.SendMessage("RaceGivenUp");
		// Change scene
		SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    public void OnRacerFinished(CharacterRaceState racerState) {
        foreach (var racer in racers) {
            if (racer.state == racerState) {
                racer.characterController.DisableActions(CharacterMovementController.StopMethod.BrakeStop);
                racer.characterController.gameObject.GetComponent<FaceAnimationsController>()?.StartSmiling();
                // TODO: Start waving animation
                racer.spellInput.DisableSpellCasting();
                break;
            }
        }
    }

    public int CompareRacers(RacerRepresentation x, RacerRepresentation y) {
        // If both have finish time, the better one should be first
        if (x.state.HasFinished && y.state.HasFinished)
            return (x.state.finishTime + x.state.timePenalization).CompareTo(y.state.finishTime + y.state.timePenalization);
        // The one having finish time should be first
        if (x.state.HasFinished && !y.state.HasFinished) return -1; // x is first
        if (!x.state.HasFinished && y.state.HasFinished) return 1; // y is first
        // The one having next hoop with higher index should be first
        if (x.state.trackPointToPassNext != y.state.trackPointToPassNext)
            return y.state.trackPointToPassNext.CompareTo(x.state.trackPointToPassNext);
        // The one closer to the hoop or finish should be first
        float xDistance, yDistance;
        if (x.state.trackPointToPassNext >= level.track.Count) { // the finish line is next
            // Get distance on the shortest line to the finish
            xDistance = Mathf.Abs(level.finish.transform.InverseTransformPoint(x.state.transform.position).z);
            yDistance = Mathf.Abs(level.finish.transform.InverseTransformPoint(y.state.transform.position).z);
        } else {
            // Get distance to the next hoop
            xDistance = Vector3.Distance(x.state.transform.position, level.track[x.state.trackPointToPassNext].position);
            yDistance = Vector3.Distance(y.state.transform.position, level.track[y.state.trackPointToPassNext].position);
        }
        return xDistance.CompareTo(yDistance);
    }

    private void StartTraining() {
        State = RaceState.Training;
        restartCountInTraining = 0;
		// Hide bonuses
		bonusParent = levelGenerator.transform.Find("Bonus");
        if (bonusParent != null) bonusParent.gameObject.SetActive(false);
        // Hide opponents
        opponentParent = levelGenerator.transform.Find("Opponents");
        if (opponentParent != null) opponentParent.gameObject.SetActive(false);
        // Place the player + enable actions
        playerRacer.characterController.ResetPosition(level.playerStartPosition);
        playerRacer.characterController.EnableActions();
    }

    private IEnumerator PlayRaceStartSequence() {
        double remainingDuration = 0;
        // Start playing the sequence
        PlayableDirector startCutscene = Cutscenes.Instance.PlayCutscene("RaceStart");
        if (startCutscene != null) {
            remainingDuration = startCutscene.duration;
            yield return new WaitForSeconds(0.18f); // allow the screen to fade out first
            remainingDuration -= startCutscene.time;
        }
        // Place the player + disable actions
        playerRacer.characterController.ResetPosition(level.playerStartPosition);
        playerRacer.characterController.GetComponent<PlayerCameraController>().ResetCameras();
        playerRacer.characterController.DisableActions();
        // Prepare HUD
        raceTime = 0;
        raceHUD.UpdateTime(raceTime + playerRacer.state.timePenalization);
        raceHUD.StartRace(); // also resets e.g. hoop progress and mana
        // Show bonuses
        if (bonusParent != null) bonusParent.gameObject.SetActive(true);
        // Activate the hoops and finish line
        for (int i = 0; i < level.track.Count; i++) {
            level.track[i].assignedHoop.Activate(i);
        }
        level.finish.Activate();
        // Highlight the first hoop
        level.track[0].assignedHoop.StartHighlighting();
        // Show the opponents
        if (opponentParent != null) opponentParent.gameObject.SetActive(true);
        // Wait until the end of the sequence
        yield return new WaitForSeconds((float)remainingDuration);
        // Reset camera (front camera, default rotation)
        playerRacer.characterController.GetComponent<PlayerCameraController>().TweenToResetView();
        // Show the race countdown
        for (int i = 3; i > 0; i--) {
            raceHUD.ShowCountdown(i.ToString());
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.CountdownRace);
            yield return new WaitForSeconds(1);
        }
        raceHUD.ShowCountdown("START!");
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.RaceStarted);
        // Actually start the race
        State = RaceState.RaceInProgress;
		// Send message
		Messaging.SendMessage("RaceStarted", racers.Count);
		// Enable player and enable opponents actions
		foreach (var racer in racers) {
            racer.characterController.EnableActions();
            racer.state.SetRaceStarted(true);
            racer.spellInput.TryEnableSpellCasting();
        }
        // Start computing player stats
        statsComputer.StartComputingStats();
    }

    private IEnumerator PlayRaceEndSequence() {
        State = RaceState.RaceFinished;
        // Disable player actions make them brake
        playerRacer.characterController.DisableActions(CharacterMovementController.StopMethod.BrakeStop);
        playerRacer.spellInput.DisableSpellCasting();
        // Start playing the sequence
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.RaceFinished);
        PlayableDirector endCutscene = Cutscenes.Instance.PlayCutscene("RaceEnd");
        double remainingDuration = 0;
        if (endCutscene != null) {
            remainingDuration = endCutscene.duration;
        }
        // Finish computing player statistics
        statsComputer.StopComputing();
        // Wait until the end of the sequence
        yield return new WaitForSeconds((float)remainingDuration);
        // Recompute racers' places
        CompleteOpponentState();
        ComputeRacerPlaces();
        int[] coinRewards = ComputeCoinRewards();
        // Update player statistics in PlayerState - computation depends on the player's place
        statsComputer.UpdateStats();
        // Update player's coins account - TODO: consider coins penalization (e.g. for exposing magic)
        int playerReward = coinRewards[playerRacer.state.place - 1];
        if (playerReward > 0) PlayerState.Instance.ChangeCoinsAmount(playerReward);
        // Send message
        Messaging.SendMessage("RaceFinished", playerRacer.state.place);
		// Show the race results
		ShowRaceResults(coinRewards);
    }

    protected void CompleteOpponentState() {
        // Handle case when the opponent did not finish
        foreach (var racer in racers) {
            if (!racer.state.HasFinished) {
                // Compute the finish time proportionaly to the number of remaining hoops
                float timeForOneHoop = raceTime;
                if (racer.state.trackPointToPassNext > 0)
                    timeForOneHoop = racer.state.lastHoopTime / racer.state.trackPointToPassNext;
                racer.state.finishTime = timeForOneHoop * level.track.Count;
                // Compute time penalization for likely missed hoops
                if (racer.state.trackPointToPassNext > 0) {
                    float missedHoopsPercentage = racer.state.hoopsMissed / racer.state.trackPointToPassNext;
                    float missedHoopsInFuture = Mathf.RoundToInt((level.track.Count - racer.state.trackPointToPassNext) * missedHoopsPercentage);
                    racer.state.timePenalization += (missedHoopsInFuture * missedHoopPenalization);
                }
                // If the final time is lower than the player's one, increase it
                float timeDifference = (playerRacer.state.finishTime + playerRacer.state.timePenalization) - (racer.state.finishTime + racer.state.timePenalization);
                if (timeDifference > 0) {
                    racer.state.finishTime += (timeDifference + UnityEngine.Random.Range(5f, 15f));
                }
            }
        }
    }

    protected void ComputeRacerPlaces() {
        // Sort the racers according to their place
        racers.Sort((x, y) => CompareRacers(x, y));
        // Let them know what their place is
        for (int i = 0; i < racers.Count; i++) {
            racers[i].state.UpdatePlace(i + 1);
        }
    }

    protected virtual void ShowRaceResults(int[] coinRewards) {
        // Collect results from individual racers
        RaceResultData[] results = new RaceResultData[racers.Count];
        foreach (var racer in racers) {
            float time = racer.state.finishTime + racer.state.timePenalization;
            // If the total time is too big, make them DNF instead
            if (time > (playerRacer.state.finishTime + playerRacer.state.timePenalization) * 6)
                time = -1;
            results[racer.state.place - 1] = new RaceResultData {
                name = racer.characterName,
                color = racer.state.assignedColor,
                time = time,
                timePenalization = racer.state.timePenalization,
                coinsReward = time > 0 ? coinRewards[racer.state.place - 1] : 0 };
                // TODO: Set coinsPenalization
        }
        raceResults.SetResultsTable(results, playerRacer.state.place);
        // Display everything
        raceResults.gameObject.TweenAwareEnable();
    }

    protected int[] ComputeCoinRewards() {
        int[] result = new int[racers.Count];
        // Compute rewards for individual places
        float firstPlaceRaw = firstPlaceReward.Evaluate(levelDifficulty) * (firstPlaceRewardRange.y - firstPlaceRewardRange.x) + firstPlaceRewardRange.x;
        result[0] = Mathf.FloorToInt(firstPlaceRaw / 10) * 10; // floor to tens
        result[1] = Mathf.FloorToInt((result[0] * 0.6f) / 10) * 10; // 60 % of the first place reward, floored to tens
        result[2] = Mathf.FloorToInt((result[0] * 0.2f) / 10) * 10; // 20 % of the first place reward, floored to tens
        return result;
    }

    protected void InitializeRelatedObjects() {
        raceHUD = FindObjectOfType<RaceUI>();
        raceResults = Utils.FindObject<RaceResultsUI>()[0];
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
        statsComputer = GetComponent<StatsComputer>();
    }

    protected IEnumerator GenerateLevel() {
        // Generate level (terrain + track)
        SetLevelGeneratorParameters();
        yield return levelGenerator.GenerateLevel();
        level = levelGenerator.Level;
        levelDifficulty = ComputeLevelDifficulty();
    }

    protected void InitializeRacers() {
        // Get references to the characters
        List<CharacterMovementController> characters = Utils.FindObject<CharacterMovementController>();
        racers = new List<RacerRepresentation>();
        for (int i = 0; i < characters.Count; i++) {
            RacerRepresentation racer = new RacerRepresentation {
                characterName = characters[i].GetComponentInChildren<CharacterAppearance>().characterName,
                characterController = characters[i],
                state = characters[i].GetComponent<CharacterRaceState>(),
                spellInput = characters[i].GetComponentInChildren<SpellInput>()
            };
            racer.state.Initialize(level.track.Count);
            racers.Add(racer);
            if (racer.characterController.isPlayer) playerRacer = racer;
            racer.characterController.DisableActions();
            racer.spellInput.DisableSpellCasting();
        }
    }

	private void OnDestroy() {
        Instance = null;
        // Unregister callbacks on player race state changes
        playerRacer.state.onHoopAdvance -= HighlightNextHoop;
        playerRacer.state.onCheckpointMissed -= ReactOnCheckpointMissed;
        playerRacer.state.onHoopMissed -= ReactOnHoopMissed;
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
        // ... default available regions
        foreach (var region in defaultRegions)
            PlayerState.Instance.SetRegionAvailability(region, true);
        // ... available regions from tutorial
        foreach (var regionFromTutorial in regionsUnlockedByTutorial)
            PlayerState.Instance.SetRegionAvailability(
                regionFromTutorial.region,
                Tutorial.Instance.CurrentStage >= regionFromTutorial.tutorialStage ? true : false
            );
        // ... available regions from Endurance
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
        TrackPointsGenerationRandomWalk trackGenerator = levelGenerator.GetComponent<TrackPointsGenerationRandomWalk>();
        trackGenerator.numberOfCheckpoints = numOfCheckpoints;
        trackGenerator.maxDirectionChangeAngle = directionChange;
        trackGenerator.distanceRange = distanceRange;
        levelGenerator.GetComponent<TrackObjectsPlacement>().hoopScale = hoopScale;
        levelGenerator.GetComponent<MaximumAngleCorrection>().maxAngle = directionChange.x;
        levelGenerator.GetComponent<OpponentsGeneration>().opponentsCount = 5; // TODO: Change this number if necessary, in the future
    }

    private float ComputeLevelDifficulty() {
        // Weighted average of current player stats which were used for level generation
        // - weight 3: dexterity // the most important
        // - weight 2: precision
        // - weight 1: endurance, speed // both combined represent length of the track
        float weightedAverage = (
            3 * (PlayerState.Instance.CurrentStats.dexterity) +
            2 * (PlayerState.Instance.CurrentStats.precision) +
            1 * (PlayerState.Instance.CurrentStats.endurance + PlayerState.Instance.CurrentStats.speed))
            / 10f;
        // Mapped from (0, 100) to (0, 1)
        return weightedAverage / 100;
    }

    // Highlights the next hoop for the player
    private void HighlightNextHoop() {
        int nextHoopIndex = playerRacer.state.trackPointToPassNext;
        level.track[nextHoopIndex - 1].assignedHoop.StopHighlighting();
        if (nextHoopIndex < level.track.Count)
            level.track[nextHoopIndex].assignedHoop.StartHighlighting();
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

    private void DetectVisitedRegions() {
        // For each track point, set its region as visited
        for (int i = 0; i < level.track.Count; i++) {
            // Only passed hoops are considered
            if (!playerRacer.state.hoopsPassedArray[i]) continue;
            // Track region (if any)
            TrackPoint trackPoint = level.track[i];
			if (trackPoint.trackRegion != LevelRegionType.NONE)
				PlayerState.Instance.SetRegionVisited(trackPoint.trackRegion);
			// Terrain region
			Vector2Int gridCoords = trackPoint.gridCoords;
			PlayerState.Instance.SetRegionVisited(level.terrain[gridCoords.x, gridCoords.y].region);
		}
    }

	protected override void PrepareForInitialization_ReplacingAwake() {
        Instance = this;
    }

	protected override void PrepareForInitialization_ReplacingStart() {
        InitializeRelatedObjects();
    }

	protected override IEnumerator InitializeAfterPreparation() {
        yield return GenerateLevel();
        InitializeRacers();
        // Initialize HUD
        int checkpointsTotal = 0, hoopsTotal = 0;
        foreach (var trackPoint in level.track) {
            if (trackPoint.isCheckpoint) checkpointsTotal++;
            else hoopsTotal++;
        }
        raceHUD.InitializeCheckpointsAndHoops(checkpointsTotal, hoopsTotal);
        // Register callbacks on player race state changes
        playerRacer.state.onHoopAdvance += HighlightNextHoop;
        playerRacer.state.onCheckpointMissed += ReactOnCheckpointMissed;
        playerRacer.state.onHoopMissed += ReactOnHoopMissed;
        // Initialize training
        StartTraining();
    }

	protected override void UpdateAfterInitialization() {
        // Update state
        switch (State) {
            case RaceState.Training:
                // Handle restart
                if (GamePause.PauseState == GamePauseState.Running && InputManager.Instance.GetBoolValue("Restart")) {
                    playerRacer.characterController.ResetPosition(level.playerStartPosition);
                    restartCountInTraining++;
                }
                break;
            case RaceState.RaceInProgress:
                // Update time from the start of the race
                raceTime += Time.deltaTime;
                raceHUD.UpdateTime(raceTime + playerRacer.state.timePenalization);
                // Update racers' place
                ComputeRacerPlaces();
                break;
            case RaceState.RaceFinished:
                // Update time from the start of the race
                raceTime += Time.deltaTime;
                break;
        }
        // Update UI
        raceHUD.UpdatePlayerState(
            playerRacer.characterController.GetCurrentSpeed(),
            playerRacer.characterController.GetCurrentAltitude());
    }
}

public enum RaceState {
    Training,
    RaceInProgress,
    RaceFinished
}

[System.Serializable]
public class RegionUnlockValue {
    [Tooltip("Region unlocked by a specific value of some stat (e.g. max altitude, endurance).")]
    public LevelRegionType region;
    [Tooltip("If the stat is greater then this value, the region becomes available.")]
    public int minValue;
}

[System.Serializable]
public class RegionUnlockTutorialStage {
    [Tooltip("Region unlocked by a specific tutorial stage.")]
    public LevelRegionType region;
    [Tooltip("If the player gets into this tutorial stage, the region becomes available.")]
    public TutorialStage tutorialStage;
}

// Everything the RaceController needs for a character
public class RacerRepresentation {
    public string characterName = string.Empty;
    public CharacterMovementController characterController;
    public CharacterRaceState state;
    public SpellInput spellInput;
}