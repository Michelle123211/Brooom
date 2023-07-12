using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public void StartTestingTrack() {
        SceneLoader.Instance.LoadScene(Scene.TestingTrack);
    }
}
