using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    // Parameters
    [Header("Forward movement")]
    public float maxSpeed = 40;
    public float forwardResponsiveness = 0.01f;
    [Header("Yaw and Roll")]
    public float turnSpeed = 3;
    public float yawResponsiveness = 0.03f;
    public float maxRollAngle = 30;
    [Header("Pitch")]
    public float maxPitchAngle = 35;
    public float pitchResponsiveness = 0.03f;
    public float maxAltitude = 50;

    // Components
    Rigidbody rb;

    // Other objects
    CameraController cameraController;

    // Storing the input (and therefore direction of the movement, -1 or 1)
    float forwardInput, yawInput, pitchInput;

    // Current values (for gradual change)
    float currentForward = 0; // current speed
    float currentYaw = 0, currentPitch = 0; // between -1 and 1

    // Bonus speed (added on top of the current speed)
    bool hasBonusSpeed = false;
    float bonusSpeed = 0;
    float bonusSpeedDuration = 0;
    DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> bonusSpeedTween;

    // The following 4 methods are used from the broom upgrades
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

    // Used from the speed-up bonus
    public void SetBonusSpeed(float value, float duration) {
        bonusSpeed = value;
        bonusSpeedDuration += duration;
        hasBonusSpeed = true;
        TweenBonusSpeed(value, 1f);
        cameraController?.ZoomIn(true); // zoom the camera in to make the effect stronger
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

    // Changes the bonus speed value gradually over time
    private void TweenBonusSpeed(float targetValue, float duration) {
        if (bonusSpeedTween != null && bonusSpeedTween.IsActive() && bonusSpeedTween.IsPlaying())
            bonusSpeedTween.Kill();
        bonusSpeedTween = DOTween.To(() => bonusSpeed, x => bonusSpeed = x, targetValue, duration).SetEase(Ease.InOutCubic);
    }

    // Resets the bonus speed when it should no longer be active
    private void UpdateBonusSpeed() {
        bonusSpeedDuration -= Time.fixedDeltaTime;
        if (bonusSpeedDuration <= 0) { // bonus is no longer active
            bonusSpeedDuration = 0;
            bonusSpeed = 0;
            hasBonusSpeed = false;
            TweenBonusSpeed(0f, 1f);
            cameraController?.ZoomIn(false);
        }
    }

    private void FixedUpdate() {
        // Resolve bonus speed (if any)
        if (hasBonusSpeed)
            UpdateBonusSpeed();

        // Forward
        forwardInput = -InputManager.Instance.GetFloatValue("Forward"); // -1 = forward, 1 = brake, then invert
        currentForward += ((forwardInput * maxSpeed) - currentForward) * forwardResponsiveness; // change slowly, not immediately
        if (currentForward < 0) currentForward = 0; // not allowing reverse, only brake
        rb.velocity = transform.forward * (currentForward + bonusSpeed); // add also the bonus speed, if any

        // Limiting the altitude
        pitchInput = InputManager.Instance.GetFloatValue("Pitch"); // -1 = up, 1 = down
        if (transform.position.y >= maxAltitude && pitchInput < 1) {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (pitchInput == -1) pitchInput = 0;
        }
        if (transform.position.y <= 0.5 && pitchInput == 1) // TODO: Change this later to distance from the terrain underneath (instead of y = 4)
            pitchInput = 0;

        // Yaw
        yawInput = InputManager.Instance.GetFloatValue("Turn"); // -1 = left, 1 = right
        currentYaw += (yawInput - currentYaw) * yawResponsiveness; // change slowly, not immediately
        transform.Rotate(Vector3.up, currentYaw * turnSpeed);
        // Roll
        Vector3 eulerAngles = transform.localEulerAngles;
        eulerAngles.z = -currentYaw * maxRollAngle;
        // Pitch
        currentPitch += (pitchInput - currentPitch) * pitchResponsiveness; // change slowly, not immediately
        eulerAngles.x = currentPitch * maxPitchAngle;

        transform.localEulerAngles = eulerAngles;
    }

	// Start is called before the first frame update
	void Start() {
        rb = GetComponent<Rigidbody>();
        cameraController = FindObjectOfType<CameraController>();
    }
}
