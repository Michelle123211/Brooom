using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


/// <summary>
/// A singleton which keeps track of all achievements in the game and their progress.
/// </summary>
public class AchievementManager : MonoBehaviourSingleton<AchievementManager>, ISingleton {
	// All necessary values for the achievements (updated in reaction to specific messages)
	// ... these are grouped into several classes (each class tracking data for a particular group of achievements)
	private List<AchievementData> achievementData = new List<AchievementData> {
		new ScoreData(),
		new SpellsData(),
		new RaceData(),
		new LevelData(),
		new CoinsData(),
		new BroomData()
	};

	// Current progress of all achievements (current level, maximum level and if it is new)
	private List<AchievementProgress> achievementsProgress;

	/// <summary>
	/// Finds all achievements available in the game and initializes their progress from a save file (if available).
	/// </summary>
	public void LoadAchievementsProgress() {
		// Get all achievements' ScriptableObjects
		InitializeAchievements();
		// Load the achievements (all tracked data) from a file
		foreach (var data in achievementData)
			data.LoadData();
		// Update progress of all achievements
		UpdateAchievementsProgress();
	}

	/// <summary>
	/// Returns a list of <c>AchievementProgress</c> describing current level and maximum level for each achievement in the game.
	/// </summary>
	/// <returns>A list of all achievements with their current progress.</returns>
	public List<AchievementProgress> GetAllAchievementsProgress() {
		UpdateAchievementsProgress();
		return achievementsProgress;
	}

	/// <summary>
	/// Saves the current progress of all achievements (all of their tracked data) persistently.
	/// </summary>
	public void SaveAchievementsProgress() {
		foreach (var data in achievementData)
			data.SaveData();
	}

	/// <summary>
	/// Resets everything to default values (initialized achievements to their default values and then saves these persistently).
	/// </summary>
	public void ResetAchievementsProgress() {
		// Get all achievements' ScriptableObjects (and don't update their progress), reset them
		InitializeAchievements();
		// Save the new values
		SaveAchievementsProgress();
	}

	private void InitializeAchievements() {
		// Get all achievements' ScriptableObjects (and don't update their progress)
		Achievement[] achievements = Resources.LoadAll<Achievement>("Achievements/");
		achievementsProgress = new List<AchievementProgress>();
		foreach (var achievement in achievements) {
			if (achievement.type == AchievementType.None) continue; // invalid achievement
			AchievementProgress progress = new AchievementProgress(achievement);
			if (progress.maximumLevel > 0) // it is valid
				achievementsProgress.Add(progress);
		}
		// Reset all tracked data for all achievements
		foreach (var data in achievementData)
			data.ResetData();
	}

	private void UpdateAchievementsProgress() {
		if (achievementsProgress == null) LoadAchievementsProgress();
		for (int i = achievementsProgress.Count - 1; i >= 0; i--) {
			// Get index of the handling class
			int dataIndex = (int)achievementsProgress[i].achievement.type / 100;
			// Get the current achievement level
			int newLevel = achievementData[dataIndex].GetCurrentAchievementLevel(achievementsProgress[i].achievement);
			achievementsProgress[i].isNew = newLevel != achievementsProgress[i].currentLevel; // mark new achievement
			achievementsProgress[i].currentLevel = newLevel;
			// Handle unknown type or other problems
			if (achievementsProgress[i].currentLevel == -1) {
				Debug.LogWarning($"Achievement of type {achievementsProgress[i].achievement.type} could not be handled.");
				achievementsProgress.RemoveAt(i); // remove the achievement
			}
		}
		// Save the progress
		SaveAchievementsProgress();
	}

	private void OnDestroy() {
		// Unregister from all messages
		foreach (var data in achievementData)
			data.UnregisterCallbacks();
	}

	#region Singleton initialization

	static AchievementManager() { 
		Options = SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances;
	}

	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		// Register for all necessary messages in the Messaging class
		foreach (var data in achievementData)
			data.RegisterCallbacks();
		// Load all achievements
		LoadAchievementsProgress();
	}
	#endregion
}

