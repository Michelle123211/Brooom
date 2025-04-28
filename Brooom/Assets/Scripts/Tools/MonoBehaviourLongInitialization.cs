using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoBehaviourLongInitialization : MonoBehaviour {

	// Whether the object is initialized completely
	// Should be checked before any interaction with this object
	public bool IsInitialized { get; private set; } = false;

	public event Action onInitializationFinished;

	// This method is called from SceneLoader to initialize the object after the standard Awake() and Start() have taken place
	public IEnumerator Initialize() {
		yield return InitializeAfterPreparation();
		IsInitialized = true;
		onInitializationFinished?.Invoke();
	}

	private void Awake() {
		PrepareForInitialization_ReplacingAwake();
	}

	private void Start() {
		SceneLoader.Instance?.RegisterForLongInitialization(this); // let SceneLoader know there is an object with long initialization in the scene
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
	/// <returns></returns>
	protected abstract IEnumerator InitializeAfterPreparation();


	/// <summary>
	/// This method replaces <c>Update()</c> method in derived classes (in fact, it is called from <c>Update()</c> method of this class).
	/// It ensures any code in update loop runs only after initialization has finished.
	/// </summary>
	protected abstract void UpdateAfterInitialization();

}
