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
		SceneLoader.Instance.RegisterForLongInitialization(this); // let SceneLoader know there is an object with long initialization in the scene
		PrepareForInitialization_ReplacingStart();
	}

	private void Update() {
		// Don't do anything, unless the object initialization finished completely
		if (!IsInitialized) return;
		UpdateAfterInitialization();
	}

	// All the following methods are abstract, even though they can be empty, to emphasize that these should
	// be used instead of standard Awake(), Start() and Update() inherited from MonoBehaviour

	// This method should contain only a fast initialization
	// It replaces Awake() method in derived classes (in fact, it is called from Awake() method of this class)
	protected abstract void PrepareForInitialization_ReplacingAwake();
	// This method should contain only a fast initialization
	// It replaces Start() method in derived classes (in fact, it is called from Start() method of this class)
	protected abstract void PrepareForInitialization_ReplacingStart();

	// This method is used for long initialization
	// It should return control back as often as possible using "yield return"
	protected abstract IEnumerator InitializeAfterPreparation();

	// This method replaces Update() method in derived classes (in fact, it is called from Update() method of this class)
	// It ensures any code in update loop runs only after inicialization has finished
	protected abstract void UpdateAfterInitialization();

}