// Enum for different types of recorded values (with default empty option)
public enum AchievementType {
	None = 0,
	// Score
	NumberOne = 1,
	MaximumStat,
	// Spells
	SpellsCasted = 101,
	AllSpells,
	// Race
	RacesFinished = 201,
	RacesGivenUp,
	FirstPlaces,
	LastPlaces,
	LongestLoseStreak,
	LongestWinStreak,
	TrackTrials,
	// Level
	ObstacleCollisions = 301,
	BonusesPickedUp,
	HoopsPassed,
	HoopsMissed,
	AllRegionsAvailable,
	AllRegionsVisited,
	// Coins
	CoinsEarned = 401,
	CoinsAmount,
	// Broom
	AllBroomUpgrades = 501
}

/// <summary>
/// A class capturing current progress of a particular achievement, i.e. its current level, maximum level and whether it is newly obtained).
/// </summary>
public class AchievementProgress {
	public Achievement achievement;
	public int currentLevel = 0;
	public int maximumLevel = 0;
	public bool isNew = false;

	public AchievementProgress(Achievement achievement) {
		this.achievement = achievement;
		this.currentLevel = 0;
		if (achievement.valuesForLevels == null || achievement.valuesForLevels.Count == 0) {
			if (achievement.type == AchievementType.None) this.maximumLevel = 0; // invalid achievement
			else this.maximumLevel = 1; // achievement with only one level, represented by bool
		} else {
			this.maximumLevel = achievement.valuesForLevels.Count;
		}
	}
}


/// <summary>
/// A class tracking all data necessary for a particular group of achievements.
/// Each group will be implemented by a separated derived class.
/// </summary>
abstract class AchievementData {
	/// <summary>
	/// Resets all tracked data to its initial values.
	/// </summary>
	public abstract void ResetData();

	/// <summary>
	/// Registers callbacks to collect all necessary data.
	/// </summary>
	public abstract void RegisterCallbacks();
	/// <summary>
	/// Unregisters all callbacks registered in <c>RegisterCallbacks()</c> method.
	/// </summary>
	public abstract void UnregisterCallbacks();

	/// <summary>
	/// Gets the current level of the given achievement based on the tracked values.
	/// </summary>
	/// <param name="achievement">Achievement whose current level we want to get.</param>
	/// <returns>The current level of achievement or -1, if any error occurred.</returns>
	public abstract int GetCurrentAchievementLevel(Achievement achievement);

	/// <summary>
	/// Initializes values from a persistently stored data.
	/// </summary>
	public abstract void LoadData();
	/// <summary>
	/// Persistently stores all current values.
	/// </summary>
	public abstract void SaveData();

	/// <summary>
	/// Returns achievement's level based on the given value (which is compared to values necessary for individual levels).
	/// </summary>
	/// <param name="achievement">Achievement whose level we want to compute.</param>
	/// <param name="value">Value based on which the level is determined.</param>
	/// <returns>Level of the given achievement based on the given value.</returns>
	protected int GetAchievementLevelFromValues(Achievement achievement, float value) {
		int level = 0;
		if (achievement.valuesForLevels != null) {
			foreach (var levelValue in achievement.valuesForLevels) {
				if (value >= levelValue) level++;
				else break;
			}
		}
		return level;
	}
}


/// <summary>
/// A class tracking all data necessary for a determining level of achievements related to score (e.g., highest rank, maximum stat).
/// </summary>
[System.Serializable]
class ScoreData : AchievementData {
	// Highest place in the leaderboard
	public int highestRank = int.MaxValue;
	// Maximum statistics value
	public int maxStatValue = 0;

	/// <inheritdoc/>
	public override void ResetData() {
		highestRank = int.MaxValue;
		maxStatValue = 0;
	}

	/// <inheritdoc/>
	public override void LoadData() {
		// Load data from a file specific for this group of achievements
		ScoreData data = SaveSystem.LoadAchievementData<ScoreData>("score");
		if (data != null) {
			this.highestRank = data.highestRank;
			this.maxStatValue = data.maxStatValue;
		}
	}

	/// <inheritdoc/>
	public override void SaveData() {
		// Save data to a file specific for this group of achievements
		SaveSystem.SaveAchievementData(this, "score");
	}

