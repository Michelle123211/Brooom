using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// A simple singleton which can be used to play cutscenes (e.g., at the start/end of the race).
/// All available cutscenes should be added as children with <c>PlayableDirector</c> component.
/// Child's name is then used as a cutscene name.
/// </summary>
public class Cutscenes : MonoBehaviour {
    /// <summary>Singleton instance.</summary>
    public static Cutscenes Instance;
    // All available cutscenes (found as PlayableDirector components in children objects)
    private Dictionary<string, PlayableDirector> cutscenes = new();

    /// <summary>
    /// Plays cutscene of the given name, if it exists (it has to be added as a child object of the same name with a <c>PlayableDirector</c> component).
    /// </summary>
    /// <param name="cutsceneName">Name of the cutscene to be played (corresponds to name of the object to which <c>PlayableDirector</c> component containing this cutscene is attached).</param>
    /// <returns><c>PlayableDirector</c> component corresponding to the cutscene of the given name.</returns>
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
