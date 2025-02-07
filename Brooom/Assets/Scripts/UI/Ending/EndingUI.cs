using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingUI : MonoBehaviour {

    EndingStarUI[] stars;
    bool[] isStarBeingSpawn;

    float timeToNextStar = 1.0f;

    public void ContinueRacing() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    public void ReturnToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

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
        PlayerState.Instance.GameComplete = true;

        // Get all stars
        stars = GetComponentsInChildren<EndingStarUI>();
        isStarBeingSpawn = new bool[stars.Length];
    }

}