	/// <inheritdoc/>
	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("RankChanged", OnRankChanged);
		Messaging.RegisterForMessage("StatsChanged", OnStatsChanged);
	}

	/// <inheritdoc/>
	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("RankChanged", OnRankChanged);
		Messaging.UnregisterFromMessage("StatsChanged", OnStatsChanged);
	}

	/// <inheritdoc/>
	public override int GetCurrentAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.NumberOne: // for being first on the leaderboard
				return highestRank == 1 ? 1 : 0;
			case AchievementType.MaximumStat: // for having a maximum value (100) in an arbitrary stat
				return maxStatValue == 100 ? 1 : 0;
			default: // Unknown type
				return -1;
		}
	}

	private void OnRankChanged(int rank) {
		// Store it if it is better than ever before
		if (rank < highestRank) highestRank = rank;
		SaveData();
	}

	private void OnStatsChanged() {
		// Get maximum
		foreach (var stat in PlayerState.Instance.CurrentStats.GetListOfValues())
			if (maxStatValue < stat) maxStatValue = (int)stat;
		SaveData();
	}
}

/// <summary>
/// A class tracking all data necessary for a determining level of achievements related to spells (e.g., number of spells cast, whether all spells have been purchased).
/// </summary>
[System.Serializable]
class SpellsData : AchievementData {
	// Number of spell cast
	public int spellsCast = 0;
	// Whether all spells have been purchased
	public bool allSpellsPurchased = false;

	/// <inheritdoc/>
	public override void ResetData() {
		spellsCast = 0;
		allSpellsPurchased = false;
	}

	/// <inheritdoc/>
	public override void LoadData() {
		// Load data from a file specific for this group of achievements
		SpellsData data = SaveSystem.LoadAchievementData<SpellsData>("spells");
		if (data != null) {
			this.spellsCast = data.spellsCast;
			this.allSpellsPurchased = data.allSpellsPurchased;
		}
	}

	/// <inheritdoc/>
	public override void SaveData() {
		// Save data to a file specific for this group of achievements
		SaveSystem.SaveAchievementData(this, "spells");
	}

	/// <inheritdoc/>
	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("SpellCasted", OnSpellCast);
		Messaging.RegisterForMessage("SpellPurchased", OnSpellPurchased);
		Messaging.RegisterForMessage("AllSpellsPurchased", OnAllSpellsPurchased);
	}

	/// <inheritdoc/>
	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("SpellCasted", OnSpellCast);
		Messaging.UnregisterFromMessage("SpellPurchased", OnSpellPurchased);
		Messaging.UnregisterFromMessage("AllSpellsPurchased", OnAllSpellsPurchased);
	}

	/// <inheritdoc/>
	public override int GetCurrentAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.SpellsCasted: // for casting spells a certain number of times
				return GetAchievementLevelFromValues(achievement, spellsCast);
			case AchievementType.AllSpells: // for unlocking all spells
				return allSpellsPurchased ? 1 : 0;
			default: // Unknown type
				return -1;
		}
	}

	private void OnSpellCast(string spellIdentifier) {
		spellsCast++;
		SaveData();
	}

	private void OnSpellPurchased() {
		// TODO: May not be needed - the message is not sent nor handled and there is no achievement for it right now
		SaveData();
	}

	private void OnAllSpellsPurchased() {
		allSpellsPurchased = true;
		SaveData();
	}
}

/// <summary>
/// A class tracking all data necessary for a determining level of achievements related to race (e.g., number of races finished or given up, win streak).
/// </summary>
[System.Serializable]
class RaceData : AchievementData {
	// Number of races finished
	public int racesFinished = 0;
	// Number of first and last places
	public int firstPlace = 0;
	public int lastPlace = 0;
	// Number of races given up
	public int racesGivenUp = 0;
	// Number of consecutive last places
	public int currentLoseStreak = 0;
	public int longestLoseStreak = 0;
	// Number of consecutive first places
	public int currentWinStreak = 0;
	public int longestWinStreak = 0;
	// Maximum number of trials in a single level
	public int trackTrials = 0;

	private int currentNumberOfRacers = 0;

	/// <inheritdoc/>
	public override void ResetData() {
		racesFinished = 0;
		firstPlace = 0;
		lastPlace = 0;
		racesGivenUp = 0;
		currentLoseStreak = 0;
		longestLoseStreak = 0;
		currentWinStreak = 0;
		longestWinStreak = 0;
		trackTrials = 0;

		currentNumberOfRacers = 0;
	}

