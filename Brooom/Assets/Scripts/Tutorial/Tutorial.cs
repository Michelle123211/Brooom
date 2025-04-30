using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton managing tutorial throughout the whole game.
/// It keeps track of the current tutorial stage and moves on to the next stage when it finishes.
/// It also provides references to useful objects (tutorial panels, UI highlighter, fadeout overlay)
/// and useful methods, e.g. leaving the current tutorial stage or skipping it (these are used from a pause menu).
/// Current tutorial state is stored persistently, so it can be resumed the next time the game is started.
/// </summary>
public class Tutorial : MonoBehaviourSingleton<Tutorial>, ISingleton {

	/// <summary>Current tutorial stage.</summary>
	public TutorialStage CurrentStage { get; private set; } = TutorialStage.NotStarted;

	[Tooltip("An object capable of highlighting a certain part of the screen.")]
	public TutorialUIHighlight highlighter;
	[Tooltip("An object responsible for displaying different panels with text during tutorial.")]
	public TutorialPanels panel;

	[Tooltip("An Image used as a fadeout to black.")]
	[SerializeField] GameObject fadeout;

	[Tooltip("A prefab of panel asking the player if they want to enable or disable training before race. It is displayed during the First Race tutorial.")]
	public GameObject skipTrainingPanel;

	// An object representation of the current stage which takes care of progressing through it
	private TutorialStageBase currentStageRepresentation;


	/// <summary>
	/// Gets the current tutorial state (also a substate from the current stage) and stores it persistently.
	/// Called whenever progress in tutorial changes.
	/// </summary>
	public void SaveCurrentProgress() {
		// Get current stage's substate
		string stageState = string.Empty;
		if (currentStageRepresentation != null) stageState = currentStageRepresentation.GetCurrentState();
		// Store current tutorial progress persistently
		SaveSystem.SaveTutorialData(new TutorialSaveData { mainStage = CurrentStage.ToString(), subState = stageState });
	}

	/// <summary>
	/// Loads tutorial state from a save file and then initializes the tutorial to continue from there.
	/// Called when continuing a game.
	/// </summary>
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

	/// <summary>
	/// Resets the tutorial state and moves to the first stage.
	/// Called when starting a new game.
	/// </summary>
	public void ResetCurrentProgress() {
		// Move to the first stage
		CurrentStage = TutorialStage.NotStarted;
		MoveToNextStage(); // state is also saved there
	}

