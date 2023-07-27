using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour {

	[Header("Parameters")]
	[Tooltip("Parameters of opponents in the leaderboard.")]
	public LeaderboardRepresentation leaderboard;

	[Header("UI elements")]
	[Tooltip("A prefab of a leaderboard row which is instantiated multiple times.")]
	[SerializeField] LeaderboardRowUI leaderboardRowPrefab;
	[Tooltip("A prefab of a leaderboard gap (indicating some places are left out).")]
	[SerializeField] GameObject leaderboardGapPrefab;
	[Tooltip("A Transform which is a parent of all the leaderboard rows.")]
	[SerializeField] Transform leaderboardRowsParent;

	public void UpdateUI() {
		// Remove all existing rows
		UtilsMonoBehaviour.RemoveAllChildren(leaderboardRowsParent);

		PlayerLeaderboardData playerData = ComputePlayerData();
		// According to the place fill the table with rows and values
		if (playerData.place < 9) {
			AddOpponentsRange(1, playerData.place - 1);
			AddPlayer(playerData);
			AddOpponentsRange(playerData.place + 1, 9, true);
		} else {
			AddOpponentsRange(1, 6); // place >= 9 ==> show the first 6, then gap, player with the neighbours
			CreateGap();
			AddPlayerWithNeighbours(playerData);
		}
		// If the player is not the last one, add gap to the end
		if (playerData.place < leaderboard.opponentsCount) CreateGap();
	}

	private PlayerLeaderboardData ComputePlayerData() {
		// Name, average, place
		string name = PlayerState.Instance.CharacterCustomization.playerName;
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
	// ... LeaderboardRepresentation stores only ordered opponents, so isAfterPlayer is needed to adjust the places
	private void AddOpponentsRange(int from, int to, bool isAfterPlayer = false) {
		if (from > to) return;
		for (int i = from; i <= to; i++) {
			AddOpponentRow(i, isAfterPlayer);
		}
	}

	// Adds a row for the opponent in the given place
	// ... LeaderboardRepresentation stores only ordered opponents, so isAfterPlayer is needed to adjust the place
	private void AddOpponentRow(int place, bool isAfterPlayer = false) {
		int placeAmongOpponents = isAfterPlayer ? place - 1 : place;
		// Get name from a List and Dictionary
		string name = leaderboard.GetOpponentName(placeAmongOpponents);
		// Get average from the aniamtion curve
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

	// Instantiates a gap
	private void CreateGap() {
		Instantiate(leaderboardGapPrefab, leaderboardRowsParent);
	}
}

[System.Serializable]
public class LeaderboardRepresentation {
	[Tooltip("Total number of opponents.")]
	public int opponentsCount = 157;
	[Tooltip("Name of the file in the StreamingAssets folder containing possible names for the opponents.")]
	public string namesFilename = "names.txt";
	[Tooltip("Animation Curve which is used to map opponent's place to their average.")]
	public AnimationCurve placeToAverageCurve;
	[Tooltip("The average of the weakest opponent.")]
	public float minOpponentAverage = 0.4f;
	[Tooltip("The average of the strongest opponent.")]
	public float maxOpponentAverage = 97.2f;

	private List<string> possibleNames;

	// Finds the player's place among the opponents according to their average
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

	// Maps the opponent's place to their average
	public float GetOpponentAverage(int place) {
		// Use the given place to get average from the AnimationCurve
		float t = 1 - Utils.RemapRange(place, 1f, opponentsCount, 0, 1); // curve has higher places in the lower values
		return Utils.RemapRange(placeToAverageCurve.Evaluate(t), 0, 1, minOpponentAverage, maxOpponentAverage);
	}

	public string GetOpponentName(int place) {
		// Use the given place to get the opponent's name
		if (PlayerState.Instance.knownOpponents != null && PlayerState.Instance.knownOpponents.ContainsKey(place))
			return PlayerState.Instance.knownOpponents[place]; // opponent on this place has been seen before
		if (possibleNames == null || possibleNames.Count == 0)
			possibleNames = NamesManagement.LoadNames(namesFilename);
		string name = possibleNames[UnityEngine.Random.Range(0, possibleNames.Count)]; // a new random name
		PlayerState.Instance.knownOpponents[place] = name; // store the name among the already known ones
		return name;
	}
}

public struct PlayerLeaderboardData {
	public int place;
	public string name;
	public float average;
	public int placeChange;
	public float averageChange;
}