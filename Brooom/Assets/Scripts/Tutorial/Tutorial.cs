using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviourSingleton<Tutorial>, ISingleton {

	public TutorialStage CurrentStage { get; private set; } = TutorialStage.NotStarted;

	[Tooltip("An object capable of highlighting a certain part of the screen.")]
	public TutorialUIHighlight highlighter;
	[Tooltip("An object responsible for displaying different panels with text during tutorial.")]
	public TutorialPanels panel;

	[Tooltip("An Image used as a fadeout to black.")]
	[SerializeField] GameObject fadeout;

	private TutorialStageBase currentStageRepresentation;

	// Called whenever progress in tutorial changes
	public void SaveCurrentProgress() {
		// Get current stage's substate
		string stageState = string.Empty;
		if (currentStageRepresentation != null) stageState = currentStageRepresentation.GetCurrentState();
		// Store current tutorial progress persistently
		SaveSystem.SaveTutorialData(new TutorialSaveData { mainStage = CurrentStage.ToString(), subState = stageState });
	}
	// Called when continuing a game
	public void LoadCurrentProgress() {
		// Load persistently saved tutorial progress to continue from there
		TutorialSaveData savedState = SaveSystem.LoadTutorialData();
		if (savedState != null) {
			// Initialize current stage according to the saved state
			CurrentStage = Enum.Parse<TutorialStage>(savedState.mainStage);
			currentStageRepresentation = GetTutorialStageRepresentation(CurrentStage);
			if (currentStageRepresentation != null) {
				currentStageRepresentation.SetCurrentState(savedState.subState);
			}
			SaveCurrentProgress(); // just to make sure (e.g. if the substate changed because it was set back to a previous checkpoint)
		} else {
			// If there is no saved state, simply start from the beginning
			ResetCurrentProgress();
		}

	}
	// Called when starting a new game
	public void ResetCurrentProgress() {
		// Move to the first stage
		CurrentStage = TutorialStage.NotStarted;
		MoveToNextStage(); // state is also saved there
	}

	public void FadeOut() {
		fadeout.TweenAwareEnable();
		highlighter.StopHighlighting(); // stop highlighting anything when fading out
	}
	public void FadeIn() {
		fadeout.TweenAwareDisable();
	}

	// Returns an instance of class derived from TutorialStageBase corresponding to the given stage
	private TutorialStageBase GetTutorialStageRepresentation(TutorialStage stage) {
		return stage switch {
			TutorialStage.Introduction => new IntroductionTutorial(),
			TutorialStage.FirstRace => new FirstRaceTutorial(),
			TutorialStage.PlayerOverview => new PlayerOverviewTutorial(),
			TutorialStage.Shop => new ShopTutorial(),
			TutorialStage.Spells => new SpellsTutorial(),
			TutorialStage.TestingTrack => new TestingTrackTutorial(),
			_ => null
		};
	}

	// Moves to the next stage (if possible)
	private void MoveToNextStage() {
		if (CurrentStage == TutorialStage.Finished) return;
		// Find the next stage - the least number greater than the current stage (there could be gaps)
		int nextStage = (int)TutorialStage.Finished;
		foreach (int i in Enum.GetValues(typeof(TutorialStage)))
			if (i > (int)CurrentStage && i < nextStage) nextStage = i;
		// Set next stage
		CurrentStage = (TutorialStage)nextStage;
		currentStageRepresentation = GetTutorialStageRepresentation(CurrentStage);
		SaveCurrentProgress();
	}

	// Moves to the next stage (if possible) and updates it right away (so that it can be triggered as soon as possible)
	private void MoveToNextStageAndUpdate() {
		MoveToNextStage();
		if (currentStageRepresentation != null)
			currentStageRepresentation.Update();
	}

	// Updates the current stage representation to ensure tutorial is progressing
	private void Update() {
		if (CurrentStage == TutorialStage.Finished) return;
		if (currentStageRepresentation == null) return;
		// Update current tutorial stage and check if it is time to move to a next one
		if (!currentStageRepresentation.Update()) {
			MoveToNextStageAndUpdate(); // update it right away so that it can be triggered as soon as possible
		}
	}

	static Tutorial() {
		Options = SingletonOptions.LazyInitialization | SingletonOptions.RemoveRedundantInstances | SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes;
	}

	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		LoadCurrentProgress();
	}

	private void OnDestroy() {
		SaveCurrentProgress(); // just to make sure
	}

}

public enum TutorialStage { 
	// TODO: Add more stages if necessary, but make sure stages are assigned correctly to regions in RaceController's Inspector
	NotStarted = 0,
	Introduction = 10, // story, motivation, movement, track elements
	FirstRace = 20, // training, reset position, starting zone
	PlayerOverview = 30, // leaderboard, stats
	Shop = 40, // button to go there, spells, broom upgrades
	Spells = 50, // equip spell, mana, mana bonus, spell target and aiming, spell cast, recharge bonus
	TestingTrack = 60,
	Finished = 99
}

public abstract class TutorialStageBase {

	// Tutorial stage may be in one of several states - could be checked to make sure we don't trigger or initialize it twice
	private enum TutorialStageState {
		NotTriggered,
		Initializing,
		Running,
		Finished
	}

	private TutorialStageState stageState = TutorialStageState.NotTriggered;

	// Returns true if this tutorial stage is still running, false if it should finish
	public bool Update() {
		// Check if it is possible to trigger this tutorial stage
		if (stageState == TutorialStageState.NotTriggered) {
			if (CheckTriggerConditions()) {
				stageState = TutorialStageState.Initializing;
				Tutorial.Instance.StartCoroutine(Initialize());
			}
		}
		// If the tutorial stage is running, update it and check if it is finished
		if (stageState == TutorialStageState.Running) {
			if (!UpdateTutorialStage()) {
				stageState = TutorialStageState.Finished;
				Finish();
			}
		}
		// Check if this tutorial stage has finished
		return (stageState != TutorialStageState.Finished);
	}

	// Performs a cleanup (putting everything into a consistent state, e.g. after a tutorial stage is skipped)
	public abstract void Finish();

	// Sets the current state according to the state loaded from a persistent storage
	public abstract void SetCurrentState(string state);
	// Gets the current state as a string so it could be stored persistently
	public abstract string GetCurrentState();

	// Initializes the tutorial stage (started as a coroutine so it is possible to wait until it is done)
	protected IEnumerator Initialize() {
		yield return InitializeTutorialStage();
		stageState = TutorialStageState.Running;
	}

	// Check if it is possible to start this tutorial stage
	protected abstract bool CheckTriggerConditions();
	// Initializes the tutorial stage (started as a coroutine so it is possible to wait until it is done)
	protected abstract IEnumerator InitializeTutorialStage();
	// Returns true if this tutorial stage is still running, false if it should finish
	protected abstract bool UpdateTutorialStage();

}
