using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentRaceControls : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) {
            FindObjectOfType<AITestRaceController>().StartRace();
            Destroy(FindObjectOfType<StartingZone>().transform.parent.gameObject);
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            Time.timeScale = 0.2f;
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            Time.timeScale = 1f;
        }
    }
}
