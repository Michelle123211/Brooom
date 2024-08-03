using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AchievementManager : MonoBehaviourSingleton<AchievementManager>, ISingleton {
	// All necessary values for the achievements
	// ... updated in reaction to specific messages
	private List<AchievementData> achievementData = new List<AchievementData> {
		new ScoreData(),
		new SpellsData(),
		new RaceData(),
		new LevelData(),
		new CoinsData(),
		new BroomData()
	};

	private List<AchievementProgress> achievementsProgress;

	public void LoadAchievementsProgress() {
		// Get all achievements' ScriptableObjects
		InitializeAchievements();
		// Load the achievements (all tracked data) from a file
		foreach (var data in achievementData)
			data.LoadData();
		// Update progress of all achievements
		UpdateAchievementsProgress();
	}

	// Returns a list of all achievements with their current progress (level)
	public List<AchievementProgress> GetAllAchievementsProgress() {
		UpdateAchievementsProgress();
		return achievementsProgress;
	}

	// Save the achievements (all tracked data) persistently
	public void SaveAchievementsProgress() {
		foreach (var data in achievementData)
			data.SaveData();
	}

	// Reset everything to default values
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
		// Reset all data
		foreach (var data in achievementData)
			data.ResetData();
	}

	private void UpdateAchievementsProgress() {
		if (achievementsProgress == null) LoadAchievementsProgress();
		for (int i = achievementsProgress.Count - 1; i >= 0; i--) {
			// Get index of the handling class
			int dataIndex = (int)achievementsProgress[i].achievement.type / 100;
			// Get the current achievement level
			int newLevel = achievementData[dataIndex].GetAchievementLevel(achievementsProgress[i].achievement);
			achievementsProgress[i].levelChanged = newLevel != achievementsProgress[i].currentLevel; // Mark any changes
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
	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		// Register for all necessary messages in the Messaging class
		foreach (var data in achievementData)
			data.RegisterCallbacks();
		// Load all achievements
		LoadAchievementsProgress();
	}

	// Persistent singleton
	protected override void SetSingletonOptions() {
		Options = (int)SingletonOptions.CreateNewGameObject | (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances;
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

public class AchievementProgress {
	public Achievement achievement;
	public int currentLevel = 0;
	public int maximumLevel = 0;
	public bool levelChanged = false;

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

abstract class AchievementData {
	// Resets all data to its initial values
	public abstract void ResetData();

	// Registers callbacks to collect all necessary data
	public abstract void RegisterCallbacks();
	// Unregisters all registered callbacks
	public abstract void UnregisterCallbacks();

	// Returns -1 in case of problems
	public abstract int GetAchievementLevel(Achievement achievement);

	public abstract void LoadData();
	public abstract void SaveData();

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

[System.Serializable]
class ScoreData : AchievementData {
	// Highest place in the leaderboard
	public int highestRank = int.MaxValue;
	// Maximum statistics value
	public int maxStatValue = 0;

	public override void ResetData() {
		highestRank = int.MaxValue;
		maxStatValue = 0;
	}

	public override void LoadData() {
		ScoreData data = SaveSystem.LoadAchievementData<ScoreData>("score");
		if (data != null) {
			this.highestRank = data.highestRank;
			this.maxStatValue = data.maxStatValue;
		}
	}

	public override void SaveData() {
		SaveSystem.SaveAchievementData(this, "score");
	}

	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("RankChanged", OnRankChanged);
		Messaging.RegisterForMessage("StatsChanged", OnStatsChanged);
	}

	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("RankChanged", OnRankChanged);
		Messaging.UnregisterFromMessage("StatsChanged", OnStatsChanged);
	}

	public override int GetAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.NumberOne:
				return highestRank == 1 ? 1 : 0;
			case AchievementType.MaximumStat:
				return maxStatValue == 100 ? 1 : 0;
			default: // Unknown type
				return -1;
		}
	}

	private void OnRankChanged(int rank) {
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

[System.Serializable]
class SpellsData : AchievementData {
	// Number of spell cast
	public int spellsCasted = 0;
	// Whether all spells have been purchased
	public bool allSpellsPurchased = false;

	public override void ResetData() {
		spellsCasted = 0;
		allSpellsPurchased = false;
	}

	public override void LoadData() {
		SpellsData data = SaveSystem.LoadAchievementData<SpellsData>("spells");
		if (data != null) {
			this.spellsCasted = data.spellsCasted;
			this.allSpellsPurchased = data.allSpellsPurchased;
		}
	}

	public override void SaveData() {
		SaveSystem.SaveAchievementData(this, "spells");
	}

	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("SpellCasted", OnSpellCasted);
		Messaging.RegisterForMessage("SpellPurchased", OnSpellPurchased);
		Messaging.RegisterForMessage("AllSpellsPurchased", OnAllSpellsPurchased);
	}

	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("SpellCasted", OnSpellCasted);
		Messaging.UnregisterFromMessage("SpellPurchased", OnSpellPurchased);
		Messaging.UnregisterFromMessage("AllSpellsPurchased", OnAllSpellsPurchased);
	}

	public override int GetAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.SpellsCasted:
				return GetAchievementLevelFromValues(achievement, spellsCasted);
			case AchievementType.AllSpells:
				return allSpellsPurchased ? 1 : 0;
			default: // Unknown type
				return -1;
		}
	}

	private void OnSpellCasted(string spellIdentifier) {
		spellsCasted++;
		SaveData();
	}

	private void OnSpellPurchased() {
		// TODO: May not be needed
		SaveData();
	}

	private void OnAllSpellsPurchased() {
		allSpellsPurchased = true;
		SaveData();
	}
}

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

	public override void LoadData() {
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

	public override void SaveData() {
		SaveSystem.SaveAchievementData(this, "race");
	}

	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("RaceStarted", OnRaceStarted);
		Messaging.RegisterForMessage("RaceGivenUp", OnRaceGivenUp);
		Messaging.RegisterForMessage("RaceFinished", OnRaceFinished);
		Messaging.RegisterForMessage("TrainingEnded", OnTrainingEnded);
	}

	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("RaceStarted", OnRaceStarted);
		Messaging.UnregisterFromMessage("RaceGivenUp", OnRaceGivenUp);
		Messaging.UnregisterFromMessage("RaceFinished", OnRaceFinished);
		Messaging.UnregisterFromMessage("TrainingEnded", OnTrainingEnded);
	}

	public override int GetAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.RacesFinished:
				return GetAchievementLevelFromValues(achievement, racesFinished);
			case AchievementType.FirstPlaces:
				return GetAchievementLevelFromValues(achievement, firstPlace);
			case AchievementType.LastPlaces:
				return GetAchievementLevelFromValues(achievement, lastPlace);
			case AchievementType.RacesGivenUp:
				return GetAchievementLevelFromValues(achievement, racesGivenUp);
			case AchievementType.LongestLoseStreak:
				return GetAchievementLevelFromValues(achievement, longestLoseStreak);
			case AchievementType.LongestWinStreak:
				return GetAchievementLevelFromValues(achievement, longestWinStreak);
			case AchievementType.TrackTrials:
				return GetAchievementLevelFromValues(achievement, trackTrials);
			default: // Unknown type
				return -1;
		}
	}

	private void OnRaceStarted(int numRacers) { // TODO: Corresponding message must be called before OnRaceFinished - MESSAGES
		currentNumberOfRacers = numRacers;
		SaveData();
	}

	private void OnRaceGivenUp() {
		racesGivenUp++;
		SaveData();
	}

	private void OnRaceFinished(int place) {
		racesFinished++;
		if (place == 1) { // first
			firstPlace++;
			currentLoseStreak = 0;
			longestWinStreak++;
			if (currentWinStreak > longestWinStreak) longestWinStreak = currentWinStreak;
		} else if (place == currentNumberOfRacers) { // last
			currentLoseStreak = 0;
			currentWinStreak = 0;
		} else {
			lastPlace++;
			currentWinStreak = 0;
			longestLoseStreak++;
			if (currentLoseStreak > longestLoseStreak) longestLoseStreak = currentLoseStreak;
		}
		SaveData();
	}

	private void OnTrainingEnded(int numTrials) {
		if (numTrials > trackTrials) trackTrials = numTrials;
		SaveData();
	}
}

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
	// WHether all regions were unlocked
	public bool allRegionsAvailable = false;
	// Whether all regions were visited
	// TODO: How to detect visiting a region
	public bool allRegionsVisited = false;

	public override void ResetData() {
		obstacleCollisions = 0;
		bonusesPickedUp = 0;
		hoopsPassed = 0;
		hoopsMissed = 0;
		allRegionsAvailable = false;
		allRegionsVisited = false;
	}

	public override void LoadData() {
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

	public override void SaveData() {
		SaveSystem.SaveAchievementData(this, "level");
	}

	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("ObstacleCollision", OnCollisionWithObstacle);
		Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);
		Messaging.RegisterForMessage("HoopAdvance", (Action<bool>)OnHoopAdvance);
		Messaging.RegisterForMessage("NewRegionAvailable", OnNewRegionAvailable);
		Messaging.RegisterForMessage("NewRegionVisited", OnNewRegionVisited);
	}

	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("ObstacleCollision", OnCollisionWithObstacle);
		Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
		Messaging.UnregisterFromMessage("HoopAdvance", (Action<bool>)OnHoopAdvance);
		Messaging.UnregisterFromMessage("NewRegionAvailable", OnNewRegionAvailable);
		Messaging.UnregisterFromMessage("NewRegionVisited", OnNewRegionVisited);
	}

	public override int GetAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.ObstacleCollisions:
				return GetAchievementLevelFromValues(achievement, obstacleCollisions);
			case AchievementType.BonusesPickedUp:
				return GetAchievementLevelFromValues(achievement, bonusesPickedUp);
			case AchievementType.HoopsPassed:
				return GetAchievementLevelFromValues(achievement, hoopsPassed);
			case AchievementType.HoopsMissed:
				return GetAchievementLevelFromValues(achievement, hoopsMissed);
			case AchievementType.AllRegionsAvailable:
				return allRegionsAvailable ? 1 : 0;
			case AchievementType.AllRegionsVisited:
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

	private void OnNewRegionAvailable() {
		if (allRegionsAvailable) return; // no need to check again
		bool allAvailable = true;
		foreach (var region in PlayerState.Instance.regionsAvailability)
			if (!region.Value) allAvailable = false;
		allRegionsAvailable = allAvailable;
		SaveData();
	}

	private void OnNewRegionVisited() {
		// TODO: React to a new region visited
		SaveData();
	}
}

