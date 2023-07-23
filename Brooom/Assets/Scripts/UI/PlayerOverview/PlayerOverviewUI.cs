using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewUI : MonoBehaviour
{
    [SerializeField] RadarGraphUI statsGraph;
    [SerializeField] Color oldStatsColor;
    [SerializeField] Color newStatsColor;

    public void ReturnToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartNextRace() {
        SceneLoader.Instance.LoadScene(Scene.Race);
    }

	private void OnEnable() {
        // Display player stats in a graph (after a short delay so that the scene fade in does not hide it)
        statsGraph.Initialize();
        // ... old values immediately (in grey)
        statsGraph.SetPolygonColor(oldStatsColor);
        statsGraph.SetPolygonBorder(true, oldStatsColor.WithA(1), 2);
        statsGraph.DrawGraphValues(new List<float> { 0.2f, 0.5f, 1f, 0.1f, 0.3f });
        // ... new values tweened with a delay (in blue)
        Invoke(nameof(ShowPlayerStats), 1f);
    }

    private void ShowPlayerStats() {
        statsGraph.SetPolygonColor(newStatsColor);
        statsGraph.SetPolygonBorder(true, newStatsColor.WithA(1), 4);
        //statsGraph.DrawGraphValues(new List<float> {
        //    PlayerState.Instance.CurrentStats.endurance / 100f,
        //    PlayerState.Instance.CurrentStats.speed / 100f,
        //    PlayerState.Instance.CurrentStats.dexterity / 100f,
        //    PlayerState.Instance.CurrentStats.precision / 100f,
        //    PlayerState.Instance.CurrentStats.magic / 100f
        //    });
        //statsGraph.DrawGraphValues(new List<float> { 0.2f, 0.5f, 1f, 0.1f, 0.3f });
        statsGraph.DrawGraphValuesTweened(new List<float> { 1f, 0.8f, 1f, 1f, 0.9f }, new List<float> { 0.2f, 0.5f, 1f, 0.1f, 0.3f });
        //statsGraph.DrawGraphValuesTweened(new List<float> { 0.2f, 0.5f, 1f, 0.1f, 0.3f }, new List<float> { 0f, 0f, 0f, 0f, 0f });
    }
}