	/// <inheritdoc/>
	public override void LoadData() {
		// Load data from a file specific for this group of achievements
		RaceData data = SaveSystem.LoadAchievementData<RaceData>("race");
		if (data != null) {
			this.racesFinished = data.racesFinished;
			this.firstPlace = data.firstPlace;
			this.lastPlace = data.lastPlace;
			this.racesGivenUp = data.racesGivenUp;
			this.currentLoseStreak = data.currentLoseStreak;
			this.longestLoseStreak = data.longestLoseStreak;
			this.currentWinStreak = data.currentWinStreak;
			this.longestWinStreak = data.longestWinStreak;
			this.trackTrials = data.trackTrials;
		}
	}

	/// <inheritdoc/>
	public override void SaveData() {
		// Save data to a file specific for this group of achievements
		SaveSystem.SaveAchievementData(this, "race");
	}

	/// <inheritdoc/>
	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("RaceStarted", OnRaceStarted);
		Messaging.RegisterForMessage("RaceGivenUp", OnRaceGivenUp);
		Messaging.RegisterForMessage("RaceFinished", OnRaceFinished);
		Messaging.RegisterForMessage("TrainingEnded", OnTrainingEnded);
	}

	/// <inheritdoc/>
	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("RaceStarted", OnRaceStarted);
		Messaging.UnregisterFromMessage("RaceGivenUp", OnRaceGivenUp);
		Messaging.UnregisterFromMessage("RaceFinished", OnRaceFinished);
		Messaging.UnregisterFromMessage("TrainingEnded", OnTrainingEnded);
	}

	/// <inheritdoc/>
	public override int GetCurrentAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.RacesFinished: // for a certain number of races finished
				return GetAchievementLevelFromValues(achievement, racesFinished);
			case AchievementType.FirstPlaces: // for a certain number of placing first
				return GetAchievementLevelFromValues(achievement, firstPlace);
			case AchievementType.LastPlaces: // for a certain number of placing last
				return GetAchievementLevelFromValues(achievement, lastPlace);
			case AchievementType.RacesGivenUp: // for a certain number of races finished
				return GetAchievementLevelFromValues(achievement, racesGivenUp);
			case AchievementType.LongestLoseStreak: // for a lose streak of certain length
				return GetAchievementLevelFromValues(achievement, longestLoseStreak);
			case AchievementType.LongestWinStreak: // for a win streak of certain length
				return GetAchievementLevelFromValues(achievement, longestWinStreak);
			case AchievementType.TrackTrials: // for e certain number of trials during training phase
				return GetAchievementLevelFromValues(achievement, trackTrials);
			default: // Unknown type
				return -1;
		}
	}

	private void OnRaceStarted(int numRacers) {
		// Store number of races so it can be used later when the race is finished to determine if the player is last
		currentNumberOfRacers = numRacers;
		SaveData();
	}

	private void OnRaceGivenUp() {
		racesGivenUp++;
		SaveData();
	}

	// Requires OnRaceStarted to be invoked first (to initialize currentNumberOfRacers)
	private void OnRaceFinished(int place) {
		// Note down another race finished
		racesFinished++;
		// Update win/lose streaks and number of firt/last place
		if (place == 1) { // first
			firstPlace++;
			currentLoseStreak = 0;
			currentWinStreak++;
			if (currentWinStreak > longestWinStreak) longestWinStreak = currentWinStreak;
		} else if (place == currentNumberOfRacers) { // last
			lastPlace++;
			currentWinStreak = 0;
			currentLoseStreak++;
			if (currentLoseStreak > longestLoseStreak) longestLoseStreak = currentLoseStreak;
		} else {
			currentLoseStreak = 0;
			currentWinStreak = 0;
		}
		SaveData();
	}

	private void OnTrainingEnded(int numTrials) {
		// Remember maximum number of trials during any training
		if (numTrials > trackTrials) trackTrials = numTrials;
		SaveData();
	}
}


