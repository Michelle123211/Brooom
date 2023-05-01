using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentInputDebug : MonoBehaviour
{
    public bool debugForward;
    public bool debugTurn;
    public bool debugPitch;
    public bool debugCastSpell;
    public bool debugSwitchSpell;
    public bool debugRestart;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (debugForward) {
            Debug.Log("Forward: " + InputManager.Instance.GetFloatValue("Forward"));
        }
        if (debugTurn) {
            Debug.Log("Turn: " + InputManager.Instance.GetFloatValue("Turn"));
        }
        if (debugPitch) {
            Debug.Log("Pitch: " + InputManager.Instance.GetFloatValue("Pitch"));
        }
        if (debugCastSpell) {
            if (InputManager.Instance.GetBoolValue("CastSpell"))
                Debug.Log("CastSpell: just pressed");
        }
        if (debugSwitchSpell) {
            float value = InputManager.Instance.GetFloatValue("SwitchSpell");
            if (value != 0)
                Debug.Log("SwitchSpell: " + value);
        }
        if (debugRestart) {
            if (InputManager.Instance.GetBoolValue("Restart"))
                Debug.Log("Restart: just pressed");
        }
    }
}
