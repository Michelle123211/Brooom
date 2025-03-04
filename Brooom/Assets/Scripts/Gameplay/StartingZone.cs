using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartingZone : MonoBehaviour
{
	[Tooltip("How many seconds the player must stay in tho starting zone before the race starts.")]
	[SerializeField] float durationToStart;

	[Tooltip("Label displaying countdown to the race start.")]
	[SerializeField] TextMeshProUGUI countdownLabel;

	private bool isCountingDown = false;
	private float currentTime = 0;
	private int secondsLeft = int.MaxValue;

	private string countdownText = string.Empty;

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player") && !other.isTrigger) {
			currentTime = durationToStart;
			secondsLeft = int.MaxValue;
			isCountingDown = true;
			// Initialize and show UI
			UpdateCountdownUI();
			Utils.TweenAwareEnable(countdownLabel.gameObject);
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player") && !other.isTrigger) {
			isCountingDown = false;
			// Hide UI
			Utils.TweenAwareDisable(countdownLabel.gameObject);
		}
	}

	private void UpdateCountdownUI() {
		int newSecondsLeft = Mathf.CeilToInt(currentTime);
		if (newSecondsLeft < secondsLeft) { // play countdown sound each second
			AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.CountdownStartingZone);
			secondsLeft = newSecondsLeft;
		}
		countdownLabel.text = string.Format(countdownText, newSecondsLeft);
	}

	private void Update() {
		if (isCountingDown) {
			currentTime -= Time.deltaTime;
			// Update UI
			UpdateCountdownUI();
			if (currentTime < 0) {
				// Start the race
				RaceController.Instance?.StartRace();
				// Destroy the whole starting zone
				Destroy(transform.parent.gameObject);
			}
		}
	}

	private void Start() {
		// Cache localized string
		countdownText = LocalizationManager.Instance.GetLocalizedString("RaceLabelEntering");
	}
}
