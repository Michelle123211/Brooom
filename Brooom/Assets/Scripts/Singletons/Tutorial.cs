using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviourSingleton<Tutorial>, ISingleton {

	[field:SerializeField]
	public TutorialStage CurrentStage { get; private set; } = TutorialStage.FirstRace; // TODO: Change to Introduction and hide the field in Inspector

	// Called whenever progress in tutorial changes
	private void SaveCurrentProgress() { 
		// TODO: Store persistently current tutorial progress
	}

	private void LoadCurrentProgress() { 
		// TODO: Load persistently saved tutorial progress to continue from there
	}

	// The following few methods are used as callbacks for several events to track tutorial progress
	private void OnRaceFinished() {
		if (CurrentStage == TutorialStage.FirstRace) MoveToNextStage();
	}

	private void RegisterCallbacksBasedOnStage() {
		// TODO: Add more callbacks
		if (CurrentStage <= TutorialStage.FirstRace)
			Messaging.RegisterForMessage("RaceFinished", OnRaceFinished);
	}
	private void UnregisterCallbacks() {
		// TODO: Add more callbacks
		Messaging.UnregisterFromMessage("RaceFinished", OnRaceFinished); // should work even if the callback was never registered
	}

	private void MoveToNextStage() {
		if (CurrentStage != TutorialStage.Finished)
			CurrentStage = (TutorialStage)((int)CurrentStage + 1);
		SaveCurrentProgress();
	}

	static Tutorial() {
		Options = SingletonOptions.LazyInitialization | SingletonOptions.RemoveRedundantInstances | SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes;
	}

	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		LoadCurrentProgress();
		RegisterCallbacksBasedOnStage();
	}

	private void OnDestroy() {
		SaveCurrentProgress(); // just to make sure
		UnregisterCallbacks();
	}

}

public enum TutorialStage { 
	// TODO: Add more stages if necessary, but make sure stages are assigned correctly to regions in RaceController's Inspector
	Introduction, // story, motivation
	BasicMovement, // forward, turn, brake
	FlyUpOrDown, // up and down
	LookingAround, // look around, reset
	Hoop, // hoop and checkpoint
	Bonus,
	FreeMovement, // can fly freely and proceed whenever
	FirstRace,
	PlayerOverview, // leaderboard, stats
	Shop, // button to go there, spells, broom upgrades
	EquipSpell,
	CastSpell, // mana bonus, cast spell, recharge bonus
	TestingTrack,
	Finished
}
