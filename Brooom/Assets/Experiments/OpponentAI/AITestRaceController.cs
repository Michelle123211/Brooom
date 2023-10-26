using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AITestRaceController : RaceController {

    public override void StartRace() {
        raceTime = 0;
        // Activate the hoops and finish line
        for (int i = 0; i < level.track.Count; i++) {
            level.track[i].assignedHoop.Activate(i);
        }
        level.finish.Activate();
        // Actually start the race
        State = RaceState.RaceInProgress;
        // Enable racer actions
        foreach (var racer in racers) {
            racer.characterController.EnableActions();
            racer.state.SetRaceStarted(true);
        }
    }

    public override void EndRace() {
        State = RaceState.RaceFinished;
        // Disable racers' actions make them brake
        foreach (var racer in racers) {
            racer.characterController.DisableActions(CharacterMovementController.StopMethod.BrakeStop);
        }
        // Recompute racers' places
        CompleteOpponentState();
        ComputeRacerPlaces();
        // Show the race results
        ShowRaceResults();
    }

	public override void GiveUpRace() {
        // Change scene
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    protected override void ShowRaceResults() {
        // Collect results from individual racers
        RaceResultData[] results = new RaceResultData[racers.Count];
        foreach (var racer in racers) {
            float time = racer.state.finishTime + racer.state.timePenalization;
            // If the total time is too big, make them DNF instead
            results[racer.state.place - 1] = new RaceResultData {
                name = racer.characterName,
                time = time,
                coinsReward = 0
            };
        }
        raceResults.SetResultsTable(results);
        // Display everything
        raceResults.gameObject.TweenAwareEnable();
    }

	private void Update() {
        if (State == RaceState.RaceInProgress)
            raceTime += Time.deltaTime;
	}

	private void Start() {
        InitializeRelatedObjects();
        GenerateLevel();
        InitializeRacers();
        // Hide HUD
        raceHUD.gameObject.SetActive(false);
    }

	private void Awake() {
        Instance = this;
	}

	private void OnDestroy() {
        Instance = null;
	}
}