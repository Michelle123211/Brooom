using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class Analytics : MonoBehaviourSingleton<Analytics>, ISingleton {

	[Tooltip("If this is false, no events will be logged into a file.")]
	[SerializeField] bool analyticsEnabled = false;

	[Tooltip("Name of the file (without extension) the analytics will be saved into.")]
	[SerializeField] string analyticsFileName = "Events";

	private static readonly long maxFileSize = 1048576; // 1 MB
	

	private CultureInfo culture = new CultureInfo("cs-CZ");
	private StreamWriter file;

	private Scene currentScene = Scene.Start;


	public void LogEvent(AnalyticsCategory category, string eventDescription) {
		if (!analyticsEnabled) return;
		// Output into a file in the following format - "CurrentDatetime | CurrentScene | Category | EventDescription"
		file.WriteLine($"{DateTime.Now.ToString(culture)} | {SceneLoader.Instance.CurrentScene} | {category} | {eventDescription}");
	}
	static Analytics() { 
		Options = SingletonOptions.RemoveRedundantInstances | SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes;
	}

	private void OnSceneStartedLoading(Scene newScene) {
		// Unregister scene-specific callbacks from the old scene (stored in currentScene)
		UnregisterSceneSpecificCallbacks(currentScene);
	}

	private void OnSceneLoaded(Scene newScene) {
		LogEvent(AnalyticsCategory.Game, $"Scene changed from {currentScene} to {newScene}.");
		// Register scene-specific callbacks in the new scene
		RegisterSceneSpecificCallbacks(newScene);

		currentScene = newScene;
	}

	private void RegisterSceneSpecificCallbacks(Scene scene) {
		switch (scene) {
			case Scene.Race:
				// TODO: CharacterRaceState - onPlaceChanged, onHoopAdvance, onCheckpointMissed, onHoopMissed, onWrongDirectionChanged
				// TODO: EffectibleCharacter - onNewEffectAdded
				// TODO: CharacterEffect - onEffectStart, onEffectEnd
				// TODO: SpellInRace - onBecomesAvailable, onBecomesUnavailable
				// TODO: SpellController - onManaAmountChanged, onSelectedSpellChanged, onSpellCast
				// TODO: LevelGenerationPipeline - onLevelGenerated
				break;
		}
	}

	private void UnregisterSceneSpecificCallbacks(Scene scene) {
		switch (scene) {

		}
	}

	private void RegisterForMessages() {
		// TODO: Message "RankChanged"
		// TODO: Message "StatsChanged"
		// TODO: Message "SpellCasted"
		// TODO: Message "SpellPurchased"
		// TODO: Message "AllSpellsPurchased"
		// TODO: Message "RaceStarted"
		// TODO: Message "RaceGivenUp"
		// TODO: Message "RaceFinished"
		// TODO: Message "TrainingEnded"
		// TODO: Message "ObstacleCollision"
		// TODO: Message "BonusPickedUp"
		// TODO: Message "HoopAdvance"
		// TODO: Message "NewRegionAvailable"
		// TODO: Message "NewRegionVisited"
		// TODO: Message "CoinsChanged"
		// TODO: Message "AllBroomUpgrades"
	}

	private void UnregisterFromMessages() { 
		
	}

	private void RegisterGlobalCallbacks() {
		// TODO: Register all global callbacks
		SceneLoader.Instance.onSceneStartedLoading += OnSceneStartedLoading;
		SceneLoader.Instance.onSceneLoaded += OnSceneLoaded;
		// TODO: PlayerState - onCoinsAmountChanged
	}

	private void UnregisterGlobalCallbacks() {
		// TODO: Unregister all global callbacks
		SceneLoader.Instance.onSceneStartedLoading -= OnSceneStartedLoading;
		SceneLoader.Instance.onSceneLoaded -= OnSceneLoaded;
		// TODO: PlayerState - onCoinsAmountChanged
	}

	#region Initialization + singleton stuff

	public void AwakeSingleton() {
		// Make sure the analytics output file exists
		string eventsFolder = Application.persistentDataPath + "/Events/";
		if (!Directory.Exists(eventsFolder))
			Directory.CreateDirectory(eventsFolder);
		// If the file is too large already, just empty it completely
		string filePath = eventsFolder + analyticsFileName + ".log";
		FileInfo fileInfo = new FileInfo(filePath);
		if (fileInfo.Exists && fileInfo.Length > maxFileSize) {
			using (FileStream fileStream = File.Open(filePath, FileMode.Open)) {
				fileStream.SetLength(0);
			}
		}
		// Open the file for writing
		file = new StreamWriter(filePath, true); // true for append
		file.AutoFlush = true;
		LogEvent(AnalyticsCategory.Analytics, "Analytics set up.");
	}

	public void InitializeSingleton() {
		// Nothing to do here because it uses eager initialization, so this method is called at the same time as AwakeSingleton()
	}

	private void Start() {
		RegisterForMessages();
		RegisterGlobalCallbacks();
	}

	private void OnDestroy() {
		UnregisterFromMessages();
		UnregisterGlobalCallbacks();

		// Flush the content and close the file
		LogEvent(AnalyticsCategory.Analytics, "Analytics shut down.");
		file.WriteLine("===========================================");
		file.Close();
	}
	#endregion
}

public enum AnalyticsCategory { 
	Analytics,
	Game,
	Stats,
	Race,
	Spells,
	Shop,
	TestingTrack,
	Tutorial,
	Other
}
