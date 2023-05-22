using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour, ISingleton {

    // Describes the chosen behaviour of the singleton using SingletonOptions enum values
    public static int Options { get; protected set; }


    private static T _Instance;
    public static T Instance {
        get {
            // Lazy initialization
            if ((Options & (int)SingletonOptions.LazyInitialization) != 0) {
                InitializeSingletonInstance();
            }
            return _Instance;
        }
    }

    private static void InitializeSingletonInstance() {
        if (_Instance != null) return;
        _Instance = FindObjectOfType<T>();
        if (_Instance == null) {
            // Create a new GameObject representing the singleton if there is none in the scene
            if ((Options & (int)SingletonOptions.CreateNewGameObject) != 0) {
                GameObject go = new GameObject();
                go.name = typeof(T).Name;
                _Instance = go.AddComponent<T>();
            } else {
                Debug.LogError($"No instance of the {typeof(T).Name} singleton was found in the scene.");
                return;
            }
        }
        _Instance.InitializeSingleton();
        // Persistency between scenes
        if ((Options & (int)SingletonOptions.PersistentBetweenScenes) != 0) {
            GameObject.DontDestroyOnLoad(_Instance);
        }
    }

    // A method used to set different singleton options flags (SingletonOptions enum) other than the default ones
    //    It is called in the Awake() and may be overriden by derived types to set their specific options flags
    protected virtual void SetSingletonOptions() {
        Options = (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances;
    }

    private void Awake() {
        SetSingletonOptions();
        // Remove redundant instances from the scene
        if (_Instance != null && _Instance != this && (Options & (int)SingletonOptions.RemoveRedundantInstances) != 0)
            Destroy(gameObject);
        else {
            // Eager initialization
            if ((Options & (int)SingletonOptions.LazyInitialization) == 0)
                InitializeSingletonInstance();
            _Instance.AwakeSingleton();
        }
	}
}

public enum SingletonOptions { 
    LazyInitialization, // Initialize the singleton instance when it is needed for the first time
    RemoveRedundantInstances, // Remove redundant instances from the scene
    CreateNewGameObject, // Create a new GameObject representing the singleton if there is none in the scene
    PersistentBetweenScenes // Use DontDestroyOnLoad
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