using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// A singleton providing functionality for loading scenes, passing parameters between scenes and showing a loading screen.
/// </summary>
public class SceneLoader : MonoBehaviourSingleton<SceneLoader>, ISingleton {

	/// <summary>Currently loaded scene (<c>Scene.None</c> if a scene is being loaded right now).</summary>
	public Scene CurrentScene { get; private set; } =  Scene.Start;

	/// <summary>Invoked when starting to load a scene. Parameter is the scene to be loaded.</summary>
	public event Action<Scene> onSceneStartedLoading;
	/// <summary>Invoked when loading a scene finishes. Parameter is the newly loaded scene.</summary>
	public event Action<Scene> onSceneLoaded;

	[SerializeField] private Animator animator; // for loading screen (fade out, loading text)

	#region Passing parameters between scenes

	private bool hasParameters = false; // whether some parameters have been set for the next scene
	private Dictionary<string, bool> boolParameters = new Dictionary<string, bool>(); // bool parameters to be passed to the next scene (name --> value)
	private Dictionary<string, int> intParameters = new Dictionary<string, int>(); // int parameters to be passed to the next scene (name --> value)
	private Dictionary<string, float> floatParameters = new Dictionary<string, float>(); // float parameters to be passed to the next scene (name --> value)
	private Dictionary<string, string> stringParameters = new Dictionary<string, string>(); // string parameters to be passed to the next scene (name --> value)

	/// <summary>
	/// Sets a <c>bool</c> parameter with the given name which will be passed to the next scene loaded.
	/// </summary>
	/// <param name="parameterName">Name of the parameter to set.</param>
	/// <param name="parameterValue">Value of the parameter.</param>
	public void SetBoolParameterForNextScene(string parameterName, bool parameterValue) {
		boolParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}
	/// <summary>
	/// Sets an <c>int</c> parameter with the given name which will be passed to the next scene loaded.
	/// </summary>
	/// <param name="parameterName">Name of the parameter to set.</param>
	/// <param name="parameterValue">Value of the parameter.</param>
	public void SetIntParameterForNextScene(string parameterName, int parameterValue) {
		intParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}
	/// <summary>
	/// Sets a <c>float</c> parameter with the given name which will be passed to the next scene loaded.
	/// </summary>
	/// <param name="parameterName">Name of the parameter to set.</param>
	/// <param name="parameterValue">Value of the parameter.</param>
	public void SetFloatParameterForNextScene(string parameterName, float parameterValue) {
		floatParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}
	/// <summary>
	/// Sets a <c>string</c> parameter with the given name which will be passed to the next scene loaded.
	/// </summary>
	/// <param name="parameterName">Name of the parameter to set.</param>
	/// <param name="parameterValue">Value of the parameter.</param>
	public void SetStringParameterForNextScene(string parameterName, string parameterValue) {
		stringParameters.Add(parameterName, parameterValue);
		hasParameters = true;
	}

	// Passes all stored parameters into the <c>SceneLoadingParameters</c> component found in the scene
	// Then deletes all currently stored parameters
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

	// Number of objects in the newly loaded scene which have long initialization (extend MonoBehaviourLongInitialization) and have already finished initializing
	private int numberOfInitializedObjects = 0;
	// A list of all (self-registered) objects in the newly loaded scene which have long initialization (extend MonoBehaviourLongInitialization)
	//	- it is necessary to wait for their initialization to finish before hiding the loading screen and fading into the scene
	private List<MonoBehaviourLongInitialization> objectsWithLongInitialization = new List<MonoBehaviourLongInitialization>();

	/// <summary>
	/// Remembers the object in a list and then later waits for its initialization to finish.
	/// </summary>
	/// <param name="objectWithLongInitialization">Object with long initialization to be registered.</param>
	public void RegisterForLongInitialization(MonoBehaviourLongInitialization objectWithLongInitialization) {
		// Remember the object to later wait for its initialization to finish
		objectsWithLongInitialization.Add(objectWithLongInitialization);
		objectWithLongInitialization.onInitializationFinished += IncreaseInitializedObjectCounter;
	}

	// Increases counter of initialized objects
	private void IncreaseInitializedObjectCounter() {
		numberOfInitializedObjects++;
	}

	// Resets all data related to objects with long initialization
	private void ResetLongInitializationData() {
		foreach (var objectWithLongInitialization in objectsWithLongInitialization)
			objectWithLongInitialization.onInitializationFinished -= IncreaseInitializedObjectCounter;
		objectsWithLongInitialization.Clear();
		numberOfInitializedObjects = 0;
	}

	#endregion

	/// <summary>
	/// Loads the scene with the given name (using fade in/out and loading screen). This method can be used in <c>UnityEvent</c>.
	/// </summary>
	/// <param name="sceneName">Name of the scene to be loaded.</param>
	public void LoadScene(string sceneName) {
		LoadScene(Enum.Parse<Scene>(sceneName), true, true);
	}

	/// <summary>
	/// Loads the scene whose name is given by the enum value.
	/// </summary>
	/// <param name="scene">Scene to be loaded.</param>
	/// <param name="fade">Whether to display fade out while loading.</param>
	/// <param name="showLoading">Whether to display loading screen content while loading (doesn't make sense without fade out).</param>
	public void LoadScene(Scene scene, bool fade = true, bool showLoading = true) {
		StartCoroutine(LoadSceneAsync(scene, fade, showLoading));
	}

	/// <summary>
	/// Asynchronously loads the scene whose name is given by the enum value.
	/// Handles fadeout and loading screen if requested. Also waits for objects with long initialization to finish before fading into the scene.
	/// Started as a coroutine so that it is possible to easily wait for something to finish.
	/// </summary>
	/// <param name="scene">Scene to be loaded.</param>
	/// <param name="fade">Whether to display fade out while loading.</param>
	/// <param name="showLoading">Whether to display loading screen content while loading (doesn't make sense without fade out).</param>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	private IEnumerator LoadSceneAsync(Scene scene, bool fade = true, bool showLoading = true) {
		GamePause.DisableGamePause(); // disable pause while loading screen is on
		CurrentScene = Scene.None;
		onSceneStartedLoading?.Invoke(scene);
		Utils.EnableCursor();
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
		// Load next scene asynchronously and activate it right away
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
		// Singleton options override
		Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances | SingletonOptions.LazyInitialization;
	}

	/// <inheritdoc/>
	public void AwakeSingleton() {
	}

	/// <inheritdoc/>
	public void InitializeSingleton() {
		CurrentScene = Enum.Parse<Scene>(SceneManager.GetActiveScene().name);
	}

}


/// <summary>
/// All different scenes in the game, supported by <c>SceneLoader</c>.
/// </summary>
public enum Scene { 
	Start = 0,
	MainMenu = 1,
	CharacterCreation = 2,
	Tutorial = 3,
	PlayerOverview = 4,
	Race = 5,
	QuickRace = 6,
	TestingTrack = 7,
	Ending = 8,
	Exit = 9,
	LevelGeneratorDemo = 10,

	None = 8888
}
