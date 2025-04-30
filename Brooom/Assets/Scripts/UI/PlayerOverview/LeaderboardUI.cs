using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// A component representing a global leaderboard of racers, displayed in Player Overview.
/// It displays first few ranks and then the player and one rank above and one below, leaving the rest out.
/// Racers' names are chosen randomly, but once a name is generated for an opponent on a particular place in the global leaderboard,
/// this name is stored persistently so that it can be used again next time the leaderboard is displayed.
/// </summary>
public class LeaderboardUI : MonoBehaviour {

	[Header("Parameters")]
	[Tooltip("Parameters of opponents in the leaderboard (e.g. number of opponents, minimum and maximum stats values).")]
	public LeaderboardRepresentation leaderboard;

	[Header("UI elements")]
	[Tooltip("A prefab of a leaderboard row which is instantiated multiple times.")]
	[SerializeField] LeaderboardRowUI leaderboardRowPrefab;
	[Tooltip("A prefab of a leaderboard gap (indicating some places are left out).")]
	[SerializeField] GameObject leaderboardGapPrefab;
	[Tooltip("A Transform which is a parent of all the leaderboard rows.")]
	[SerializeField] Transform leaderboardRowsParent;


	/// <summary>
	/// Computes the player's place in the global leaderboard and then instantiates and initialized leaderboard rows accordingly,
	/// showing first few rows, and then the player together with one row above and one row below, leaving the rest out.
	/// </summary>
	/// <returns>The player's current place in the leaderboard.</returns>
	public int GetPlayerPlaceAndUpdateUI() {
		// Remove all existing rows
		UtilsMonoBehaviour.RemoveAllChildren(leaderboardRowsParent);

		PlayerLeaderboardData playerData = ComputePlayerData();
		// According to the place fill the table with rows and values
		if (playerData.place < 6) { // Show first 6 places (including player)
			AddOpponentsRange(1, playerData.place - 1);
			AddPlayer(playerData);
			AddOpponentsRange(playerData.place + 1, 6, true);
		} else { // Show first 3 places, then gap, and then player with neighbours
			AddOpponentsRange(1, 3);
			CreateGap();
			AddPlayerWithNeighbours(playerData);
		}
		// If the player is not the last one, add gap to the end
		if (playerData.place < leaderboard.opponentsCount) CreateGap();
		// Save all the currently known opponents
		SaveSystem.SaveKnownOpponents(PlayerState.Instance.knownOpponents);

		// Return the current place
		return playerData.place;
	}

	// Gets player's current data to be displayed in the leaderboard
	private PlayerLeaderboardData ComputePlayerData() {
		// Name, average, place
		string name = PlayerState.Instance.CharacterCustomization.characterName;
		float average = PlayerState.Instance.CurrentStats.GetWeightedAverage();
		int place = leaderboard.GetPlayerPlace(average);

		// Previous average and place
		float previousAverage = PlayerState.Instance.PreviousStats.GetWeightedAverage();
		int previousPlace = leaderboard.GetPlayerPlace(previousAverage);

		// Notify anyone interested that the current place changed
		if (place != previousPlace)
			Messaging.SendMessage("RankChanged", place);

		return new PlayerLeaderboardData {
			name = name,
			average = average,
			place = place,
			averageChange = average - previousAverage,
			placeChange = place - previousPlace
		};
	}

	// Adds rows for the opponents placed between the given range (inclusive)
	// ... LeaderboardRepresentation stores only opponents, so isAfterPlayer is needed to adjust the places
	private void AddOpponentsRange(int from, int to, bool isAfterPlayer = false) {
		if (from > to) return;
		for (int i = from; i <= to; i++) {
			AddOpponentRow(i, isAfterPlayer);
		}
	}

	// Adds a row for the opponent in the given place
	// ... LeaderboardRepresentation stores only opponents, so isAfterPlayer is needed to adjust the place
	private void AddOpponentRow(int place, bool isAfterPlayer = false) {
		int placeAmongOpponents = isAfterPlayer ? place - 1 : place;
		// Get name from a Dictionary or generate a new one
		string name = leaderboard.GetOpponentName(placeAmongOpponents);
		// Get average from the animation curve
		float average = leaderboard.GetOpponentAverage(placeAmongOpponents);
		CreateRow(place, name, average);
	}

	// Adds rows for the player and his immediate predecessor and successor
	private void AddPlayerWithNeighbours(PlayerLeaderboardData playerData) {
		AddOpponentRow(playerData.place - 1);
		AddPlayer(playerData);
		if (playerData.place <= leaderboard.opponentsCount)
			AddOpponentRow(playerData.place + 1, true);
	}

