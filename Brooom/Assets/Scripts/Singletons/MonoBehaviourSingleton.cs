using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class for a singleton of a component, with a static property for getting the instance, ensuring it will be initialized in time.
/// It also provides several different options for its behaviour (lazy initialization, removal of redundant instances in a scene,
/// creation of a new object with the component if there is none available, persistency between scenes), which can be even combined together.
/// Derived classes then represent a singleton specialized on their type and have to implement an <c>ISingleton</c> interface.
/// </summary>
/// <typeparam name="T">Type of the component for which the singleton is created.</typeparam>
public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour, ISingleton {

    /// <summary>Describes the chosen behaviour of the singleton. It could be overwritten in class constructor and may be set to a combination of several behaviours.</summary>
    public static SingletonOptions Options { get; protected set; } = SingletonOptions.PersistentBetweenScenes; // static field is separate for each type specialization

    private static bool isInitialized = false;

    private static T _Instance;
    /// <summary>Singleton instance. Getter makes sure it is initialized in time.</summary>
    public static T Instance {
        get {
            // Lazy initialization
            if (_Instance == null) TrySetSingletonInstance();
            if (!isInitialized) InitializeSingleton();
            return _Instance;
        }
    }

    // Tries to find the singleton instance in a scene, calls its AwakeSingleton() method and makes it persisten between scenes (if requested by singleton options)
    private static void TrySetSingletonInstance() {
        if (_Instance != null) return;
        // Try to find it in the scene (even hidden)
        T objectFound = UtilsMonoBehaviour.FindObject<T>();
        if (objectFound != null) {
            _Instance = objectFound;
        }
        // Call AwakeSingleton()
        if (_Instance != null) {
            _Instance.AwakeSingleton();
            // If necessary, make it persistent between scenes
            if (Options.HasFlag(SingletonOptions.PersistentBetweenScenes))
                GameObject.DontDestroyOnLoad(_Instance);
        }
    }

    // If there is no singleton instance yet, tries to find it in a scene, if that fails and singleton options allow it, a new GameObject is created
    // Then the singleton instance is initialized
    private static void InitializeSingleton() {
        if (_Instance == null) TrySetSingletonInstance();

        if (_Instance == null) {
            // Create a new GameObject representing the singleton if there is none already
            if (Options.HasFlag(SingletonOptions.CreateNewGameObject)) {
                GameObject go = new GameObject();
                go.name = typeof(T).Name;
                _Instance = go.AddComponent<T>();
                // If necessary, make it persistent between scenes
                if (Options.HasFlag(SingletonOptions.PersistentBetweenScenes))
                    GameObject.DontDestroyOnLoad(_Instance);
                _Instance.AwakeSingleton();
            } else {
                Debug.LogError($"No instance of the {typeof(T).Name} singleton was found in the scene.");
                return;
            }
        }

        _Instance.InitializeSingleton();
        isInitialized = true;
    }

    /// <summary>
    /// Resets the singleton instance (setting it to <c>null</c>). May be used e.g. from <c>OnDestroy()</c>.
    /// </summary>
    protected void ResetInstance() {
        _Instance = null;
        isInitialized = false;
    }

    private void Awake() {
        // Remove redundant instances from the scene
        if (_Instance != null && _Instance != this && Options.HasFlag(SingletonOptions.RemoveRedundantInstances))
            Destroy(gameObject);
        else {
            if (_Instance == null) {
                TrySetSingletonInstance();
            }
            // Eager initialization
            if (!Options.HasFlag(SingletonOptions.LazyInitialization)) {
                InitializeSingleton();
            }
        }
	}
}

/// <summary>
/// Available singleton behaviours, may be even combined together.
/// </summary>
[Flags]
public enum SingletonOptions { 
    LazyInitialization = 1, // Initialize the singleton instance when it is needed for the first time
    RemoveRedundantInstances = 2, // Remove redundant instances from the scene
    CreateNewGameObject = 4, // Create a new GameObject representing the singleton if there is none in the scene
    PersistentBetweenScenes = 8 // Use DontDestroyOnLoad on the singleton instance
}

/// <summary>
/// An interface containing useful methods for the types derived from <c>MonoBehaviourSingleton&lt;T&gt;</c>.
/// This way the programmer is required to implement these methods (even if they are empty) and it is less likely
/// they would override <c>Awake()</c> method by accident.
/// It also reminds the programmer that any initialization needs to be done in a special method 
/// (to function correctly even in the case of lazy initialization).
/// </summary>
public interface ISingleton {

    /// <summary>
    /// A replacement for <c>MonoBehaviour</c>'s <c>Awake()</c> method, which is needed in the <c>MonoBehaviourSingleton&lt;T&gt;</c> base class
    /// and therefore cannot be defined also in the derived class.
    /// It is used for anything which needs to be done in the usual <c>Awake()</c> and it is in fact called from <c>Awake()</c> method
    /// of the <c>MonoBehaviourSingleton&lt;T&gt;</c>.
    /// </summary>
    public void AwakeSingleton();

    /// <summary>
    /// Called when a new instance of the singleton is created (and, in case of lazy initialization, used for the first time).
    /// It can be used to initialize some data later then in the <c>Awake()</c> method (e.g. for lazy initialization).
    /// </summary>
    public void InitializeSingleton();

}