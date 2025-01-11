using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewUI : MonoBehaviour
{

    [SerializeField] LeaderboardUI leaderboard;
    [SerializeField] RadarGraphUI statsGraph;
    [SerializeField] EquippedSpellsUI equippedSpells;
    [SerializeField] AchievementsUI achievements;
    [SerializeField] Color oldStatsColor;
    [SerializeField] Color newStatsColor;

    private bool isInitialized = false;

    public void ReturnToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartNextRace() {
        SceneLoader.Instance.LoadScene(Scene.Race);
    }

    public void UpdateUI() {
        int place = leaderboard.GetPlayerPlaceAndUpdateUI();
        UpdateGraph();
        equippedSpells.UpdateUI();
        achievements.UpdateUI(!isInitialized); // if isInitialized == true, then achievements will not be completely replaced, only updated

        // Show game ending (with a short delay) if the player is on the first place
        if (place == 1 && !PlayerState.Instance.GameComplete) {
            PlayerState.Instance.GameComplete = true;
            Invoke(nameof(ShowGameEnding), 3);
        }
    }

	private void OnEnable() {
        UpdateUI();
        isInitialized = true;
    }

    private void UpdateGraph() {
        // Display player stats in a graph (after a short delay so that the scene fade in does not hide it)
        statsGraph.ResetValues();
        List<float> oldValues = PlayerState.Instance.PreviousStats.GetListOfValues();
        // ... with localized graph labels
        statsGraph.Initialize(oldValues.Count, 100, GetLocalizedStatsGraphLabels(), GetLocalizedStatsDescription());
        // ... old values immediately (in grey)
        statsGraph.SetPolygonColor(oldStatsColor);
        statsGraph.SetPolygonBorder(true, oldStatsColor.WithA(1), 2);
        statsGraph.DrawGraphValues(oldValues);
        // ... new values tweened with a delay (in blue)
        Invoke(nameof(ShowCurrentPlayerStats), 1f);
    }

    private void ShowCurrentPlayerStats() {
        statsGraph.SetPolygonColor(newStatsColor);
        statsGraph.SetPolygonBorder(true, newStatsColor.WithA(1), 4);
        statsGraph.DrawGraphValuesTweened(PlayerState.Instance.CurrentStats.GetListOfValues(), PlayerState.Instance.PreviousStats.GetListOfValues(), true, "N0");
    }

    private List<string> GetLocalizedStatsGraphLabels() {
        List<string> statNames = PlayerStats.GetListOfStatNames();
        List<string> labels = new List<string>();
        foreach (var name in statNames) {
            // Take only the first letter
            labels.Add(LocalizationManager.Instance.GetLocalizedString("PlayerStat" + name)[0].ToString());
        }
        return labels;
    }

    private List<string> GetLocalizedStatsDescription() {
        List<string> statNames = PlayerStats.GetListOfStatNames();
        List<string> descriptions = new List<string>();
        foreach (var name in statNames) {
            descriptions.Add(LocalizationManager.Instance.GetLocalizedString("PlayerStatTooltip" + name));
        }
        return descriptions;
    }

    private void ShowGameEnding() {
        SceneLoader.Instance.LoadScene(Scene.Ending);
	}

}
