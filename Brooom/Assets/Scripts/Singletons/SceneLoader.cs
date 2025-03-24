using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviourSingleton<SceneLoader>, ISingleton {

	public Scene CurrentScene { get; private set; } =  Scene.Start;

	public event Action<Scene> onSceneStartedLoading;
	public event Action<Scene> onSceneLoaded;

	[SerializeField] private Animator animator;

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

	#region Registration of objects with long initialization

	private int numberOfInitializedObjects = 0;
	private List<MonoBehaviourLongInitialization> objectsWithLongInitialization = new List<MonoBehaviourLongInitialization>();

	public void RegisterForLongInitialization(MonoBehaviourLongInitialization objectWithLongInitialization) {
		// Remember the object to invoke its initialization later
		objectsWithLongInitialization.Add(objectWithLongInitialization);
		objectWithLongInitialization.onInitializationFinished += IncreaseInitializedObjectCounter;
	}

	private void IncreaseInitializedObjectCounter() {
		numberOfInitializedObjects++;
	}

	private void ResetLongInitializationData() {
		foreach (var objectWithLongInitialization in objectsWithLongInitialization)
			objectWithLongInitialization.onInitializationFinished -= IncreaseInitializedObjectCounter;
		objectsWithLongInitialization.Clear();
		numberOfInitializedObjects = 0;
	}

	#endregion

	// Loads scene with the given name (using fade in/out and loading screen) - one parameter variant to be used in UnityEvent
	public void LoadScene(string sceneName) {
		LoadScene(Enum.Parse<Scene>(sceneName), true, true);
	}

	// Loads scene whose name is given by the enum value
	public void LoadScene(Scene scene, bool fade = true, bool showLoading = true) {
		StartCoroutine(LoadSceneAsync(scene, fade, showLoading));
	}

	private IEnumerator LoadSceneAsync(Scene scene, bool fade = true, bool showLoading = true) {
		GamePause.DisableGamePause(); // disable pause while loading screen is on
		onSceneStartedLoading?.Invoke(scene);
		// Enable cursor
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		// It does not make sense to display loading without the fade
		if (!fade && showLoading)
			showLoading = false;
		// Fade out to overlay
		if (fade) animator.SetTrigger("FadeOut");
		if (showLoading) animator.SetTrigger("LoadingIn");
		if (fade) {
			// Wait until the fade out animation starts playing
			yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FadeOut"));
			// Wait until the fade out animation is finished
			yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
		}
		if (showLoading) {
			// Wait until the loading animation starts playing
			yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("LoadingFadeIn"));
			// Wait until the fade out animation is completed fully
			yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
		}
		// Load next scene asycnhronously and activate it right away
		ResetLongInitializationData();
		AsyncOperation loadingScene = SceneManager.LoadSceneAsync(scene.ToString());
		loadingScene.allowSceneActivation = true;
		// Wait until the scene is loaded
		yield return new WaitUntil(() => loadingScene.isDone);
		CurrentScene = scene;
		PassAndResetCurrentParameters();
		// Wait until all registered objects with long initialization outside of Awake() or Start() methods are initialized
		Time.timeScale = 0;
		yield return new WaitUntil(() => numberOfInitializedObjects == objectsWithLongInitialization.Count); // objects should be already registered (from their Start() method)
		ResetLongInitializationData();
		Time.timeScale = 1;
		// Fade into the new scene
		if (fade) {
			animator.SetTrigger("FadeIn");
		}
		// Reset previous LoadingIn trigger in case it stayed set
		animator.ResetTrigger("LoadingIn");
		// Wait until the idle animation starts playing
		yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
		GamePause.EnableGamePause(); // enable pause again
		onSceneLoaded?.Invoke(scene);
	}

	static SceneLoader() { 
		Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances | SingletonOptions.LazyInitialization;
	}

	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		CurrentScene = Enum.Parse<Scene>(SceneManager.GetActiveScene().name);
	}

}


public enum Scene { 
	Start = 0,
	MainMenu = 1,
	CharacterCreation = 2,
	Tutorial = 3,
	PlayerOverview = 4,
	Race = 5,
	TestingTrack = 6,
	Ending = 7,
	Exit = 8,


	ProceduralTerrain = 9999 // TODO: Remove when not necessary (it is there for debugging purposes only)
}
