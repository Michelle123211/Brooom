using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, $"Game started.");
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }
}
