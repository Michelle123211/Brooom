using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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
            currentForward * maxSpeed, // current speed
            maxSpeed, //max speed
            transform.position.y, // current altitude
            maxAltitude // max altitude
        };
        return result;
    }


    // Update is called once per frame
    void Update() {
        // Handling input and storing it (for use in FixedUpdate)

        forward = 0;  yaw = 0; pitch = 0;

        // Forward
        if (Input.GetKey(KeyCode.W)) forward = 1; // forward
        else if (Input.GetKey(KeyCode.S)) forward = -1; // brake

        // Left/right
        if (Input.GetKey(KeyCode.A)) yaw = -1; // left
        else if (Input.GetKey(KeyCode.D)) yaw = 1; // right

        // Up/down
        if (Input.GetKey(KeyCode.Space)) pitch = -1; // up
        else if (Input.GetKey(KeyCode.LeftControl)) pitch = 1; // down
    }

	private void FixedUpdate() {
        // Forward
        currentForward += (forward - currentForward) * forwardResponsiveness; // change slowly, not immediately
        if (currentForward < 0) currentForward = 0; // not allowing reverse, only brake
        rb.velocity = transform.forward * maxSpeed * currentForward;

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
    }
}
