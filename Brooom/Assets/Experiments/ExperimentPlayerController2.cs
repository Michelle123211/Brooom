using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentPlayerController2 : MonoBehaviour
{
    [Header("Pitch")]
    public float maxPitchAngle = 35;
    public float pitchPower = 1;
    public PIDController pitchController = new PIDController();


    // Storing the input (and therefore direction of the movement, -1 or 1)
    float pitch;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Handling input and storing it (for use in FixedUpdate)
        pitch = 0;
        // Up/down
        if (Input.GetKey(KeyCode.Space)) pitch = -1; // up
        else if (Input.GetKey(KeyCode.LeftShift)) pitch = 1; // down
    }

	private void FixedUpdate() {
        Vector3 eulerAngles = transform.localEulerAngles;

        // Pitch
        float currentAngle = (eulerAngles.x + 360 + 180) % 360 - 180;
        float value = pitchController.Update(Time.fixedDeltaTime, currentAngle, pitch * maxPitchAngle);
        //Debug.Log(value);
        eulerAngles.x += value * pitchPower;

        transform.localEulerAngles = eulerAngles;
	}
}

[System.Serializable]
public class PIDController {
    [Range(0, 1)]
    public float proportionalGain = 1;
    [Range(0, 1)]
    public float integralGain = 1;
    [Range(0, 1)]
    public float derivativeGain = 1;
    [Tooltip("Which calculation to use for the derivative term (Value prevents derivative kick when changing the target value).")]
    public DerivativeMeasurement derivativeMeasurement = DerivativeMeasurement.ValueRateOfChange;

    // Different calculations of derivative
    public enum DerivativeMeasurement { 
        ValueRateOfChange, // prevents derivative kick when changing the target value
        ErrorRateOfChange
    }
    private float previousError;
    private float previousValue;
    private bool derivativeInitialized = false;

    public float Update(float dt, float currentValue, float targetValue) {
        float error = targetValue - currentValue;

        // Proportional term
        float P = proportionalGain * error;
        //Debug.Log($"Current value: {currentValue}");
        //Debug.Log($"Target value: {targetValue}");
        //Debug.Log($"Error: {error}");
        //Debug.Log($"Return value: {P * dt}");
        //Debug.Log("------------");

        // Derivative term - two possible calculations
        float rateOfChange = 0;
        if (derivativeInitialized) { // skip the first iteration to ensure values in the previousValue or previousError variables
            switch (derivativeMeasurement) {
                case (DerivativeMeasurement.ValueRateOfChange):
                    rateOfChange = -(currentValue - previousValue);
                    previousValue = currentValue;
                    break;
                case (DerivativeMeasurement.ErrorRateOfChange):
                    rateOfChange = (error - previousError) / dt;
                    previousError = error;
                    break;
            }
        } else {
            derivativeInitialized = true;
        }
        float D = rateOfChange * derivativeGain; // divided by the time step to compute derivative

        float result = P + D;
        Debug.Log(D);

        return result;
    }

    // Should be called if the system is moved by external means (e.g. teleported) or turned off for long period of time
    public void Reset() {
        derivativeInitialized = false;
    }
}