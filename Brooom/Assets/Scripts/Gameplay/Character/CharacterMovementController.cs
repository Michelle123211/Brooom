using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(Rigidbody))]
public class CharacterMovementController : MonoBehaviour {

    // Parameters
    [Header("Forward movement")]
    public float maxSpeed = 10;
    public float forwardResponsiveness = 0.01f;
    [Header("Yaw and Roll")]
    public float turnSpeed = 2;
    public float yawResponsiveness = 0.01f;
    public float maxRollAngle = 30;
    [Header("Pitch")]
    public float maxPitchAngle = 35;
    public float pitchResponsiveness = 0.01f;


    private bool actionsEnabled = false;
    public bool ActionsEnabled {
        get => actionsEnabled;
        set {
            if (actionsEnabled && !value) ResetMovement();
            actionsEnabled = value;
        }
    }

    // Distinguish between controlling movement of the player or the opponents
    [HideInInspector] public bool isPlayer = true;

    // Components
    CharacterInput characterInput;
    Rigidbody rb;
    PlayerCameraController cameraController; // available only for the player (not opponents)

    // Current values (for gradual change)
    float currentForwardSpeed = 0; // current speed
    float currentYaw = 0, currentPitch = 0; // between -1 and 1
    float previousForwardInput; // direction of the movement, -1, 0 or 1

    // Bonus speed (added on top of the current speed)
    bool hasBonusSpeed = false;
    float maxBonusSpeed = 0;
    float bonusSpeed = 0;
    float bonusSpeedDuration = 0;
    DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> bonusSpeedTween;



    // The following 5 methods are used from the broom upgrades
    public void SetMaxSpeed(float maxSpeed) {
        this.maxSpeed = maxSpeed;
    }
    public void SetMaxAltitude(float maxAltitude) {
        if (isPlayer)
            PlayerState.Instance.maxAltitude = maxAltitude;
    }
    public void SetForwardResponsiveness(float responsiveness) {
        this.forwardResponsiveness = responsiveness;
    }
    public void SetYawResponsiveness(float responsiveness) {
        this.yawResponsiveness = responsiveness;
    }
    public void SetPitchResponsiveness(float responsiveness) {
        this.pitchResponsiveness = responsiveness;
    }

    // Used from the speed-up bonus
    public void SetBonusSpeed(float value, float duration) {
        maxBonusSpeed = Mathf.Max(maxBonusSpeed, value);
        bonusSpeedDuration = Mathf.Max(bonusSpeedDuration, duration); // don't add it but override it so that the bonus does not accumulate for too long
        hasBonusSpeed = true;
        TweenBonusSpeed(maxBonusSpeed, 1f);
        if (isPlayer)
            cameraController?.ZoomIn(true); // zoom the camera in to make the effect stronger
    }

    public void ResetPosition(Vector3 position) {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        ResetMovement();
    }

    private void ResetMovement() {
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        currentForwardSpeed = 0;
        currentYaw = 0;
        currentPitch = 0;

        previousForwardInput = 0;
    }

    public float[] GetValuesForDisplay() {
        float[] result = new float[] {
            rb.velocity.magnitude, // current speed
            maxSpeed, //max speed
            transform.position.y, // current altitude
            PlayerState.Instance.maxAltitude // max altitude
        };
        return result;
    }

    public float GetCurrentSpeed() {
        return rb.velocity.magnitude;
    }

    public float GetCurrentAltitude() {
        return transform.position.y;
    }

    // Changes the bonus speed value gradually over time
    private void TweenBonusSpeed(float targetValue, float duration) {
        if (bonusSpeedTween != null && bonusSpeedTween.IsActive() && bonusSpeedTween.IsPlaying())
            bonusSpeedTween.Kill();
        bonusSpeedTween = DOTween.To(() => bonusSpeed, x => bonusSpeed = x, targetValue, duration).SetEase(Ease.InOutCubic);
    }

    // Resets the bonus speed when it should no longer be active
    private void UpdateBonusSpeed(float forwardInput) {
        bonusSpeedDuration -= Time.fixedDeltaTime;
        if (bonusSpeedDuration <= 0) { // bonus is no longer active
            maxBonusSpeed = 0;
            bonusSpeedDuration = 0;
            hasBonusSpeed = false;
            TweenBonusSpeed(maxBonusSpeed, 1f);
            if (isPlayer)
                cameraController?.ZoomIn(false);
        } else if (previousForwardInput != forwardInput) { // change of direction, tween the bonus speed accordingly, TODO: Will not work with controllers, sign must be compared then
            TweenBonusSpeed(Mathf.Clamp(forwardInput, 0, 1) * maxBonusSpeed, 1f); // tween to 0 if braking or stopping, tween to maxBonusSpeed if going forward
        }
    }

    private void Awake() {
        characterInput = GetComponent<CharacterInput>();
        if (characterInput == null)
            Debug.LogError("An object with CharacterMovementController component must also have any component derived from CharacterInput (e.g. PlayerCharacterInput, OpponentCharacterInput).");
        rb = GetComponent<Rigidbody>();
        isPlayer = gameObject.CompareTag("Player");
        if (isPlayer)
            cameraController = GetComponent<PlayerCameraController>();
	}

	private void FixedUpdate() {
        // Do nothing if movement actions are not enabled
        if (!actionsEnabled) return;

        CharacterMovementValue movementInput = characterInput.GetMovementInput();

        // Forward input
        float forwardInput = (float)movementInput.forward;
        previousForwardInput = forwardInput;

        // Resolve bonus speed (if any)
        if (hasBonusSpeed)
            UpdateBonusSpeed(forwardInput);

        // Forward
        currentForwardSpeed += ((forwardInput * maxSpeed) - currentForwardSpeed) * forwardResponsiveness; // change slowly, not immediately
        if (currentForwardSpeed < 0) currentForwardSpeed = 0; // not allowing reverse, only brake
        rb.velocity = transform.forward * (currentForwardSpeed + bonusSpeed); // add also the bonus speed, if any

        // Limiting the altitude
        float pitchInput = (float)movementInput.pitch;
        if (transform.position.y >= PlayerState.Instance.maxAltitude && pitchInput < 1) {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (pitchInput == -1) pitchInput = 0;
        }
        if (transform.position.y <= 0.5 && pitchInput == 1) // TODO: Change this later to distance from the terrain underneath (instead of fixed y)
            pitchInput = 0;

        // Yaw
        float yawInput = (float)movementInput.yaw;
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
}
