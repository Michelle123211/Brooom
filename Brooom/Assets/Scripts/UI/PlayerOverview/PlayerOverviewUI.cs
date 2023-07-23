using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewUI : MonoBehaviour
{
    [SerializeField] RadarGraphUI statsGraph;

    public void ReturnToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartNextRace() {
        SceneLoader.Instance.LoadScene(Scene.Race);
    }

	private void OnEnable() {
        // Display player stats in a graph
        //statsGraph.DrawGraphValues(new List<float> {
        //    PlayerState.Instance.CurrentStats.endurance / 100f,
        //    PlayerState.Instance.CurrentStats.speed / 100f,
        //    PlayerState.Instance.CurrentStats.dexterity / 100f,
        //    PlayerState.Instance.CurrentStats.precision / 100f,
        //    PlayerState.Instance.CurrentStats.magic / 100f
        //    });
        statsGraph.DrawGraphValues(new List<float> { 0.2f, 0.5f, 1f, 0.1f, 0.3f });
    }
}