	/// <summary>
	/// Finalizes the current tutorial stage to leave it in a consistent state, resets everything (e.g. panels, highlights, fadeouts)
	/// and moves on to the next tutorial stage.
	/// </summary>
	public void SkipCurrentTutorialStage() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {CurrentStage} skipped.");
		LeaveCurrentTutorialStage(); // to reset all panels, highlights, fadeouts
		if (currentStageRepresentation != null) {
			currentStageRepresentation.Finish();
		}
		MoveToNextStage();
	}

	/// <summary>
	/// Saves current tutorial state and resets all panels, highlights and fadeouts.
	/// </summary>
	public void LeaveCurrentTutorialStage() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {CurrentStage} left.");
		StopAllCoroutines();
		SaveCurrentProgress();
		FadeIn();
		panel.HideAllTutorialPanels();
		highlighter.StopHighlighting();
	}

	/// <summary>
	/// Stops highlighting area of the screen and fades out to black by showing a black overlay.
	/// </summary>
	public void FadeOut() {
		fadeout.TweenAwareEnable();
		highlighter.StopHighlighting(); // stop highlighting anything when fading out
	}
	/// <summary>
	/// Fades in by hiding a black overlay.
	/// </summary>
	public void FadeIn() {
		fadeout.TweenAwareDisable();
	}

	/// <summary>
	/// Checks if the current stage is running (i.e. already initialized, in progress and not finished yet).
	/// </summary>
	/// <returns><c>true</c> if the current tutorial stage is in progress, <c>false</c> otherwise.</returns>
	public bool IsInProgress() {
		if (currentStageRepresentation == null) return false;
		else return currentStageRepresentation.StageState == TutorialStageBase.TutorialStageState.Running;
	}

	/// <summary>
	/// Gets an instance of a class derived from <c>TutorialStageBase</c> which corresponds to the given tutorial stage.
	/// </summary>
	/// <param name="stage">Tutorial stage whose representation to get.</param>
	/// <returns>Object representation of the given tutorial stage.</returns>
	private TutorialStageBase GetTutorialStageRepresentation(TutorialStage stage) {
		return stage switch {
			TutorialStage.Introduction => new IntroductionTutorial(),
			TutorialStage.FirstRace => new FirstRaceTutorial(),
			TutorialStage.PlayerOverview => new PlayerOverviewTutorial(),
			TutorialStage.Shop => new ShopTutorial(),
			TutorialStage.EquipSpells => new EquipSpellsTutorial(),
			TutorialStage.CastSpells => new CastSpellsTutorial(),
			TutorialStage.TestingTrack => new TestingTrackTutorial(),
			_ => null
		};
	}

	/// <summary>
	/// Moves on to the next stage, if possible.
	/// </summary>
	private void MoveToNextStage() {
		if (CurrentStage == TutorialStage.Finished) return;
		// Find the next stage - the smallest number greater than the current stage (there could be gaps)
		int nextStage = (int)TutorialStage.Finished;
		foreach (int i in Enum.GetValues(typeof(TutorialStage)))
			if (i > (int)CurrentStage && i < nextStage) nextStage = i;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Moving tutorial stage from {CurrentStage} to {(TutorialStage)nextStage}.");
		// Set next stage
		CurrentStage = (TutorialStage)nextStage;
		currentStageRepresentation = GetTutorialStageRepresentation(CurrentStage);
		SaveCurrentProgress();
	}

	/// <summary>
	/// Updates the current stage representation to ensure the tutorial is progressing.
	/// Also checks if it is time to move on to the next tutorial stage, and if it is, it does so.
	/// </summary>
	private void Update() {
		// Update current tutorial stage and check if it is time to move to a next one
		if (CurrentStage == TutorialStage.Finished) return;
		if (currentStageRepresentation == null) return;
		if (!currentStageRepresentation.Update()) {
			MoveToNextStage();
		}
	}

	#region Singleton initialization
	static Tutorial() {
		// Singleton options override
		Options = SingletonOptions.LazyInitialization | SingletonOptions.RemoveRedundantInstances | SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes;
	}

	/// <inheritdoc/>
	public void AwakeSingleton() {
	}

	/// <inheritdoc/>
	public void InitializeSingleton() {
		LoadCurrentProgress();
	}
	#endregion

}

/// <summary>
/// All different tutorial stages, going one after another.
/// </summary>
public enum TutorialStage { 
	// TODO: Add more stages if necessary, but make sure stages are assigned correctly to regions in RaceController's Inspector
	NotStarted = 0,
	Introduction = 10, // story, motivation, movement, track elements
	FirstRace = 20, // training, reset position, starting zone
	PlayerOverview = 30, // leaderboard, stats
	Shop = 40, // button to go there, spells, broom upgrades
	EquipSpells = 50, // equip spell
	CastSpells = 60, // mana, mana bonus, spell target and aiming, spell cast, recharge bonus
	TestingTrack = 70, // button to go there
	Finished = 99
}


/// <summary>
/// A base class for representations of a single tutorial stage, making sure it is initialized and updated.
/// These stages are run one after another in a given order.
/// Each stage has some trigger conditions which need to be met for the tutorial stage to start.
/// </summary>
public abstract class TutorialStageBase {

	/// <summary>Prefix of localization keys related to this tutorial stage.</summary>
	protected abstract string LocalizationKeyPrefix { get; }

	/// <summary>
	/// Tutorial stage may be in one of several states.
	/// </summary>
	public enum TutorialStageState {
		NotTriggered,
		Initializing,
		Running,
		Finished
	}

