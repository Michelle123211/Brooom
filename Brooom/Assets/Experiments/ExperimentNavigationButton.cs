using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentNavigationButton : MonoBehaviour
{
    public void BackToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void ChangeScene(string scene) {
        SceneLoader.Instance.LoadScene(scene);
    }
}
