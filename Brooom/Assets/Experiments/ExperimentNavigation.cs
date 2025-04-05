using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentNavigation : MonoBehaviour
{
    public List<ExperimentKeyAndScene> keysAndCorrespondingScenes;

    // Start is called before the first frame update
    void Start()
    {
        if (keysAndCorrespondingScenes == null) keysAndCorrespondingScenes = new List<ExperimentKeyAndScene>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var keyAndScene in keysAndCorrespondingScenes) {
            if (Input.GetKeyDown(keyAndScene.key)) {
                Utils.EnableCursor();
                SceneLoader.Instance.LoadScene(keyAndScene.scene);
            }
        }
    }
}

[System.Serializable]
public class ExperimentKeyAndScene {
    public KeyCode key = KeyCode.P;
    public string scene = "PlayerOverview";
}
