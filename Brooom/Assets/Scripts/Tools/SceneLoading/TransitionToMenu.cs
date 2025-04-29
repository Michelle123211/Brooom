using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component which simply loads the Main Menu scene from its <c>Start()</c> method.
/// It can be used in the initial scene which is empty and is there only to make sure everything is setup before the game actually starts.
/// </summary>
public class TransitionToMenu : MonoBehaviour {

    void Start() {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, $"Game started.");
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

}