/// <summary>
/// A class tracking all data necessary for a determining level of achievements related to level (e.g., number of hoops passed or bonuses picked up, whethet all regions have been visited).
/// </summary>
[System.Serializable]
class LevelData : AchievementData {
	// Number of collisions with obstacles
	public int obstacleCollisions = 0;
	// Number of picked up bonuses
	public int bonusesPickedUp = 0;
	// Number of hoops passed
	public int hoopsPassed = 0;
	// Number of hoops missed
	public int hoopsMissed = 0;
	// Whether all regions were unlocked
	public bool allRegionsAvailable = false;
	// Whether all regions were visited
	public bool allRegionsVisited = false;

	/// <inheritdoc/>
	public override void ResetData() {
		obstacleCollisions = 0;
		bonusesPickedUp = 0;
		hoopsPassed = 0;
		hoopsMissed = 0;
		allRegionsAvailable = false;
		allRegionsVisited = false;
	}

	/// <inheritdoc/>
	public override void LoadData() {
		// Load data from a file specific for this group of achievements
		LevelData data = SaveSystem.LoadAchievementData<LevelData>("level");
		if (data != null) {
			this.obstacleCollisions = data.obstacleCollisions;
			this.bonusesPickedUp = data.bonusesPickedUp;
			this.hoopsPassed = data.hoopsPassed;
			this.hoopsMissed = data.hoopsMissed;
			this.allRegionsAvailable = data.allRegionsAvailable;
			this.allRegionsVisited = data.allRegionsVisited;
		}
	}

	/// <inheritdoc/>
	public override void SaveData() {
		// Save data to a file specific for this group of achievements
		SaveSystem.SaveAchievementData(this, "level");
	}

	/// <inheritdoc/>
	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("ObstacleCollision", OnCollisionWithObstacle);
		Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);
		Messaging.RegisterForMessage("HoopAdvance", (Action<bool>)OnHoopAdvance);
		Messaging.RegisterForMessage("NewRegionAvailable", OnNewRegionAvailable);
		Messaging.RegisterForMessage("NewRegionVisited", OnNewRegionVisited);
	}

	/// <inheritdoc/>
	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("ObstacleCollision", OnCollisionWithObstacle);
		Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
		Messaging.UnregisterFromMessage("HoopAdvance", (Action<bool>)OnHoopAdvance);
		Messaging.UnregisterFromMessage("NewRegionAvailable", OnNewRegionAvailable);
		Messaging.UnregisterFromMessage("NewRegionVisited", OnNewRegionVisited);
	}

	/// <inheritdoc/>
	public override int GetCurrentAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.ObstacleCollisions: // for colliding with obstacles a certain number of times
				return GetAchievementLevelFromValues(achievement, obstacleCollisions);
			case AchievementType.BonusesPickedUp: // for picking up a certain number of bonuses
				return GetAchievementLevelFromValues(achievement, bonusesPickedUp);
			case AchievementType.HoopsPassed: // for passing through a certain number of hoops
				return GetAchievementLevelFromValues(achievement, hoopsPassed);
			case AchievementType.HoopsMissed: // for missing a certain number of hoops
				return GetAchievementLevelFromValues(achievement, hoopsMissed);
			case AchievementType.AllRegionsAvailable: // for unlocking all regions
				return allRegionsAvailable ? 1 : 0;
			case AchievementType.AllRegionsVisited: // for visiting all regions
				return allRegionsVisited ? 1 : 0;
			default: // Unknown type
				return -1;
		}
	}

	private void OnCollisionWithObstacle() {
		obstacleCollisions++;
		SaveData();
	}

	private void OnBonusPickedUp(GameObject bonus) {
		bonusesPickedUp++;
		SaveData();
	}

	private void OnHoopAdvance(bool passed) {
		if (passed) hoopsPassed++;
		else hoopsMissed++;
		SaveData();
	}

	private void OnNewRegionAvailable(int regionNumber) {
		if (allRegionsAvailable) return; // no need to check again
		bool allAvailable = true;
		// Enumerate all possible regions and make sure they are all available
		foreach (LevelRegionType region in Enum.GetValues(typeof(LevelRegionType))) {
			if (region == LevelRegionType.NONE) continue;
			if (region == LevelRegionType.MysteriousTunnel) continue; // TODO: Skipped only temporarily, until it is added to the game
			if (!PlayerState.Instance.regionsAvailability.ContainsKey(region) || !PlayerState.Instance.regionsAvailability[region]) {
				allAvailable = false;
				break;
			}
		}
		allRegionsAvailable = allAvailable;
		SaveData();
	}

	private void OnNewRegionVisited(int regionNumber) {
		if (allRegionsVisited) return; // no need to check again
		bool allVisited = true;
		// Enumerate all possible regions and make sure they have been visited
		foreach (LevelRegionType region in Enum.GetValues(typeof(LevelRegionType))) {
			if (region == LevelRegionType.NONE) continue;
			if (region == LevelRegionType.MysteriousTunnel) continue; // TODO: Skipped only temporarily, until it is added to the game
			if (!PlayerState.Instance.regionsVisited.ContainsKey(region) || !PlayerState.Instance.regionsVisited[region]) {
				allVisited = false;
				break;
			}
		}
		allRegionsVisited = allVisited;
		SaveData();
	}
}