	/// <summary>Current state of the tutorial stage.</summary>
	public TutorialStageState StageState { get; private set; } = TutorialStageState.NotTriggered;
	private TutorialStepProgressTracker currentStepProgress = null; // it may be set when waiting for some step to be finished, then it is updated from Update()


	/// <summary>
	/// Updates the tutorial stage, e.g. checks if trigger conditions are met (if the tutorial stage is not running yet), 
	/// checks if the tutorial stage is finished (if it is running already).
	/// </summary>
	/// <returns><c>true</c> if this tutorial stage is still running, <c>false</c> if it should finish.</returns>
	public bool Update() {
		// Check if it is possible to trigger this tutorial stage
		if (StageState == TutorialStageState.NotTriggered) {
			if (CheckTriggerConditions()) {
				Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Trigger conditions satisfied for {LocalizationKeyPrefix} while tutorial is {(SettingsUI.enableTutorial ? "enabled" : "disabled")}.");
				// If tutorial is disabled in the settings, simply stop immediately (and move on to the next stage)
				if (!SettingsUI.enableTutorial) return false;
				// Otherwise invoke the initialization
				StageState = TutorialStageState.Initializing;
				Tutorial.Instance.StartCoroutine(Initialize());
			}
		}
		// If the tutorial stage is running, update it and check if it is finished
		if (StageState == TutorialStageState.Running) {
			HandlePauseIfNecessary();
			if (currentStepProgress != null) currentStepProgress.UpdateProgress();
			if (!UpdateTutorialStage()) {
				StageState = TutorialStageState.Finished;
				Finish();
			}
		}
		// Check if this tutorial stage has finished
		return (StageState != TutorialStageState.Finished);
	}

	/// <summary>
	/// Performs a cleanup, putting everything into a consistent state, e.g. after a tutorial stage is skipped.
	/// </summary>
	public abstract void Finish();

	/// <summary>
	/// Sets the current state according to the state loaded from a persistent storage.
	/// </summary>
	/// <param name="state">State loaded from a save file.</param>
	public abstract void SetCurrentState(string state);
	/// <summary>
	/// Gets the current state as a string so it could be stored persistently.
	/// </summary>
	/// <returns>Current state as a string.</returns>
	public abstract string GetCurrentState();

	/// <summary>
	/// Initializes the tutorial stage. It is started as a coroutine, so it is possible to wait easily until it is done.
	/// </summary>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	protected IEnumerator Initialize() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Initializing tutorial stage {LocalizationKeyPrefix}.");
		yield return InitializeTutorialStage();
		StageState = TutorialStageState.Running;
	}

	/// <summary>
	/// Checks if trigger conditions are met and this tutorial stage can be started.
	/// </summary>
	/// <returns><c>true</c> if it is possible to start this tutorial stage, <c>false</c> otherwise.</returns>
	protected abstract bool CheckTriggerConditions();
	/// <summary>
	/// Initializes the tutorial stage. It is started as a coroutine, so it is possible to wait easily until it is done.
	/// </summary>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	protected abstract IEnumerator InitializeTutorialStage();
	/// <summary>
	/// Updates the tutorial stage. Called from <c>Update()</c> method.
	/// </summary>
	/// <returns><c>true</c> if the tutorial stage is still running, <c>false</c> if it should finish.</returns>
	protected abstract bool UpdateTutorialStage();

	/// <summary>
	/// Gets localized string stored under the key "Tutorial&lt;LocalizationKeyPrefix&gt;_&lt;localizationKeySuffix&gt;".
	/// </summary>
	/// <param name="localizationKeySuffix">Suffix of the localization key, distinguishing strings from the same tutorial stage.</param>
	/// <returns>Localized string under the given key.</returns>
	protected string GetLocalizedText(string localizationKeySuffix) {
		return LocalizationManager.Instance.GetLocalizedString($"Tutorial{LocalizationKeyPrefix}_{localizationKeySuffix}");
	}

	/// <summary>
	/// Instantiates the given type derived from <c>TutorialStepProgressTracker</c>, starts tracking its progress
	/// and waits until it is finished. During that time, the progress is updated from <c>Update()</c> method.
	/// </summary>
	/// <typeparam name="T">Type of the tutorial step progress to instantiate.</typeparam>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	protected IEnumerator WaitUntilStepIsFinished<T>() where T : TutorialStepProgressTracker, new() {
		currentStepProgress = new T();
		currentStepProgress.StartTrackingProgress();
		yield return new WaitUntil(() => currentStepProgress.IsFinished);
		currentStepProgress.StopTrackingProgress();
		currentStepProgress = null;
	}

	// Methods for pausing and resuming game
	// - Game cannot be paused/resumed from a coroutine because of synchronization issues, but more tutorial stages may need this functionality,
	//   so it is extracted here and all stages have access to it and don't have to handle it separately
	private bool shouldPauseGame = false;
	/// <summary>
	/// Pauses the game during the next <c>Update()</c>.
	/// </summary>
	protected void PauseGame() {
		// Note down the game should be paused, then it will be paused during the next update
		shouldPauseGame = true;
	}
	private bool shouldResumeGame = false;
	/// <summary>
	/// Resumes the game during the next <c>Update()</c>.
	/// </summary>
	protected void ResumeGame() {
		// Note down the game should be resumed, then it will be resumed during the next update
		shouldResumeGame = true;
	}
	// Checks whether it was requested to pause or resume the game, and then does that
	private void HandlePauseIfNecessary() {
		if (shouldPauseGame || shouldResumeGame) {
			GamePause gamePause = UtilsMonoBehaviour.FindObject<GamePause>();
			if (shouldPauseGame) {
				if (gamePause != null) gamePause.PauseGame();
				else Time.timeScale = 0;
				shouldPauseGame = false;
			} else if (shouldResumeGame) {
				if (gamePause != null) gamePause.ResumeGame();
				else Time.timeScale = 1;
				shouldResumeGame = false;
			}
		}
	}

}


