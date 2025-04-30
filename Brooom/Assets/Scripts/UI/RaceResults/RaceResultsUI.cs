using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component responsible for displaying results at the end of the race.
/// It gets the race results data from <c>RaceControllerBase</c> implementation
/// and then instantiates several <c>RaceResultRowUI</c>s to display them.
/// </summary>
public class RaceResultsUI : MonoBehaviour {

    [Header("Results table")]
    [Tooltip("A parent object of all the results rows.")]
    [SerializeField] Transform resultRowsParent;
    [Tooltip("A prefab of a results table row which is instantiated several times.")]
    [SerializeField] RaceResultRowUI resultRowPrefab;

    int playerIndex; // index of the results row belonging to the player

    /// <summary>
    /// Goes to the Player Overview scene.
    /// </summary>
    public void GoToPlayerOverview() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    /// <summary>
    /// Instantiates rows according to the given race results while highlighting the row belonging to the player.
    /// </summary>
    /// <param name="results">An array of race results data of all racers (sorted according to their place in race).</param>
    /// <param name="playerPlace">The player's place in race.</param>
    public void SetResultsTable(RaceResultData[] results, int playerPlace) {
        playerIndex = playerPlace - 1;
        // Remove all existing rows from the table
        UtilsMonoBehaviour.RemoveAllChildren(resultRowsParent);
        // Instantiate rows according to the results
        for (int i = 0; i < results.Length; i++) {
            RaceResultRowUI row = Instantiate<RaceResultRowUI>(resultRowPrefab, resultRowsParent);
            row.Initialize(i + 1, results[i]);
            // Highlight player
            if (i == playerIndex)
                row.HighlightPlayer();
        }
    }

    /// <summary>
    /// Shows race results by showing invidividual rows one by one from the first place to the last one.
    /// </summary>
    public void ShowResults() {
        StartCoroutine(nameof(ShowResultsOneByOne));
    }

    // Shows results rows gradually one by one starting from the first place
    private IEnumerator ShowResultsOneByOne() {
        for (int i = 0; i < resultRowsParent.childCount; i++) {
            AudioManager.Instance.PlayOneShot(
                i == playerIndex
                    ? AudioManager.Instance.Events.GUI.RaceResultPlayer
                    : AudioManager.Instance.Events.GUI.RaceResult
            );
            resultRowsParent.GetChild(i).gameObject.TweenAwareEnable();
            yield return new WaitForSeconds(0.2f);
        }
    }

	private void OnEnable() {
        Utils.EnableCursor();
    }

	private void Update() {
        // Enable cursor if not already
        if (Utils.IsCursorLocked()) {
            Utils.EnableCursor();
        }
    }
}

/// <summary>
/// A data structure containing all race results data for a single racer, e.g. color assigned to that race, name, time, reward.
/// </summary>
public struct RaceResultData {
    public Color color;
    public string name;
    public float time;
    public float timePenalization;
    public int coinsReward;
    public int coinsPenalization;
}