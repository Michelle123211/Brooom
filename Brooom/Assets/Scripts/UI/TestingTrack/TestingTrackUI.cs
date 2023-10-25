using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTrackUI : MonoBehaviour {

    public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

	private void Update() {
		if (InputManager.Instance.GetBoolValue("Pause"))
			ReturnBack();
	}

}
