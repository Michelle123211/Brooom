using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTrackUI : MonoBehaviour {

	[SerializeField] RaceSpellsUI spellsUI; // TODO: Delete, only temporary

	public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

	private void Start() {
		CharacterRaceState playerRaceState = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterRaceState>("Player");
		spellsUI.Initialize(playerRaceState.gameObject); // initialize and show
	}

	private void Update() {
		if (InputManager.Instance.GetBoolValue("Pause"))
			ReturnBack();
	}

}
