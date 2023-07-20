using System.Collections;
using System.Collections.Generic;
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
		// TODO: Get name
		// TODO: Compute current average
		// TODO: Compute player's place from average and a curve

		// TODO: Compute previous average
		// TODO: Compute previous player's place from previous average and a curve

		// TODO: Compute place change
		// TODO: Compute average change
		return new PlayerLeaderboardData {
			name = PlayerState.Instance.CharacterCustomization.playerName,
			average = 92.4f,
			place = 158,
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
	[Tooltip("Animation Curve which is used to map opponent's place to their average.")]
	public AnimationCurve placeToAverageCurve;
	[Tooltip("The average of the weakest opponent.")]
	public float minOpponentAverage = 0.4f;
	[Tooltip("The average of the strongest opponent.")]
	public float maxOpponentAverage = 97.2f;

	// Maps the opponent's place to their average
	public float GetOpponentAverage(int place) {
		// Use the given place to get average from the AnimationCurve
		float t = 1 - Utils.RemapRange(place, 1f, opponentsCount, 0, 1); // curve has higher places in the lower values
		return Utils.RemapRange(placeToAverageCurve.Evaluate(t), 0, 1, minOpponentAverage, maxOpponentAverage);
	}

	public string GetOpponentName(int place) {
		// TODO: Use the given place to get the opponent's name
		// TODO: Take into consideration already known names (from PlayerState.knownOpponents)
		// TODO: Otherwise load a list of possible names (if it is not loaded already)...
		// TODO: ...and choose a new one randomly
		// TODO: Store the name among the already known ones (in PlayerState.knownOpponents dictionary)
		return "Test";
	}
}

public struct PlayerLeaderboardData {
	public int place;
	public string name;
	public float average;
	public int placeChange;
	public float averageChange;
}