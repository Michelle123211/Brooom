using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Cutscenes : MonoBehaviour {
    // Simple singleton
    public static Cutscenes Instance;

    private Dictionary<string, PlayableDirector> cutscenes = new();

    public PlayableDirector PlayCutscene(string cutsceneName) {
        if (cutscenes.TryGetValue(cutsceneName, out PlayableDirector cutscene)) {
            cutscene.Play();
            return cutscene;
        } else {
            return null;
        }
    }

    private void Awake() {
        Instance = this;
    }

	private void Start() {
        // Initialize list of cutscenes
        PlayableDirector[] cutscenesFound = GetComponentsInChildren<PlayableDirector>();
        foreach (var cutscene in cutscenesFound) {
            cutscenes[cutscene.name] = cutscene;
        }
	}

	private void OnDestroy() {
        Instance = null;
    }
}
