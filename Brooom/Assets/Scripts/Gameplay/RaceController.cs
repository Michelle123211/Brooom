using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.Playables;


/// <summary>
/// This class handles progress of race, i.e. making sure the race is started and finished, race results are displayed etc.
/// It extends <c>RaceControllerBase</c> to add coin rewards based on place, training stage and stats computation.
/// </summary>
public class RaceController : RaceControllerBase {

    [Header("Coins")]
    [Tooltip("Curve describing first place reward depending on the track difficulty.")]
    public AnimationCurve firstPlaceReward;
    [Tooltip("Minimum and maximum reward for the first place.")]
    public Vector2Int firstPlaceRewardRange = new Vector2Int(100, 4000);

    [Tooltip("Whether First Race tutorial is running during this race (it may affect behaviour).")]
    public bool isInTutorial = false;

    // Related objects
    /// <summary>Responsible for computing new stats values based on performance during the race.</summary>
    protected StatsComputer statsComputer;

    private int restartCountInTraining = 0;

    private bool isTrainingSkipped = false; // whether the training is already being skipped

    /// <summary>
    /// <inheritdoc/>
    /// It sends a message that training has ended.
    /// </summary>
	protected override void BeforeRaceStartSequence() {
        Messaging.SendMessage("TrainingEnded", restartCountInTraining);
    }
    /// <summary>
    /// <inheritdoc/>
    /// It ensures data for stats computation is being collected.
    /// </summary>
	protected override void AfterRaceStartSequence() {
        // Start computing player stats
        statsComputer.StartComputingStats();
    }

    /// <summary>
    /// <inheritdoc/>
    /// It notes down any newly visited regions and stops collecting data for stats computation.
    /// </summary>
	protected override void BeforeRaceEndSequence() {
        // Note down visited regions (but not during First Race tutorial)
        if (!isInTutorial)
            DetectVisitedRegions();
        // Finish computing player statistics
        statsComputer.StopComputing();
    }

    /// <summary>
    /// <inheritdoc/>
    /// It updates the player's stats values.
    /// </summary>
	protected override void AfterRaceEndSequence() {
        // Update player statistics in PlayerState - computation depends on the player's place
        statsComputer.UpdateStats();
    }

    /// <summary>
    /// Decreases all stats by a small amount and goes back to the Main Menu.
    /// </summary>
	protected override void OnRaceGivenUp() {
        // Decrease all stats
        statsComputer.LowerAllStatsOnRaceGivenUp();
        // Change scene
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    /// <inheritdoc/>
    protected override int[] ComputeCoinRewards() {
        int[] result = new int[racers.Count];
        // Compute rewards for individual places
        float firstPlaceRaw = firstPlaceReward.Evaluate(levelDifficulty) * (firstPlaceRewardRange.y - firstPlaceRewardRange.x) + firstPlaceRewardRange.x;
        result[0] = Mathf.FloorToInt(firstPlaceRaw / 10) * 10; // floor to tens
        result[1] = Mathf.FloorToInt((result[0] * 0.7f) / 10) * 10; // 70 % of the first place reward, floored to tens
        result[2] = Mathf.FloorToInt((result[0] * 0.4f) / 10) * 10; // 40 % of the first place reward, floored to tens
        return result;
    }

	protected override void InitializeAnythingRelated() {
        statsComputer = GetComponent<StatsComputer>();
    }

    // Prepares everything necessary for the training stage
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
        playerRacer.characterController.ResetPosition(Level.playerStartPosition);
        if (Tutorial.Instance.CurrentStage != TutorialStage.FirstRace) // otherwise it is driven by tutorial
            playerRacer.characterController.EnableActions();
    }

    // Notes down all visited regions
    private void DetectVisitedRegions() {
        // For each track point, set its region as visited
        for (int i = 0; i < Level.Track.Count; i++) {
            // Only passed hoops are considered
            if (!playerRacer.state.hoopsPassedArray[i]) continue;
            // Track region (if any)
            TrackPoint trackPoint = Level.Track[i];
			if (trackPoint.trackRegion != LevelRegionType.NONE)
				PlayerState.Instance.SetRegionVisited(trackPoint.trackRegion);
			// Terrain region
			Vector2Int gridCoords = trackPoint.gridCoords;
			PlayerState.Instance.SetRegionVisited(Level.Terrain[gridCoords.x, gridCoords.y].region);
		}
    }

	protected override void AfterInitialization() {
        // Initialize training
        StartTraining();
    }

	protected override void Update_Training() {
		base.Update_Training();
        // Handle skipping training - remove starting zone and start race immediately
        if (SettingsUI.skipTraining && !isTrainingSkipped && !isInTutorial) { // if not being skipped already and not in tutorial
            isTrainingSkipped = true; // prevent this from triggering again
            Destroy(FindObjectOfType<StartingZone>().transform.parent.gameObject);
            Invoke(nameof(StartRace), 3); // after 3 s to allow information about new region to appear
        }
        // Handle restart
        if (GamePause.PauseState == GamePauseState.Running && InputManager.Instance.GetBoolValue("Restart")) {
            Analytics.Instance.LogEvent(AnalyticsCategory.Race, "Position reset during training.");
            playerRacer.characterController.ResetPosition(Level.playerStartPosition);
            restartCountInTraining++;
        }
    }

	protected override void OnDestroy_Derived() {
	}

}