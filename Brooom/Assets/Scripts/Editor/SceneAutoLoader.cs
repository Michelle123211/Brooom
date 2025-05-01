using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


// Heavily inspired by solutions from https://discussions.unity.com/t/executing-first-scene-in-build-settings-when-pressing-play-button-in-editor/489673/8
// and also from https://gist.github.com/pdcp1/6e7f7315fc000de55676

// Adds menu items (also connected with key shortcuts):
//	- to run the game from the initial scene - can be used while a different scene is opened in the editor
//	- to reload the last scene - can be used after finishing the game run by the previous command to get back to the originally opened scene
// Uses EditorPrefs for storing state
[InitializeOnLoad]
public class SceneAutoLoader : EditorWindow {

	private const string StartScenePath = "Assets/Scenes/Start.unity";

	private const string EditorPrefsStartScene = "StartScene";
	private const string EditorPrefsLastScene = "LastScene";

	private static string StartScene {
		get => EditorPrefs.GetString(EditorPrefsStartScene, StartScenePath);
		set => EditorPrefs.SetString(EditorPrefsStartScene, value);
	}
	private static string LastScene {
		get => EditorPrefs.GetString(EditorPrefsLastScene);
		set => EditorPrefs.SetString(EditorPrefsLastScene, value);
	}

	[MenuItem("Play/Play Start scene _%h")]
	public static void PlayMainScene() {
		LastScene = EditorSceneManager.GetActiveScene().path;
		EditorSceneManager.OpenScene(StartScene);
		EditorApplication.isPlaying = true;
	}

	[MenuItem("Play/Reload last scene _%g")]
	public static void ReloadLastScene() {
		EditorSceneManager.OpenScene(LastScene);
	}

}
