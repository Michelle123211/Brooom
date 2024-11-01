using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviourSingleton<SceneLoader>, ISingleton {

	[HideInInspector]
	public string currentScene = Scene.Start.ToString();

	[SerializeField] private Animator animator;

	public event Action onSceneLoaded;

	#region Passing parameters between scenes

	private bool hasParameters = false;
	private Dictionary<string, bool> boolParameters = new Dictionary<string, bool>();
	private Dictionary<string, int> intParameters = new Dictionary<string, int>();
	private Dictionary<string, float> floatParameters = new Dictionary<string, float>();
	private Dictionary<string, string> stringParameters = new Dictionary<string, string>();

	public void SetBoolParameterForNextScene(string parameterName, bool parameterValue) {
		boolParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}
	public void SetIntParameterForNextScene(string parameterName, int parameterValue) {
		intParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}
	public void SetFloatParameterForNextScene(string parameterName, float parameterValue) {
		floatParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}
	public void SetStringParameterForNextScene(string parameterName, string parameterValue) {
		stringParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}

	private void PassAndResetCurrentParameters() {
		if (!hasParameters) return;
		SceneLoadingParameters sceneLoadingParameters = FindObjectOfType<SceneLoadingParameters>();
		// Pass parameters
		if (sceneLoadingParameters != null) {
			foreach (var boolParameter in boolParameters) sceneLoadingParameters.PassBoolParameter(boolParameter.Key, boolParameter.Value);
			foreach (var intParameter in intParameters) sceneLoadingParameters.PassIntParameter(intParameter.Key, intParameter.Value);
			foreach (var floatParameter in floatParameters) sceneLoadingParameters.PassFloatParameter(floatParameter.Key, floatParameter.Value);
			foreach (var stringParameter in stringParameters) sceneLoadingParameters.PassStringParameter(stringParameter.Key, stringParameter.Value);
		}
		// Reset parameters
		boolParameters.Clear();
		intParameters.Clear();
		floatParameters.Clear();
		stringParameters.Clear();
		hasParameters = false;
	}

	#endregion

	// Loads scene with the given name (using fade in/out and loading screen) - one parameter variant to be used in UnityEvent
	public void LoadScene(string sceneName) {
		LoadScene(sceneName, true, true);
	}

	// Loads scene whose name is given by the enum value
	public void LoadScene(Scene scene, bool fade = true, bool showLoading = true) {
		LoadScene(scene.ToString(), fade, showLoading);
	}

	// Loads scene with the given name
	public void LoadScene(string sceneName, bool fade = true, bool showLoading = true) {
		StartCoroutine(LoadSceneAsync(sceneName, fade, showLoading));
	}

	private IEnumerator LoadSceneAsync(string sceneName, bool fade = true, bool showLoading = true) {
		// Enable cursor
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		// It does not make sense to display loading without the fade
		if (!fade && showLoading)
			showLoading = false;
		// Load the next scene asynchronously and don't activate it immediately
		AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneName);
		loadingScene.allowSceneActivation = false;
		// Fade out to overlay
		if (fade) animator.SetTrigger("FadeOut");
		if (showLoading) animator.SetTrigger("LoadingIn");
		if (fade) {
			// Wait until the fade out animation starts playing
			yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FadeOut"));
			// Wait until the fade out animation is finished
			yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
		}
		// Enable activation of the scene
		loadingScene.allowSceneActivation = true;
		// Wait until the scene is loaded
		yield return new WaitUntil(() => loadingScene.isDone);
		currentScene = sceneName;
		PassAndResetCurrentParameters();
		// Fade into the new scene
		if (fade) {
			animator.SetTrigger("FadeIn");
		}
		// Reset previous LoadingIn trigger in case it stayed set
		animator.ResetTrigger("LoadingIn");
		// Call registered callbacks
		onSceneLoaded?.Invoke();
	}

	public void InitializeSingleton() {
		currentScene = SceneManager.GetActiveScene().name;
	}

	public void AwakeSingleton() {
	}

	protected override void SetSingletonOptions() {
		Options = (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances | (int)SingletonOptions.LazyInitialization;
	}
}


public enum Scene { 
	MainMenu,
	CharacterCreation,
	Tutorial,
	Race,
	PlayerOverview,
	TestingTrack,
	Ending,
	Start,
	Exit
}