/// <summary>
/// A class tracking all data necessary for a determining level of achievements related to coins (e.g., current amount, total amount gained over time).
/// </summary>
[System.Serializable]
class CoinsData : AchievementData {
	// Total coins gained
	public int totalCoinsGain = 0;
	// Maximum number of coins at a single time
	public int maxCoins = 0;

	/// <inheritdoc/>
	public override void ResetData() {
		totalCoinsGain = 0;
		maxCoins = 0;
	}

	/// <inheritdoc/>
	public override void LoadData() {
		// Load data from a file specific for this group of achievements
		CoinsData data = SaveSystem.LoadAchievementData<CoinsData>("coins");
		if (data != null) {
			this.totalCoinsGain = data.totalCoinsGain;
			this.maxCoins = data.maxCoins;
		}
	}

	/// <inheritdoc/>
	public override void SaveData() {
		// Save data to a file specific for this group of achievements
		SaveSystem.SaveAchievementData(this, "coins");
	}

	/// <inheritdoc/>
	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("CoinsChanged", OnCoinsAmountChanged);
	}

	/// <inheritdoc/>
	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("CoinsChanged", OnCoinsAmountChanged);
	}

	/// <inheritdoc/>
	public override int GetCurrentAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.CoinsEarned: // for a certain amount gained in total over time
				return GetAchievementLevelFromValues(achievement, totalCoinsGain);
			case AchievementType.CoinsAmount: // for having a certain amount at a single moment
				return GetAchievementLevelFromValues(achievement, maxCoins);
			default: // Unknown type
				return -1;
		}
	}

	private void OnCoinsAmountChanged(int delta) {
		if (delta > 0) // gain
			totalCoinsGain += delta;
		int currentCoins = PlayerState.Instance.Coins;
		if (currentCoins > maxCoins) maxCoins = currentCoins;
		SaveData();
	}
}


/// <summary>
/// A class tracking all data necessary for a determining level of achievements related to broom (e.g., whether a broom has been upgraded to maximum).
/// </summary>
[System.Serializable]
class BroomData : AchievementData {
	// Whether all broom upgrades have been purchased
	public bool allUpgradesPurchased = false;

	/// <inheritdoc/>
	public override void ResetData() {
		allUpgradesPurchased = false;
	}

	/// <inheritdoc/>
	public override void LoadData() {
		// Load data from a file specific for this group of achievements
		BroomData data = SaveSystem.LoadAchievementData<BroomData>("broom");
		if (data != null) {
			this.allUpgradesPurchased = data.allUpgradesPurchased;
		}
	}

	/// <inheritdoc/>
	public override void SaveData() {
		// Save data to a file specific for this group of achievements
		SaveSystem.SaveAchievementData(this, "broom");
	}

	/// <inheritdoc/>
	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("AllBroomUpgrades", OnAllBroomUpgradesPurchased);
	}

	/// <inheritdoc/>
	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("AllBroomUpgrades", OnAllBroomUpgradesPurchased);
	}

	/// <inheritdoc/>
	public override int GetCurrentAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.AllBroomUpgrades: // for purchasing all broom upgrades
				return (allUpgradesPurchased ? 1 : 0);
			default: // Unknown type
				return -1;
		}
	}

	private void OnAllBroomUpgradesPurchased() {
		if (allUpgradesPurchased) return; // no need to continue
		allUpgradesPurchased = true;
		SaveData();
	}
}