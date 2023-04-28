using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentPlayerController : MonoBehaviour {
    // Parameters
    [Header("Forward movement")]
    public float maxSpeed = 40;
    public float forwardResponsiveness = 0.01f;
    [Header("Yaw and Roll")]
    public float turnSpeed = 3;
    public float yawResponsiveness = 0.05f;
    public float maxRollAngle = 25;
    [Header("Pitch")]
    public float maxPitchAngle = 35;
    public float pitchResponsiveness = 0.03f;
    public float maxAltitude = 50;

    // Components
    Rigidbody rb;

    // Storing the input (and therefore direction of the movement, -1 or 1)
    float forward, yaw, pitch;

    // Current values (for gradual change, between -1 and 1)
    float currentForward = 0, currentYaw = 0, currentPitch = 0;



    [Header("Experimental")]
    [Tooltip("How long it takes in seconds to go from 0 to max speed.")]
    public float accelerationDuration = 10;
    [Tooltip("A curve describing speed change in time.")]
    public AnimationCurve acceleration = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("How long it takes in seconds to go from 0 to max speed.")]
    public float decelerationDuration = 4f;
    [Tooltip("A curve describing speed change in time.")]
    public AnimationCurve deceleration = AnimationCurve.EaseInOut(0, 1, 1, 0);
    ForwardState forwardState = ForwardState.IDLE;
    enum ForwardState {
        POSITIVE, // positive key pressed
        NEGATIVE, // negative key pressed
        RELEASED_POSITIVE, // no key pressed and broom moving in the positive direction
        RELEASED_NEGATIVE, // no key pressed and broom moving in the negative direction
        IDLE // no key pressed and broom not moving
    }
    AnimationCurve forwardCurve;
    float forwardT = 0;
    float timeout = 5;
    float referenceValue = 1;


    public void SetMaxSpeed(float maxSpeed) {
        this.maxSpeed = maxSpeed;
    }

    public void SetMaxAltitude(float maxAltitude) {
        this.maxAltitude = maxAltitude;
    }

    public void SetYawResponsiveness(float responsiveness) {
        this.yawResponsiveness = responsiveness;
    }

    public void SetPitchResponsiveness(float responsiveness) {
        this.pitchResponsiveness = responsiveness;
    }


    public float[] GetValuesForDisplay() {
        float[] result = new float[] {
            rb.velocity.magnitude, // current speed
            maxSpeed, //max speed
            transform.position.y, // current altitude
            maxAltitude // max altitude
        };
        return result;
    }


    // Update is called once per frame
    void Update() {
        // Handling input and storing it (for use in FixedUpdate)

        //forward = 0; 
        yaw = 0; pitch = 0;

        // Forward
        float forwardKey = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0);
        switch (forwardState) {
            case ForwardState.POSITIVE:
                if (forwardKey != 1) { // not staying positive
                    forwardState = ForwardState.RELEASED_POSITIVE;
                    forwardCurve = deceleration;
                    forwardT = 0;
                    referenceValue = rb.velocity.magnitude;
                    timeout = referenceValue / maxSpeed * decelerationDuration;
                }
                break;
            case ForwardState.NEGATIVE: // TODO
                if (forwardKey != -1) { // not staying negative
                    forwardState = ForwardState.RELEASED_NEGATIVE;
                    forwardCurve = deceleration;
                    forwardT = 0;
                    referenceValue = -rb.velocity.magnitude;
                    timeout = Mathf.Abs(decelerationDuration) / maxSpeed * decelerationDuration;
                }
                break;
            case ForwardState.RELEASED_POSITIVE:
                if (Mathf.Abs(rb.velocity.magnitude) < 0.1) { // broom basically stopped
                    forwardState = ForwardState.IDLE;
                }
                break;
            case ForwardState.RELEASED_NEGATIVE: // TODO
                if (Mathf.Abs(rb.velocity.magnitude) < 0.1) { // broom basically stopped
                    forwardState = ForwardState.IDLE;
                }
                break;
            case ForwardState.IDLE:
                if (forwardKey == 1) {
                    forwardState = ForwardState.POSITIVE;
                    forwardCurve = acceleration;
                    timeout = accelerationDuration;
                    forwardT = 0;
                    referenceValue = maxSpeed;
                }
                break;
        }
        Debug.Log(forwardState);
        //if (Input.GetKey(KeyCode.W)) forward = 1; // forward
        //else if (Input.GetKey(KeyCode.S)) forward = -1; // brake

        // Left/right
        if (Input.GetKey(KeyCode.A)) yaw = -1; // left
        else if (Input.GetKey(KeyCode.D)) yaw = 1; // right

        // Up/down
        if (Input.GetKey(KeyCode.Space)) pitch = -1; // up
        else if (Input.GetKey(KeyCode.LeftControl)) pitch = 1; // down
    }

    private void FixedUpdate() {
        // Forward
        forwardT += Time.fixedDeltaTime;
        if (forwardT > timeout) forwardT = timeout;
        float currentValue = forwardCurve.Evaluate(forwardT / timeout);
        if (forwardState == ForwardState.IDLE)
            currentValue = 0;
        float speed = currentValue * referenceValue; // initialValue + currentValue * (targetValue - initialValue);
        Debug.Log($"current value: {currentValue}, current speed: {speed}");
        //rb.velocity = transform.forward * maxSpeed * currentValue;
        rb.velocity = transform.forward * speed;
        //currentForward += (forward - currentForward) * forwardResponsiveness; // change slowly, not immediately
        //if (currentForward < 0) currentForward = 0; // not allowing reverse, only brake
        //rb.velocity = transform.forward * maxSpeed * currentForward;

        // Limiting the altitude
        if (transform.position.y >= maxAltitude && pitch < 1) {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (pitch == -1) pitch = 0;
        }
        if (transform.position.y <= 4 && pitch == 1) // TODO: Change this later to distance from the terrain underneath (instead of y = 4)
            pitch = 0;

        // Yaw
        currentYaw += (yaw - currentYaw) * yawResponsiveness; // change slowly, not immediately
        transform.Rotate(Vector3.up, currentYaw * turnSpeed);
        // Roll
        Vector3 eulerAngles = transform.localEulerAngles;
        eulerAngles.z = -currentYaw * maxRollAngle;
        // Pitch
        currentPitch += (pitch - currentPitch) * pitchResponsiveness; // change slowly, not immediately
        eulerAngles.x = currentPitch * maxPitchAngle;

        transform.localEulerAngles = eulerAngles;
    }

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();

        forwardState = ForwardState.IDLE;
        forwardCurve = acceleration;
        forwardT = 0;
        timeout = accelerationDuration;
        referenceValue = 1;
    }
}



