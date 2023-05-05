using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentBackButton : MonoBehaviour
{
    public void BackToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu, true, true);
    }
}
