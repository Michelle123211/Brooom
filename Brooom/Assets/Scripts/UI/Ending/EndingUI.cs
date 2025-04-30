using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component responsible for the UI of the game ending screen.
/// It plays sound effect and repeatedly (in random intervals) spawns stars on the screen around a cup.
/// </summary>
public class EndingUI : MonoBehaviour {

    EndingStarUI[] stars; // stars which can be repeatedly spawned on the screen
    bool[] isStarBeingSpawn; // whether the star on the corresponding index is already being spawned (is not idle and cennot be spawned again right now)

    float timeToNextStar = 1.0f; // time remaning until another start should be spawned

    /// <summary>
    /// Goes to the Player Overview scene from which the player can continue racing.
    /// </summary>
    public void ContinueRacing() {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Game continued after completion.");
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    /// <summary>
    /// Goes to the Main Menu scene.
    /// </summary>
    public void ReturnToMenu() {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Returning to menu after completion.");
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    // Spawns stars repeatedly in random intervals
	private void Update() {
        // Spawn a random star after a random interval
        timeToNextStar -= Time.deltaTime;
        if (timeToNextStar < 0) {
            // Choose a new time
            timeToNextStar += Random.Range(0.1f, 0.3f);
            // Choose a random star from the ones which are ready
            int readyStarsCount = 0;
            for (int i = 0; i < stars.Length; i++) {
                isStarBeingSpawn[i] = !stars[i].IsIdle();
                if (!isStarBeingSpawn[i]) readyStarsCount++;
            }
            int randomIndex = Random.Range(0, readyStarsCount);
            // Spawn the chosen star
            for (int i = 0; i < stars.Length; i++) {
                if (randomIndex == 0) {
                    stars[i].PlaySpawnAnimation();
                    break;
                }
                if (!isStarBeingSpawn[i]) randomIndex--;
            }
        }
	}

	private void Start() {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Game completed.");
        PlayerState.Instance.GameComplete = true;

        // Get all stars
        stars = GetComponentsInChildren<EndingStarUI>();
        isStarBeingSpawn = new bool[stars.Length];

        // Play applause sound
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.Applause);
    }

}