// Represents one twenable property of the flying mechanics (e.g. forward movement, yaw, roll, pitch).
// Gets negative, positive and neutral values, then interpolates between them based on the direction and tweening curve.
[System.Serializable]
public class TweenedFlyPropertyWithNeutral {
    [Header("Values")]
    public float negativeValue;
    public float positiveValue;
    public float neutralValue;

    [Header("Curves")]
    [Tooltip("How long it takes in seconds to go from the neutral to the extreme.")]
    public float accelerationDuration;
    [Tooltip("A curve describing acceleration to either extreme when the corresponding key is pressed.")]
    public AnimationCurve accelerationCurve;
    [Tooltip("How long it takes in seconds to go from the extreme to the neutral.")]
    public float decelerationDuration;
    [Tooltip("A curve describing decelaration to neutral when no key is pressed.")]
    public AnimationCurve decelerationCurve;
    [Tooltip("How long it takes in seconds to go to the neutral before acceleration to the other side.")]
    public float transitionDuration;
    [Tooltip("A curve describing quick deceleration to neutral before acceleration to the other side.")]
    public AnimationCurve transitionCurve;

    enum State {
        ACCEL_POSITIVE,
        ACCEL_NEGATIVE,
        DECEL_NEUTRAL,
        TRANSITION, // quick transition to neutral before going to the other side
        IDLE
    }

    private State currentState = State.IDLE;
    private float currentTime = 0;
    private float currentValue = 0;
    private float currentDuration = 0;
    private float currentDirection = 0; // -1 to negative, 1 to positive, 0 to neutral

    public float GetCurrentValueInTimeStep(float timeStep, float direction) {
        State newState = GetNewState(direction);
        TransitionToState(newState);
        // Make time step
        currentTime += timeStep;
        if (currentTime > currentDuration)
            currentTime = currentDuration;
        currentDirection = direction;
        // Compute and return the value
        ComputeCurrentValue();
        return currentValue;
    }

    private State GetNewState(float dir) {
        switch (currentState) {
            case State.ACCEL_POSITIVE:
                if (dir == 1) {

                } else if (dir == -1) {

                } else { // 

                }
                break;
            case State.ACCEL_NEGATIVE:
                break;
            case State.DECEL_NEUTRAL:
                break;
            case State.TRANSITION:
                break;
        }




        // pohyb dopredu:
        //currentState == State.ACCEL_POSITIVE || (currentState == State.DECEL_NEUTRAL && currentValue )




        if (dir == 1 && IsInInterval(currentValue, neutralValue, positiveValue)) { // going to the positive while not being on the negative side
            return State.ACCEL_POSITIVE;
        }
        if (dir == -1 && IsInInterval(currentValue, neutralValue, negativeValue)) { // going to the negative while not being on the positive side
            return State.ACCEL_NEGATIVE;
        }
        if (dir == 0 && currentValue != neutralValue) { // not going anywhere while not being in the neutral => return slowly to the neutral
            return State.DECEL_NEUTRAL;
        }
        if (dir != 0 && currentValue != neutralValue) { // going in the opposite direction then the desired one
            return State.TRANSITION;
        }
        return State.IDLE;

    }

    private void TransitionToState(State newState) {

        switch (currentState) {
            case State.ACCEL_POSITIVE:
                break;
            case State.ACCEL_NEGATIVE:
                break;
            case State.DECEL_NEUTRAL:
                break;
            case State.TRANSITION:
                break;
        }
    }

    private void ComputeCurrentValue() {
    }

    // Returns true if the given value is between the two bounds (not dependent on the order of the bounds and/or their signs)
    private bool IsInInterval(float value, float bound1, float bound2) {
        float range = Mathf.Abs(bound1 - bound2);
        return Mathf.Abs(bound1 - value) <= range && Mathf.Abs(bound2 - value) <= range;
    }
}
