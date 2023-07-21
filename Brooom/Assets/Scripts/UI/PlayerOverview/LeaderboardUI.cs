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

	private PlayerLeaderboardData ComputePlayerData() {
		// Name, average, place
		string name = PlayerState.Instance.CharacterCustomization.playerName;
		float average = PlayerState.Instance.stats.GetWeightedAverage();
		int place = leaderboard.GetPlayerPlace(average);

		// Chenge - from previous average and place
		// TODO: Compute previous average
		// TODO: Compute previous player's place from previous average and a curve

		// TODO: Compute place change
		// TODO: Compute average change
		return new PlayerLeaderboardData {
			name = name,
			average = average,
			place = place,
			averageChange = -2.3f,
			placeChange = 5
		};
	}

	private void RemoveAllRows() {
		for (int i = leaderboardRowsParent.childCount - 1; i >= 0; i--) {
			Destroy(leaderboardRowsParent.GetChild(i).gameObject);
		}
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

	private void OnEnable() {
		RemoveAllRows();
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
			LoadPossibleNames();
		string name = possibleNames[UnityEngine.Random.Range(0, possibleNames.Count)]; // a new random name
		PlayerState.Instance.knownOpponents[place] = name; // store the name among the already known ones
		return name;
	}

	private void LoadPossibleNames() {
		possibleNames = new List<string>();
		// Load list of names from a file
		if (string.IsNullOrEmpty(namesFilename)) { // empty filename
			Debug.LogError($"An empty filename was given for the file containing a list of names.");
		} else {
			string path = Path.Combine(Application.streamingAssetsPath, namesFilename);
			try {
				if (!File.Exists(path)) { // file does not exist
					Debug.LogError($"The file '{namesFilename}' does not exist in the StreamingAssets.");
				} else using (StreamReader reader = new StreamReader(path)) { // reading the file
					ParseNamesFromReader(reader);
				}
			} catch (IOException ex) { // exception while reading
				Debug.LogError($"An exception occurred while reading the file '{namesFilename}': {ex.Message}");
			}
		}
		// Make sure there is at least one name
		if (possibleNames.Count == 0)
			UseDefaultNames();
	}

	private void ParseNamesFromReader(StreamReader reader) {
		string line;
		while ((line = reader.ReadLine()) != null) {
			if (string.IsNullOrEmpty(line)) continue; // skip empty rows
			if (line[0] == '#') continue; // skip lines beginning with # (denotes names sections)
			string name = line.Trim();
			// Take only first 20 characters if the name is longer than that
			if (name.Length > 20) name = name.Substring(0, 20);
			possibleNames.Add(name);
		}
	}

	private void UseDefaultNames() {
		possibleNames.Add("Emil");
	}
}

public struct PlayerLeaderboardData {
	public int place;
	public string name;
	public float average;
	public int placeChange;
	public float averageChange;
}