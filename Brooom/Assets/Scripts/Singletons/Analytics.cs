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

	private SpellController playerSpellController;
	private PlayerIncomingSpellTracker playerIncomingSpellTracker;


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
		// TODO: CharacterRaceState - onPlaceChanged, onHoopAdvance, onCheckpointMissed, onHoopMissed, onWrongDirectionChanged
		// TODO: EffectibleCharacter - onNewEffectAdded
		// TODO: CharacterEffect - onEffectStart, onEffectEnd
		// TODO: LevelGenerationPipeline - onLevelGenerated
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
		// Reset data fields
		this.playerSpellController = null;
		this.playerIncomingSpellTracker = null;
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
		// Get all slots' content
		StringBuilder slotsContent = new StringBuilder("Current content is: ");
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			Spell equippedSpell = PlayerState.Instance.equippedSpells[i];
			if (equippedSpell == null || string.IsNullOrEmpty(equippedSpell.Identifier))
				slotsContent.Append("Empty");
			else slotsContent.Append(equippedSpell.SpellName);
			if (i < PlayerState.Instance.equippedSpells.Length - 1)
				slotsContent.Append(" - ");
		}
		slotsContent.Append(".");
		// Log
		if (spell == null)
			LogEvent(AnalyticsCategory.Spells, $"Slot {slotIndex} has been emptied. {slotsContent}");
		else
			LogEvent(AnalyticsCategory.Spells, $"Spell {spell.SpellName} has been assigned to slot {slotIndex}. {slotsContent}");
	}

	private void OnSpellCastAtPlayer(IncomingSpellInfo spellInfo) {
		spellInfo.SpellObject.onSpellHit += OnSpellHitPlayer;
	}
	private void OnSpellHitPlayer(SpellEffectController spellEffectController) {
		LogEvent(AnalyticsCategory.Spells, $"Player was hit by spell {spellEffectController.Spell.SpellName}.");
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
