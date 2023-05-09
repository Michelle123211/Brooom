using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }
}
