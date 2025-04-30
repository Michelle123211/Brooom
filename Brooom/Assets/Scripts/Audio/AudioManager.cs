using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An audio manager representation providing methods for audio interactions (e.g., changing VCA volume, playing one-shot).
/// </summary>
public class AudioManager : MonoBehaviourSingleton<AudioManager>, ISingleton {

	[field:Tooltip("FMOD events references for easy access from code.")]
	[field: SerializeField]
	public FMODEvents Events { get; private set; }

	[Tooltip("StudioEventEmitter playing PauseMenu snapshot to alter audio when the game is paused.")]
	[SerializeField] FMODUnity.StudioEventEmitter pauseMenuSnapshot;

	// VCA handles - to be able to change their volume easily
	FMOD.Studio.VCA masterVCA;
	FMOD.Studio.VCA musicVCA;
	FMOD.Studio.VCA ambienceVCA;
	FMOD.Studio.VCA soundEffectsVCA;
	FMOD.Studio.VCA gameSoundEffectsVCA;
	FMOD.Studio.VCA UISoundEffectsVCA;

	/// <summary>
	/// Changes volume of the given VCA (to adjust volume of a whole group of sounds at once).
	/// </summary>
	/// <param name="chosenVCA">VCA whose volume to change.</param>
	/// <param name="volume">New volume value.</param>
	public void ChangeVCAVolume(VCA chosenVCA, float volume) {
		FMOD.Studio.VCA VCAHandle = chosenVCA switch {
			VCA.Master => masterVCA,
			VCA.Music => musicVCA,
			VCA.Ambience => ambienceVCA,
			VCA.SoundEffects => soundEffectsVCA,
			VCA.GameSoundEffects => gameSoundEffectsVCA,
			VCA.UISoundEffects => UISoundEffectsVCA,
			_ => masterVCA
		};
		if (VCAHandle.isValid()) VCAHandle.setVolume(volume);
	}

	/// <summary>
	/// Plays the given FMOD event as a 2D one-shot without position in space.
	/// </summary>
	/// <param name="eventReference">Reference to FMOD event which should be played.</param>
	public void PlayOneShot(FMODUnity.EventReference eventReference) {
		FMODUnity.RuntimeManager.PlayOneShot(eventReference);
	}

	/// <summary>
	/// Plays the given FMOD event as a 3D one-shot at the given position.
	/// </summary>
	/// <param name="eventReference">Reference to FMOD event which should be played.</param>
	/// <param name="position">Position in 3D space at which the sound should be played.</param>
	public void PlayOneShotAtPosition(FMODUnity.EventReference eventReference, Vector3 position) {
		FMODUnity.RuntimeManager.PlayOneShot(eventReference, position);
	}

	/// <summary>
	/// Plays the given FMOD event as a 3D one-shot attached to the given game object.
	/// </summary>
	/// <param name="eventReference">Reference to FMOD event which should be played.</param>
	/// <param name="gameObject">Game object to which the sound should be attached.</param>
	public void PlayOneShotAttached(FMODUnity.EventReference eventReference, GameObject gameObject) {
		FMODUnity.RuntimeManager.PlayOneShotAttached(eventReference, gameObject);
	}

	/// <summary>
	/// Starts playing the PauseMenu snapshot to alter audio when the game is paused.
	/// </summary>
	public void PauseGame() {
		pauseMenuSnapshot.Play();
	}

	/// <summary>
	/// Stops playing the PauseMenu snapshot to restore the original audio when the game is not paused.
	/// </summary>
	public void ResumeGame() {
		pauseMenuSnapshot.Stop();
	}

	// Sets Scene parameter value when the scene changes (this affects volume of different groups of audio, e.g. music is louder outside of race).
	private void OnSceneLoading(Scene scene) {
		if (SceneLoader.Instance.CurrentScene != Scene.Start) // scene after Start is set only after it is loaded completely
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Scene", (float)scene); // global parameter
	}

	// Sets Scene parameter value when the scene changes (this affects volume of different groups of audio, e.g. music is louder outside of race).
	private void OnSceneLoaded(Scene scene) {
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Scene", (float)scene); // global parameter
	}

	// Gets VCA handle based on the VCA enum value
	private FMOD.Studio.VCA GetVCAHandle(VCA chosenVCA) {
		return chosenVCA switch {
			VCA.Master => FMODUnity.RuntimeManager.GetVCA("vca:/Master"),
			VCA.Music => FMODUnity.RuntimeManager.GetVCA("vca:/Music"),
			VCA.Ambience => FMODUnity.RuntimeManager.GetVCA("vca:/Ambience"),
			VCA.SoundEffects => FMODUnity.RuntimeManager.GetVCA("vca:/SoundEffects"),
			VCA.GameSoundEffects => FMODUnity.RuntimeManager.GetVCA("vca:/GameSoundEffects"),
			VCA.UISoundEffects => FMODUnity.RuntimeManager.GetVCA("vca:/UISoundEffects"),
			_ => FMODUnity.RuntimeManager.GetVCA("vca:/Master")
		};
	}

	#region Singleton

	static AudioManager() {
		// Singleton options override
		Options = SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances;
	}

	/// <inheritdoc/>
	public void AwakeSingleton() {
	}

	/// <inheritdoc/>
	public void InitializeSingleton() {
		SceneLoader.Instance.onSceneStartedLoading += OnSceneLoading;
		SceneLoader.Instance.onSceneLoaded += OnSceneLoaded;
		// Setup VCA handles
		masterVCA = GetVCAHandle(VCA.Master);
		musicVCA = GetVCAHandle(VCA.Music);
		ambienceVCA = GetVCAHandle(VCA.Ambience);
		soundEffectsVCA = GetVCAHandle(VCA.SoundEffects);
		gameSoundEffectsVCA = GetVCAHandle(VCA.GameSoundEffects);
		UISoundEffectsVCA = GetVCAHandle(VCA.UISoundEffects);
	}

	#endregion
}

/// <summary>
/// Different types of supported VCAs.
/// </summary>
public enum VCA {
	Master,
	Music,
	Ambience,
	SoundEffects,
	GameSoundEffects,
	UISoundEffects
}