[System.Serializable]
class CoinsData : AchievementData {
	// Total coins gained
	public int totalCoinsGain = 0;
	// Maximum number of coins at a single time
	public int maxCoins = 0;

	public override void ResetData() {
		totalCoinsGain = 0;
		maxCoins = 0;
	}

	public override void LoadData() {
		CoinsData data = SaveSystem.LoadAchievementData<CoinsData>("coins");
		if (data != null) {
			this.totalCoinsGain = data.totalCoinsGain;
			this.maxCoins = data.maxCoins;
		}
	}

	public override void SaveData() {
		SaveSystem.SaveAchievementData(this, "coins");
	}

	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("CoinsChanged", OnCoinsAmountChanged);
	}

	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("CoinsChanged", OnCoinsAmountChanged);
	}

	public override int GetAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.CoinsEarned:
				return GetAchievementLevelFromValues(achievement, totalCoinsGain);
			case AchievementType.CoinsAmount:
				return GetAchievementLevelFromValues(achievement, maxCoins);
			default: // Unknown type
				return -1;
		}
	}

	private void OnCoinsAmountChanged(int delta) {
		if (delta > 0) // gain
			totalCoinsGain += delta;
		int currentCoins = PlayerState.Instance.coins;
		if (currentCoins > maxCoins) maxCoins = currentCoins;
		SaveData();
	}
}

[System.Serializable]
class BroomData : AchievementData {
	// Whether all broom upgrades have been purchased
	public bool allUpgradesPurchased = false;

	public override void ResetData() {
		allUpgradesPurchased = false;
	}

	public override void LoadData() {
		BroomData data = SaveSystem.LoadAchievementData<BroomData>("broom");
		if (data != null) {
			this.allUpgradesPurchased = data.allUpgradesPurchased;
		}
	}

	public override void SaveData() {
		SaveSystem.SaveAchievementData(this, "broom");
	}

	public override void RegisterCallbacks() {
		Messaging.RegisterForMessage("AllBroomUpgrades", OnAllBroomUpgradesPurchased);
	}

	public override void UnregisterCallbacks() {
		Messaging.UnregisterFromMessage("AllBroomUpgrades", OnAllBroomUpgradesPurchased);
	}

	public override int GetAchievementLevel(Achievement achievement) {
		switch (achievement.type) {
			case AchievementType.AllBroomUpgrades:
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