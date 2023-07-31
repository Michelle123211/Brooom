using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingUI : MonoBehaviour {
    public void ContinueRacing() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    public void ReturnToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

	private void Start() {
		// TODO: Mark in PlayerState that the game is complete
	}
}