	// Adds a row for the player with the given data
	private void AddPlayer(PlayerLeaderboardData playerData) {
		LeaderboardRowUI playerRow = CreateRow(playerData.place, playerData.name, playerData.average);
		playerRow.SetPlayerData(playerData.placeChange, playerData.averageChange);
	}

	// Instantiates a row with the given content
	private LeaderboardRowUI CreateRow(int place, string name, float average) {
		LeaderboardRowUI row = Instantiate<LeaderboardRowUI>(leaderboardRowPrefab, leaderboardRowsParent);
		row.Initialize(place, name, average);
		return row;
	}

	// Instantiates a gap between leaderboard rows
	private void CreateGap() {
		Instantiate(leaderboardGapPrefab, leaderboardRowsParent);
	}

}

/// <summary>
/// A class containing parameters for the global leaderboard of racers,
/// e.g., a total number of racers, minimum and maximum stats average, a curve mapping rank to stats average.
/// </summary>
[System.Serializable]
public class LeaderboardRepresentation {

	[Tooltip("Total number of opponents in the leaderboard.")]
	public int opponentsCount = 157;
	[Tooltip("Animation Curve which is used to map opponent's place to their stats average.")]
	public AnimationCurve placeToAverageCurve;
	[Tooltip("The stats average of the weakest opponent.")]
	public float minOpponentAverage = 0.4f;
	[Tooltip("The stats average of the strongest opponent.")]
	public float maxOpponentAverage = 97.2f;


	/// <summary>
	/// Finds the player's rank among the opponents according to their stats average.
	/// </summary>
	/// <param name="average">The player's stats average (based on which they are ranked).</param>
	/// <returns>The player's place in the global leaderboard.</returns>
	public int GetPlayerPlace(float average) {
		// Handle corner cases
		if (average > GetOpponentAverage(1)) return 1;
		if (average < GetOpponentAverage(opponentsCount)) return opponentsCount + 1;
		// Binary search to find the two opponents between whom the player belongs
		int min = 1, max = opponentsCount; // min and max of the place interval we are searching
		int middle;
		float opponentAverage;
		while (max - min > 1) {
			middle = (max + min) / 2;
			opponentAverage = GetOpponentAverage(middle);
			if (average >= opponentAverage) { // player is better (move to the lower numbers)
				max = middle;
			} else { // player is worse (move to the higher numbers)
				min = middle;
			}
		}
		// We found the two
		return max; // player is between min and max, so on the (min + 1 = max)-th place
	}

	/// <summary>
	/// Maps the opponent's place in the global leaderboard to their stats average.
	/// </summary>
	/// <param name="place">Place in the leaderboard for which stats average is computed.</param>
	/// <returns>Stats average corresponding to the given place in the leaderboard.</returns>
	public float GetOpponentAverage(int place) {
		// Use the given place to get average from the AnimationCurve
		float t = 1 - Utils.RemapRange(place, 1f, opponentsCount, 0, 1); // curve has higher places in the lower values
		return Utils.RemapRange(placeToAverageCurve.Evaluate(t), 0, 1, minOpponentAverage, maxOpponentAverage);
	}

	/// <summary>
	/// Gets name of the opponent on the given place in the global loeaderboard.
	/// If opponent on that place is seen for the first time, a random name is generated.
	/// Otherwise, the name is already stored in the game state and obtained from there.
	/// </summary>
	/// <param name="place">Place on which the opponent is placed.</param>
	/// <returns>Name of the opponent on the given place.</returns>
	public string GetOpponentName(int place) {
		// Use the given place to get the opponent's name
		if (PlayerState.Instance.knownOpponents != null && PlayerState.Instance.knownOpponents.ContainsKey(place))
			return PlayerState.Instance.knownOpponents[place]; // opponent on this place has been seen before
		string name = NamesManagement.GetRandomName(); // a new random name
		PlayerState.Instance.knownOpponents[place] = name; // store the name among the already known ones
		return name;
	}

}

/// <summary>
/// A data structure containing the player's data necessary to display their results in the global leaderboard,
/// e.g., place, name, stats average.
/// </summary>
public struct PlayerLeaderboardData {
	/// <summary>The player's place in the global leaderboard.</summary>
	public int place;
	/// <summary>The player's name.</summary>
	public string name;
	/// <summary>The player's stats average.</summary>
	public float average;
	/// <summary>Change in the player's place since the last time.</summary>
	public int placeChange;
	/// <summary>Change in the player's stats average since the last time.</summary>
	public float averageChange;
}