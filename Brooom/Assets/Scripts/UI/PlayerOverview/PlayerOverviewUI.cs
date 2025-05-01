using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing UI of the Player Overview screen.
/// It provides methods for different buttons in the screen and makes sure all UI elements show current values when the screen is displayed.
/// </summary>
public class PlayerOverviewUI : MonoBehaviour {

    [Tooltip("Component displaying current amount of coins.")]
    [SerializeField] CoinsUI coins;
    [Tooltip("Component displaying global leaderboard of racers.")]
    [SerializeField] LeaderboardUI leaderboard;
    [Tooltip("Component displaying radar graph with the player's current stats.")]
    [SerializeField] RadarGraphUI statsGraph;
    [Tooltip("Component displaying slots with equipped spells.")]
    [SerializeField] EquippedSpellsUI equippedSpells;
    [Tooltip("Component displaying all known and unknown achievements.")]
    [SerializeField] AchievementsUI achievements;
    [Tooltip("Color used for a radar graph polygon depicting old stats values.")]
    [SerializeField] Color oldStatsColor;
    [Tooltip("Color used for a radar graph polygon depicting new stats values.")]
    [SerializeField] Color newStatsColor;


    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    public void ReturnToMenu() {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Returned to Main Menu.");
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    /// <summary>
    /// Loads the Race scene.
    /// </summary>
    public void StartNextRace() {
        Analytics.Instance.LogEvent(AnalyticsCategory.Race, "New race entered from Player Overview.");
        SceneLoader.Instance.LoadScene(Scene.Race);
    }

    /// <summary>
    /// Updates all UI elements in the Player Overview screen to show current values, i.e. coins, leaderboard, stats graph, equipped spells, achievements.
    /// Also detects if the player has completed the game, then it loads the Ending scene.
    /// </summary>
    public void UpdateUI() {
        coins.UpdateCoinsAmount(0, PlayerState.Instance.Coins);
        int place = leaderboard.GetPlayerPlaceAndUpdateUI();
        UpdateGraph();
        equippedSpells.UpdateUI();
        achievements.UpdateUI();

        // Show game ending (with a short delay) if the player is on the first place
        if (place == 1 && !PlayerState.Instance.GameComplete) {
            PlayerState.Instance.GameComplete = true;
            Invoke(nameof(ShowGameEnding), 1);
        }
    }

	private void OnEnable() {
        UpdateUI();
    }

    // Displays old and new player's stats in a graph (after a short delay so that the scene fade in does not hide it)
    private void UpdateGraph() {
        statsGraph.ResetValues();
        List<float> oldValues = PlayerState.Instance.PreviousStats.GetListOfValues();
        // Localized graph labels
        statsGraph.Initialize(oldValues.Count, 100, GetLocalizedStatsGraphLabels(), GetLocalizedStatsDescription());
        // Display old values immediately
        statsGraph.SetPolygonColor(oldStatsColor);
        statsGraph.SetPolygonBorder(true, oldStatsColor.WithA(1), 2);
        statsGraph.DrawGraphValues(oldValues);
        // Tween new values with a delay
        Invoke(nameof(ShowCurrentPlayerStats), 1f);
    }

    // Displays new player's stats in a graph (tweened starting from the previous values)
    private void ShowCurrentPlayerStats() {
        statsGraph.SetPolygonColor(newStatsColor);
        statsGraph.SetPolygonBorder(true, newStatsColor.WithA(1), 4);
        statsGraph.DrawGraphValuesTweened(PlayerState.Instance.CurrentStats.GetListOfValues(), PlayerState.Instance.PreviousStats.GetListOfValues(), true, "N0");
    }

    // Gets localized stats' names and takes their first letter as an axis label
    private List<string> GetLocalizedStatsGraphLabels() {
        List<string> statNames = PlayerStats.GetListOfStatNames();
        List<string> labels = new();
        foreach (var name in statNames) {
            // Take only the first letter
            labels.Add(LocalizationManager.Instance.GetLocalizedString("PlayerStat" + name)[0].ToString());
        }
        return labels;
    }

    // Gets localized stats' descriptions (which will be used in a tooltip)
    private List<string> GetLocalizedStatsDescription() {
        List<string> statNames = PlayerStats.GetListOfStatNames();
        List<string> descriptions = new();
        foreach (var name in statNames) {
            descriptions.Add(LocalizationManager.Instance.GetLocalizedString("PlayerStatTooltip" + name));
        }
        return descriptions;
    }

    // Loads Ending scene
    private void ShowGameEnding() {
        SceneLoader.Instance.LoadScene(Scene.Ending);
	}

}
