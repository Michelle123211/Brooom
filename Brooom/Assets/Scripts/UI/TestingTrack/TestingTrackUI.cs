using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class TestingTrackUI : MonoBehaviour {

	[Header("Basic info")]
	[SerializeField] TextMeshProUGUI speedText;
	[SerializeField] TextMeshProUGUI altitudeText;

	[Header("Spells")]
	[SerializeField] RaceSpellsUI spellsUI; // TODO: Delete, only temporary

	private CharacterMovementController playerMovementController;

	public void UpdatePlayerState() {
		speedText.text = Math.Round(playerMovementController.GetCurrentSpeed(), 1).ToString();
		altitudeText.text = Math.Round(playerMovementController.GetCurrentAltitude(), 1).ToString();
	}

	public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

	private void Start() {
		playerMovementController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterMovementController>("Player");
		spellsUI.Initialize(playerMovementController.gameObject); // initialize and show
	}

	private void Update() {
		if (InputManager.Instance.GetBoolValue("Pause"))
			ReturnBack();
		UpdatePlayerState();
	}

}
