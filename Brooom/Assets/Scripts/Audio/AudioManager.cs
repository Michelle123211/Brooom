using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviourSingleton<AudioManager>, ISingleton {

	// FMOD Events - to have access to them without having to use strings with path or any other direct dependency
	//		- Non-exhaustive list, only simple one-shots
	[field: SerializeField]
	public FMODEvents Events { get; private set; }

	[SerializeField] FMODUnity.StudioEventEmitter pauseMenuSnapshot;

	// VCA handles - to be able to change their volume easily
	FMOD.Studio.VCA masterVCA;
	FMOD.Studio.VCA musicVCA;
	FMOD.Studio.VCA ambienceVCA;
	FMOD.Studio.VCA soundEffectsVCA;
	FMOD.Studio.VCA gameSoundEffectsVCA;
	FMOD.Studio.VCA UISoundEffectsVCA;

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

	public void PlayOneShot(FMODUnity.EventReference eventReference) {
		FMODUnity.RuntimeManager.PlayOneShot(eventReference);
	}

	public void PlayOneShotAtPosition(FMODUnity.EventReference eventReference, Vector3 position) {
		FMODUnity.RuntimeManager.PlayOneShot(eventReference, position);
	}

	public void PlayOneShotAttached(FMODUnity.EventReference eventReference, GameObject gameObject) {
		FMODUnity.RuntimeManager.PlayOneShotAttached(eventReference, gameObject);
	}

	public void PauseGame() {
		pauseMenuSnapshot.Play();
	}

	public void ResumeGame() {
		pauseMenuSnapshot.Stop();
	}

	private void OnSceneLoading(Scene scene) {
		if (SceneLoader.Instance.CurrentScene != Scene.Start) // scene after Start is set only after it is loaded completely
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Scene", (float)scene); // global parameter
	}

	private void OnSceneLoaded(Scene scene) {
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Scene", (float)scene); // global parameter
	}

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
		Options = SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances;
	}

	public void AwakeSingleton() {
	}

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


public enum VCA {
	Master,
	Music,
	Ambience,
	SoundEffects,
	GameSoundEffects,
	UISoundEffects
}