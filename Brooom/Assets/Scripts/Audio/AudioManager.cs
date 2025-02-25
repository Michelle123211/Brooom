using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviourSingleton<AudioManager>, ISingleton {

	// VCA handles - to be able to change their volume easily
	FMOD.Studio.VCA masterVCA;
	FMOD.Studio.VCA musicVCA;
	FMOD.Studio.VCA ambienceVCA;
	FMOD.Studio.VCA soundEffectsVCA;

	public void ChangeVCAVolume(VCA chosenVCA, float volume) {
		FMOD.Studio.VCA VCAHandle = chosenVCA switch {
			VCA.Master => masterVCA,
			VCA.Music => musicVCA,
			VCA.Ambience => ambienceVCA,
			VCA.SoundEffects => soundEffectsVCA,
			_ => masterVCA
		};
		if (VCAHandle.isValid()) VCAHandle.setVolume(volume);
	}

	public void PlayOneShot(FMODUnity.EventReference eventReference) {
		FMODUnity.RuntimeManager.PlayOneShot(eventReference);
	}

	public void PlayOneShotAttached(FMODUnity.EventReference eventReference, GameObject gameObject) {
		FMODUnity.RuntimeManager.PlayOneShotAttached(eventReference, gameObject);
	}

	private void OnSceneLoading(Scene scene) {
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Scene", (float)scene); // global parameter
	}

	private FMOD.Studio.VCA GetVCAHandle(VCA chosenVCA) {
		return chosenVCA switch {
			VCA.Master => FMODUnity.RuntimeManager.GetVCA("vca:/Master"),
			VCA.Music => FMODUnity.RuntimeManager.GetVCA("vca:/Music"),
			VCA.Ambience => FMODUnity.RuntimeManager.GetVCA("vca:/Ambience"),
			VCA.SoundEffects => FMODUnity.RuntimeManager.GetVCA("vca:/SoundEffects"),
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
		// Setup VCA handles
		masterVCA = GetVCAHandle(VCA.Master);
		musicVCA = GetVCAHandle(VCA.Music);
		ambienceVCA = GetVCAHandle(VCA.Ambience);
		soundEffectsVCA = GetVCAHandle(VCA.SoundEffects);
	}

	#endregion
}


public enum VCA {
	Master,
	Music,
	Ambience,
	SoundEffects
}