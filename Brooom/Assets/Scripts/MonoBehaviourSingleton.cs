using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour, IInitializableSingleton
{
    private static T _Instance;
    public static T Instance {
        get {
            if (_Instance != null) return _Instance;
            _Instance = FindObjectOfType<T>();
            if (_Instance == null) {
                GameObject go = new GameObject();
                go.name = typeof(T).Name;
                _Instance = go.AddComponent<T>();
			}
            _Instance.InitializeSingleton();
            GameObject.DontDestroyOnLoad(_Instance);
            return _Instance;
        }
    }
}

public interface IInitializableSingleton {
    public void InitializeSingleton();
}