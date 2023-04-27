using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExperimentDisplay : MonoBehaviour
{
    public PlayerController player;

    public TextMeshProUGUI currentSpeedText;
    public TextMeshProUGUI maxSpeedText;
    public TextMeshProUGUI currentAltitudeText;
    public TextMeshProUGUI maxAltitudeText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float[] displayInfo = player.GetValuesForDisplay();
        currentSpeedText.text = displayInfo[0].ToString();
        maxSpeedText.text = displayInfo[1].ToString();
        currentAltitudeText.text = displayInfo[2].ToString();
        maxAltitudeText.text = displayInfo[3].ToString();

    }
}
