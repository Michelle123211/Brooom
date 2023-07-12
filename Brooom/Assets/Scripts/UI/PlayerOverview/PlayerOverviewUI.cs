using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewUI : MonoBehaviour
{

    public void ReturnToMenu() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartNextRace() {
        SceneLoader.Instance.LoadScene(Scene.Race);
    }
}
