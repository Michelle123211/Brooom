using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// A component handling audio of <c>Slider</c> or <c>Scrollbar</c> changing its value.
/// It registers callback in <c>onValueChanged</c>, and makes sure that for continuous values the sound is not played every time, but at reasonable intervals.
/// </summary>
public class UIOnValueChangedAudio : MonoBehaviour {

	[Tooltip("Which audio event should be played.")]
	[SerializeField] FMODUnity.EventReference audioEvent;

	[Tooltip("For continuous sliders only (i.e. wholeNumbers == false), when changing a value, sound will be played in these intervals (in seconds).")]
	[SerializeField] float soundInterval = 0.1f;

	Slider slider;
	Scrollbar scrollbar;

	bool isInitialized = false; // indicating whether the initial value has been set (onValueChanged should not play sound for the initialization)

	// Callback actions are stored in fields so they can be unregistered
	UnityAction<float> playSoundAction;
	UnityAction<float> playSoundInIntervalsAction;

	float timeOfLastSound = 0; // for continuous slider, updated every time sound is played

	// Plays slider sound immediately
	private void PlaySound() {
		if (!isInitialized) return;
		AudioManager.Instance.PlayOneShot(audioEvent);
	}

	// Plays slider sound only if enough time has passed from the last time ot was played
	private void PlaySoundInIntervals() {
		if (!isInitialized) return;
		float currentTime = Time.unscaledTime; // game may be paused, so use unscaledTime instead of time
		if (currentTime - timeOfLastSound > soundInterval) {
			timeOfLastSound = currentTime;
			PlaySound();
		}
	}

	void Start() {
		// Initialize actions which will be used as callbacks
		playSoundAction = (_) => PlaySound();
		playSoundInIntervalsAction = (_) => PlaySoundInIntervals();
		// Register callback
		// ... for Slider if attached
		slider = GetComponent<Slider>();
		if (slider != null) {
			if (slider.wholeNumbers) slider.onValueChanged.AddListener(playSoundAction);
			else slider.onValueChanged.AddListener(playSoundInIntervalsAction);
		}
		// ... for Scrollbar if attached
		scrollbar = GetComponent<Scrollbar>();
		if (scrollbar != null)
			scrollbar.onValueChanged.AddListener(playSoundInIntervalsAction);
	}

	private void Update() {
		if (!isInitialized) isInitialized = true; // initial value is there, from now on we can play sound in any onValueChanged
	}

	private void OnDestroy() {
		// Unregister callbacks
		if (slider != null) {
			if (slider.wholeNumbers) slider.onValueChanged.RemoveListener(playSoundAction);
			else slider.onValueChanged.RemoveListener(playSoundInIntervalsAction);
		}
		if (scrollbar != null)
			scrollbar.onValueChanged.RemoveListener(playSoundInIntervalsAction);
	}
}
