using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component representing a starting zone which the player has to enter during a training to start the race.
/// </summary>
public class StartingZone : MonoBehaviour
{
	[Tooltip("How many seconds the player must stay in the starting zone before the race starts.")]
	[SerializeField] float durationToStart;

	[Tooltip("Label displaying countdown to the race start.")]
	[SerializeField] TextMeshProUGUI countdownLabel;

	[Tooltip("A parent of all UI components shown on the screen (e.g., instructions to enter, countdown).")]
	[SerializeField] GameObject UIElements;

	private bool isCountingDown = false;
	private float currentTime = 0;
	private int secondsLeft = int.MaxValue;

	private string countdownText = string.Empty;


	/// <summary>
	/// Shows or hides all UI elements related to the starting zone (e.g., instructions to enter, countdown).
	/// </summary>
	/// <param name="show">Whether all UI elements should be shown (<c>true</c>) or hidden (<c>false</c>).</param>
	public void ShowOrHideUI(bool show) {
		UIElements.SetActive(show);
	}

	// Starts and displays countdown before entering a race, if the player has entered the trigger zone
	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player") && !other.isTrigger) { // only actual collider, not trigger around player to detect potential spell targets
			currentTime = durationToStart;
			secondsLeft = int.MaxValue;
			isCountingDown = true;
			// Initialize and show UI
			UpdateCountdownUI();
			Utils.TweenAwareEnable(countdownLabel.gameObject);
		}
	}

	// Stops and hides countdown before entering a race, if the player has exited the trigger zone
	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player") && !other.isTrigger) { // only actual collider, not trigger around player to detect potential spell targets
			isCountingDown = false;
			// Hide UI
			Utils.TweenAwareDisable(countdownLabel.gameObject);
		}
	}

	// Updates countdown before race (displaying remaining number of seconds)
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
				if (RaceControllerBase.Instance != null) RaceControllerBase.Instance.StartRace();
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
