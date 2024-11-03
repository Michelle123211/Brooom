using System;
using System.Collections.Generic;
using UnityEngine;


public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour, ISingleton {

    // Describes the chosen behaviour of the singleton - could be overwritten in class constructor
    public static SingletonOptions Options { get; protected set; } = SingletonOptions.PersistentBetweenScenes; // static field is separate for each type specialization

    private static bool isInitialized = false;

    private static T _Instance;
    public static T Instance {
        get {
            // Lazy initialization
            if (_Instance == null) TrySetSingletonInstance();
            if (!isInitialized) InitializeSingleton();
            return _Instance;
        }
    }

    private static void TrySetSingletonInstance() {
        if (_Instance != null) return;
        // Try to find it in the scene (even hidden)
        List<T> objectsFound = Utils.FindObject<T>();
        if (objectsFound.Count > 0) {
            _Instance = objectsFound[0];
        }
        // Call AwakeSingleton()
        if (_Instance != null) {
            _Instance.AwakeSingleton();
            // If necessary, make it persistent between scenes
            if (Options.HasFlag(SingletonOptions.PersistentBetweenScenes))
                GameObject.DontDestroyOnLoad(_Instance);
        }
    }

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
            } else {
                Debug.LogError($"No instance of the {typeof(T).Name} singleton was found in the scene.");
                return;
            }
        }

        _Instance.InitializeSingleton();
        isInitialized = true;
    }

    // May be used e.g. from OnDestroy
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

[Flags]
// Describes the chosen behaviour of the singleton
public enum SingletonOptions { 
    LazyInitialization = 1, // Initialize the singleton instance when it is needed for the first time
    RemoveRedundantInstances = 2, // Remove redundant instances from the scene
    CreateNewGameObject = 4, // Create a new GameObject representing the singleton if there is none in the scene
    PersistentBetweenScenes = 8 // Use DontDestroyOnLoad
}

// An interface containing useful methods for the types derived from MonoBehaviourSingleton<T>
//    This way the programmer is required to implement these methods (even if they are empty)
//    Then it is less likely the programmer would override Awake() method by accident
//    It will also remind the programmer that any initialization needs to be done in a special method (to function correctly event in case of lazy initialization)
public interface ISingleton {
    // A replacement for the MonoBehaviour's Awake() method which is needed in the base class
    //    Used for anything which needs to be done in the usual Awake()
    //    Called from the Awake() method of the MonoBehaviourSingleton<T>
    public void AwakeSingleton();

    // Called when a new instance of the singleton is created
    //    Can be used to initialize some data later then in the Awake method (e.g. for lazy initialization)
    public void InitializeSingleton();
}