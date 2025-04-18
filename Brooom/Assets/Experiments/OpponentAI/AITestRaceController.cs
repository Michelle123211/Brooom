using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AITestRaceController : RaceController {

    public override void StartRace() {
        raceTime = 0;
        // Activate the hoops and finish line
        for (int i = 0; i < Level.Track.Count; i++) {
            Level.Track[i].assignedHoop.Activate(i);
        }
        Level.finish.Activate();
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
        ShowRaceResults(new int[racers.Count]);
    }

	public override void GiveUpRace() {
        // Change scene
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

	private void Start() {
        //InitializeRelatedObjects();
        //// TODO: Remove after DEBUG
        //PlayerState.Instance.CurrentStats = new PlayerStats {
        //    speed = 50,
        //    dexterity = 50,
        //    endurance = 50,
        //    precision = 50,
        //    magic = 50
        //};

        // TODO: Set level generator parameters
        // TODO: Generate level using level generator
        // TODO: Compute level difficulty
        // TODO: InitializeRacers();

        // Hide HUD
        raceHUD.gameObject.SetActive(false);
    }
}