/// <summary>
/// A base class representing a progress within a single step of a tutorial stage.
/// It measures time elapsed from the beginning of tracking the progress.
/// Derived classes may extend it to provide functionality for a specific step.
/// A class derived from <c>TutorialStageBase</c> may then use it to track progress (e.g. elapsed time, check for player events)
/// and wait until the step is finished and it is possible to move on.
/// </summary>
public abstract class TutorialStepProgressTracker {

	/// <summary>Whether the step has already finished.</summary>
	public bool IsFinished { get; private set; } = false;

	private bool isRunning = false;
	protected float elapsedTime;

	/// <summary>
	/// Initializes everything necessary and starts tracking progress of the corresponding tutorial step.
	/// </summary>
	public void StartTrackingProgress() {
		IsFinished = false;
		elapsedTime = 0;
		InitializeStepProgress();
		isRunning = true;
	}

	/// <summary>
	/// Updates progress of the corresponding tutorial step (it is called in every frame).
	/// </summary>
	public void UpdateProgress() {
		if (!isRunning) return;
		elapsedTime += Time.deltaTime;
		UpdateStepProgress();
		IsFinished = CheckIfPossibleToMoveToNextStep();
	}

	/// <summary>
	/// Finalizes everything and stops tracking progress of the corresponding tutorial step.
	/// </summary>
	public void StopTrackingProgress() {
		FinishStepProgress();
	}

	/// <summary>
	/// Checks if the step is complete and it is possible to move to the next step.
	/// </summary>
	/// <returns><c>true</c> if it is possible to move on, <c>false</c> otherwise.</returns>
	protected abstract bool CheckIfPossibleToMoveToNextStep();
	/// <summary>
	/// Initializes everything necessary for tracking the step progress (e.g. data fields, callbacks).
	/// </summary>
	protected abstract void InitializeStepProgress();
	/// <summary>
	/// Updates the tracked progress of the corresponding step (e.g. detects events, accumulates results). Called in every frame.
	/// </summary>
	protected abstract void UpdateStepProgress();
	/// <summary>
	/// Finalizes everything (e.g. unregisters callbacks) and stops tracking the step progress.
	/// </summary>
	protected abstract void FinishStepProgress();

}
