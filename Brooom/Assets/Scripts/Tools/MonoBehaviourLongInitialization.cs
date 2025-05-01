using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An abstract class which should be extended by components requiring long initialization which cannot be done in a single frame 
/// (as is usually done in <c>Awake()</c> or <c>Start()</c> methods).
/// Instead a special method for long initialization is added and started as a coroutine, while yielding control back as often as possible.
/// If <c>SceneLoader</c> instance is located in the scene, it is made aware of such components
/// and doesn't hide loading screen until all components' initialization has finished completely.
/// </summary>
public abstract class MonoBehaviourLongInitialization : MonoBehaviour {

	/// <summary>Whether the object is initialized completely (i.e. its long initialization has finished). This should be checked before any further interaction.</summary>
	public bool IsInitialized { get; private set; } = false;
	/// <summary>Called when the long initialization has finished.</summary>
	public event Action onInitializationFinished;

	/// <summary>
	/// This method is started as a coroutine from the standard <c>Start()</c> method.
	/// It initializes the component over several frames while returning control back inbetween using <c>yield return</c>.
	/// </summary>
	/// <returns>Yields control back to allow other code to run inbetween (e.g. animation of loading screen).</returns>
	public IEnumerator Initialize() {
		yield return InitializeAfterPreparation();
		IsInitialized = true;
		onInitializationFinished?.Invoke();
	}

	private void Awake() {
		PrepareForInitialization_ReplacingAwake();
	}

	private void Start() {
		if (SceneLoader.Instance != null)
			SceneLoader.Instance.RegisterForLongInitialization(this); // let SceneLoader know there is an object with long initialization in the scene
		PrepareForInitialization_ReplacingStart();
		StartCoroutine(Initialize()); // start long initialization in a coroutine
	}

	private void Update() {
		// Don't do anything, unless the object initialization finished completely
		if (!IsInitialized) return;
		UpdateAfterInitialization();
	}

	// All the following methods are abstract, even though they can be empty, to emphasize that these should
	// be used instead of standard Awake(), Start() and Update() inherited from MonoBehaviour

	/// <summary>
	/// This method is used for a fast initialization which can be done in a single frame.
	/// It replaces <c>Awake()</c> method in derived classes (in fact, it is called from <c>Awake()</c> method of this class).
	/// </summary>
	protected abstract void PrepareForInitialization_ReplacingAwake();

	/// <summary>
	/// This method is used for a fast initialization which can be done in a single frame.
	/// It replaces <c>Start()</c> method in derived classes (in fact, it is called from <c>Start()</c> method of this class).
	/// </summary>
	protected abstract void PrepareForInitialization_ReplacingStart();

	/// <summary>
	/// This method is used for long initialization, i.e. anything which should not be done in a single frame.
	/// It should return control back as often as possible using <c>yield return</c>.
	/// </summary>
	/// <returns>Yields control back to allow other code to run inbetween (e.g. animation of loading screen).</returns>
	protected abstract IEnumerator InitializeAfterPreparation();


	/// <summary>
	/// This method replaces <c>Update()</c> method in derived classes (in fact, it is called from <c>Update()</c> method of this class).
	/// It ensures any code in update loop runs only after initialization has finished completely.
	/// </summary>
	protected abstract void UpdateAfterInitialization();

}
