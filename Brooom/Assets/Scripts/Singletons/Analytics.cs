using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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

	// Objects where callbacks are registered
	private SpellController playerSpellController; // onSpellCast, onManaAmountChanged, onSelectedSpellChanged
	private PlayerIncomingSpellTracker playerIncomingSpellTracker; // onIncomingSpellAdded
	private LevelGenerationPipeline levelGenerator; // onLevelGenerated


	public void LogEvent(AnalyticsCategory category, string eventDescription) {
		if (!analyticsEnabled) return;
		// Output into a file in the following format - "CurrentDatetime | CurrentScene | Category | EventDescription"
		file.WriteLine($"{DateTime.Now.ToString(culture)} | {SceneLoader.Instance.CurrentScene} | {category} | {eventDescription}");
	}
	static Analytics() { 
		Options = SingletonOptions.RemoveRedundantInstances | SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes;
	}

	private void RegisterSceneSpecificCallbacks(Scene scene) {
		switch (scene) {
			case Scene.Race:
				RegisterCallbacksInRaceScene();
				break;
		}
	}

	private void UnregisterSceneSpecificCallbacks(Scene scene) {
		switch (scene) {
			case Scene.Race:
				UnregisterCallbacksInRaceScene();
				break;
		}
	}

	#region Global callbacks and messages
	private void RegisterForMessages() {
		Messaging.RegisterForMessage("TrainingEnded", OnTrainingEnded);
		Messaging.RegisterForMessage("RaceStarted", OnRaceStarted);
		Messaging.RegisterForMessage("RaceGivenUp", OnRaceGivenUp);
		Messaging.RegisterForMessage("RaceFinished", OnRaceFinished);
		Messaging.RegisterForMessage("HoopAdvance", (Action<bool>)OnHoopAdvance);
		Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);
		Messaging.RegisterForMessage("NewRegionAvailable", OnNewRegionAvailable);
		Messaging.RegisterForMessage("NewRegionVisited", OnNewRegionVisited);

		// TODO: Message "RankChanged"
		// TODO: Message "StatsChanged"
		// TODO: Message "SpellCasted"
		// TODO: Message "SpellPurchased"
		// TODO: Message "AllSpellsPurchased"
		// TODO: Message "ObstacleCollision"
		// TODO: Message "CoinsChanged"
		// TODO: Message "AllBroomUpgrades"
	}

	private void UnregisterFromMessages() {
		Messaging.UnregisterFromMessage("TrainingEnded", OnTrainingEnded);
		Messaging.UnregisterFromMessage("RaceStarted", OnRaceStarted);
		Messaging.UnregisterFromMessage("RaceGivenUp", OnRaceGivenUp);
		Messaging.UnregisterFromMessage("RaceFinished", OnRaceFinished);
		Messaging.UnregisterFromMessage("HoopAdvance", (Action<bool>)OnHoopAdvance);
		Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
		Messaging.UnregisterFromMessage("NewRegionAvailable", OnNewRegionAvailable);
		Messaging.UnregisterFromMessage("NewRegionVisited", OnNewRegionVisited);
	}

	private void RegisterGlobalCallbacks() {
		// TODO: Register all global callbacks
		SceneLoader.Instance.onSceneStartedLoading += OnSceneStartedLoading;
		SceneLoader.Instance.onSceneLoaded += OnSceneLoaded;
		// TODO: PlayerState - onCoinsAmountChanged
		PlayerState.Instance.onEquippedSpellChanged += OnEquippedSpellChanged;
	}

	private void UnregisterGlobalCallbacks() {
		// TODO: Unregister all global callbacks
		SceneLoader.Instance.onSceneStartedLoading -= OnSceneStartedLoading;
		SceneLoader.Instance.onSceneLoaded -= OnSceneLoaded;
		// TODO: PlayerState - onCoinsAmountChanged
		PlayerState.Instance.onEquippedSpellChanged -= OnEquippedSpellChanged;
	}
	#endregion

	#region Scene-based callbacks
	private void RegisterCallbacksInRaceScene() {
		// Initialize data fields
		this.playerSpellController = RaceController.Instance.playerRacer.characterController.GetComponentInChildren<SpellController>();
		this.playerIncomingSpellTracker = RaceController.Instance.playerRacer.characterController.GetComponentInChildren<PlayerIncomingSpellTracker>();
		this.levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
		// Register callbacks
		this.playerSpellController.onSpellCast += OnSpellCast;
		this.playerSpellController.onManaAmountChanged += OnManaAmountChanged;
		this.playerSpellController.onSelectedSpellChanged += OnSelectedSpellChanged;
		foreach (var spell in this.playerSpellController.spellSlots) {
			if (spell != null && !spell.IsEmpty()) {
				spell.onBecomesAvailable += OnSpellBecameAvailable;
				spell.onBecomesUnavailable += OnSpellBecameUnavailable;
			}
		}
		this.playerIncomingSpellTracker.onIncomingSpellAdded += OnSpellCastAtPlayer;
		OnLevelGenerated(RaceController.Instance.Level); // level should be already generated when the scene is loaded, so there is no need to register callback
		RaceController.Instance.playerRacer.state.onPlaceChanged += OnPlaceInRaceChanged;
		RaceController.Instance.playerRacer.state.onHoopMissed += OnHoopMissed;
		RaceController.Instance.playerRacer.state.onCheckpointMissed += OnCheckpointMissed;
		RaceController.Instance.playerRacer.state.onWrongDirectionChanged += OnWrongDirectionChanged;
		// TODO: EffectibleCharacter - onNewEffectAdded
		// TODO: CharacterEffect - onEffectStart, onEffectEnd
	}

	private void UnregisterCallbacksInRaceScene() {
		// Unregister callbacks
		this.playerSpellController.onSpellCast -= OnSpellCast;
		this.playerSpellController.onManaAmountChanged -= OnManaAmountChanged;
		fullManaLogged = false;
		this.playerSpellController.onSelectedSpellChanged -= OnSelectedSpellChanged;
		foreach (var spell in this.playerSpellController.spellSlots) {
			if (spell != null && !spell.IsEmpty()) {
				spell.onBecomesAvailable -= OnSpellBecameAvailable;
				spell.onBecomesUnavailable -= OnSpellBecameUnavailable;
			}
		}
		this.playerIncomingSpellTracker.onIncomingSpellAdded -= OnSpellCastAtPlayer;
		RaceController.Instance.playerRacer.state.onPlaceChanged -= OnPlaceInRaceChanged;
		RaceController.Instance.playerRacer.state.onHoopMissed -= OnHoopMissed;
		RaceController.Instance.playerRacer.state.onCheckpointMissed -= OnCheckpointMissed;
		RaceController.Instance.playerRacer.state.onWrongDirectionChanged -= OnWrongDirectionChanged;
		// Reset data fields
		this.playerSpellController = null;
		this.playerIncomingSpellTracker = null;
		this.levelGenerator = null;
	}
	#endregion

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

	#region Spells callbacks
	private void OnSpellCast(int spellIndex) {
		LogEvent(AnalyticsCategory.Spells, $"Spell {PlayerState.Instance.equippedSpells[spellIndex].SpellName} cast.");
	}

	private void OnSelectedSpellChanged(int spellIndex) {
		LogEvent(AnalyticsCategory.Spells, $"Selected spell changed to {PlayerState.Instance.equippedSpells[spellIndex].SpellName}.");
	}

	private bool fullManaLogged = false;
	private void OnManaAmountChanged(int manaAmount) {
		if (manaAmount == playerSpellController.MaxMana) {
			if (!fullManaLogged) { // log it only once
				LogEvent(AnalyticsCategory.Spells, $"Mana amount changed to {manaAmount}.");
				LogEvent(AnalyticsCategory.Spells, $"Mana is completely full.");
			}
			fullManaLogged = true;
		} else {
			LogEvent(AnalyticsCategory.Spells, $"Mana amount changed to {manaAmount}.");
			fullManaLogged = false;
		}
	}

	private void OnSpellBecameAvailable(Spell spell) {
		LogEvent(AnalyticsCategory.Spells, $"Spell {spell.SpellName} became available.");
	}

	private void OnSpellBecameUnavailable(Spell spell) {
		LogEvent(AnalyticsCategory.Spells, $"Spell {spell.SpellName} became unavailable.");
	}

	private void OnEquippedSpellChanged(Spell spell, int slotIndex) {
		string slotsContent = GetCurrentSpellSlotsContent(out _);
		if (spell == null)
			LogEvent(AnalyticsCategory.Spells, $"Slot {slotIndex} has been emptied. Current content is: {slotsContent}.");
		else
			LogEvent(AnalyticsCategory.Spells, $"Spell {spell.SpellName} has been assigned to slot {slotIndex}. Current content is: {slotsContent}.");
	}

	private string GetCurrentSpellSlotsContent(out int spellCount) {
		spellCount = 0;
		StringBuilder slotsContent = new StringBuilder();
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			Spell equippedSpell = PlayerState.Instance.equippedSpells[i];
			if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier)) {
				slotsContent.Append(equippedSpell.SpellName);
				spellCount++;
			} else slotsContent.Append("Empty");
			if (i < PlayerState.Instance.equippedSpells.Length - 1)
				slotsContent.Append(" - ");
		}
		return slotsContent.ToString();
	}

	private void OnSpellCastAtPlayer(IncomingSpellInfo spellInfo) {
		spellInfo.SpellObject.onSpellHit += OnSpellHitPlayer;
	}
	private void OnSpellHitPlayer(SpellEffectController spellEffectController) {
		LogEvent(AnalyticsCategory.Spells, $"Player was hit by a spell {spellEffectController.Spell.SpellName}.");
	}
	#endregion

	#region Race callbacks
	private void OnLevelGenerated(LevelRepresentation level) {
		// Get basic parameters
		TrackPointsGenerationRandomWalk trackGenerator = levelGenerator.GetComponent<TrackPointsGenerationRandomWalk>();
		int checkpointsCount = trackGenerator.numberOfCheckpoints;
		int hoopsCount = trackGenerator.numberOfHoopsBetween;
		int hoopTotal = level.Track.Count;
		Vector2 maxAngle = trackGenerator.maxDirectionChangeAngle;
		Vector2 distance = trackGenerator.distanceRange;
		float hoopScale = levelGenerator.GetComponent<TrackObjectsPlacement>().hoopScale;
		// Prepare a list of regions
		StringBuilder regions = new StringBuilder();
		int i = 0;
		foreach (var region in level.RegionsInLevel) {
			regions.Append(region);
			if (i < level.RegionsInLevel.Count - 1) regions.Append(", ");
			i++;
		}
		// Log
		LogEvent(AnalyticsCategory.Race, $"Track generated with the following parameters: {checkpointsCount} checkpoints with {hoopsCount} hoops inbetween (so {hoopTotal} hoops in total), distance between hoops between {distance.x} and {distance.y}, hoop scale {hoopScale}, maximum angle {maxAngle.x} and {maxAngle.y}, and regions {regions}.");
	}

	private void OnTrainingEnded(int numTrials) {
		LogEvent(AnalyticsCategory.Race, "Training ended.");
	}

	private void OnRaceStarted(int numRacers) {
		string spellSlotContent = GetCurrentSpellSlotsContent(out int spellCount);
		LogEvent(AnalyticsCategory.Race, $"Race started with {numRacers} racers. The player has {spellCount} spells equipped: {spellSlotContent}.");
	}

	private void OnRaceGivenUp() {
		LogEvent(AnalyticsCategory.Race, "Race given up.");
	}

	private void OnRaceFinished(int place) {
		// Time and penalization
		float time = RaceController.Instance.playerRacer.state.finishTime;
		float timePenalization = RaceController.Instance.playerRacer.state.timePenalization;
		string timeText = "DNF";
		if (time > 0) timeText = Utils.FormatTime(time + timePenalization);
		// Missed hoops
		int hoopsMissed = RaceController.Instance.playerRacer.state.hoopsMissed;

		LogEvent(AnalyticsCategory.Race, $"Race finished with the following results: place {place}, time {timeText} (+{Mathf.RoundToInt(timePenalization)} s), {hoopsMissed} missed hoops.");
	}

	private void OnPlaceInRaceChanged(int place) {
		LogEvent(AnalyticsCategory.Race, $"Player's place changed to {place}.");
	}

	private void OnHoopAdvance(bool hoopPassed) {
		// Log only if passed (missed are handled separately)
		if (hoopPassed) LogEvent(AnalyticsCategory.Race, "Hoop/checkpoint passed.");
	}

	private void OnHoopMissed() {
		LogEvent(AnalyticsCategory.Race, "Hoop missed.");
	}

	private void OnCheckpointMissed() {
		LogEvent(AnalyticsCategory.Race, "Checkpoint missed.");
	}

	private void OnWrongDirectionChanged(bool isWrongDirection) { 
		if (isWrongDirection) LogEvent(AnalyticsCategory.Race, "Flying in wrong direction.");
		else LogEvent(AnalyticsCategory.Race, "No longer flying in wrong direction.");
	}

	private void OnBonusPickedUp(GameObject bonus) {
		string bonusType = "";
		if (bonus.TryGetComponent<SpeedBonusEffect>(out _)) bonusType = "Speed";
		else if (bonus.TryGetComponent<NavigationBonusEffect>(out _)) bonusType = "Navigation";
		else if (bonus.TryGetComponent<ManaBonusEffect>(out _)) bonusType = "Mana";
		else if (bonus.TryGetComponent<RechargeSpellsBonusEffect>(out _)) bonusType = "Recharge";
		LogEvent(AnalyticsCategory.Race, $"{bonusType} bonus picked up.");
	}

	private void OnNewRegionAvailable(int regionNumber) {
		LogEvent(AnalyticsCategory.Race, $"New region available: {(LevelRegionType)regionNumber}.");
	}

	private void OnNewRegionVisited(int regionNumber) {
		LogEvent(AnalyticsCategory.Race, $"New region visited: {(LevelRegionType)regionNumber}.");
	}
	#endregion

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
