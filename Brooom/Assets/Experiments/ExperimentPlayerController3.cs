using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentPlayerController3 : MonoBehaviour {
    public float maxSpeed = 40;
    public float initialValue = 0.05f;
    public float zeroThreshold = 0.001f;
    public float accelerationFactor = 1.05f;
    public float brakeDecelerationFactor = 0.8f;
    public float decelerationFactor = 0.999f;
    public float maxSpeedDelta = 0.001f;

    float forward;
    float currentForward = 0;
    float currentForwardDelta = 0;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        forward = 0;
        if (Input.GetKey(KeyCode.W)) forward = 1; // forward
        else if (Input.GetKey(KeyCode.S)) forward = -1; // brake
    }

    private void FixedUpdate() {
        if (forward == 1) {
            // increase the forward value
            if (currentForwardDelta == 0)
                currentForwardDelta = zeroThreshold;
            else
                currentForwardDelta = (currentForward * accelerationFactor) - currentForward;
        } else if (forward == -1) {
            // decrease the forward value
            currentForwardDelta = (currentForward * brakeDecelerationFactor) - currentForward;
        } else {
            // descrease the forward value slowly
            currentForwardDelta = (currentForward * decelerationFactor) - currentForward;
        }
        if (currentForwardDelta > maxSpeedDelta)
            currentForwardDelta = maxSpeedDelta;
        else if (currentForwardDelta < -maxSpeedDelta)
            currentForwardDelta = -maxSpeedDelta;
        Debug.Log(currentForwardDelta);
        currentForward += currentForwardDelta;
        if (currentForward < zeroThreshold)
            currentForward = 0;
        if (currentForward > 1)
            currentForward = 1;

        rb.velocity = transform.forward * maxSpeed * currentForward;// * Time.fixedDeltaTime;
    }

    public float[] GetValuesForDisplay() {
        float[] result = new float[] {
            rb.velocity.magnitude, // current speed
            maxSpeed, //max speed
            transform.position.y, // current altitude
            0 // max altitude
        };
        return result;
    }
}
