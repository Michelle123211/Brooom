using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(Rigidbody))]
public class CharacterMovementController : MonoBehaviour {

    public static float MAX_SPEED = 16;

    // Parameters
    [Header("Forward movement")]
    [Tooltip("Number between 0 and 1 describing initial speed as a fraction of the maximum possible speed, which is currently 16.")]
    public float initialMaxSpeed = 0.75f;
    public float forwardResponsiveness = 0.01f;
    [Header("Yaw and Roll")]
    public float turnSpeed = 2;
    public float yawResponsiveness = 0.01f;
    public float maxRollAngle = 30;
    [Header("Pitch")]
    public float maxPitchAngle = 35;
    public float pitchResponsiveness = 0.01f;

    [Header("---Other---")]
    [Tooltip("Roll and pitch angle of this transform is changed when turning. The Transform should therefore be a parent of all the visual components. If empty, local transform is used instead.")]
    [SerializeField] Transform characterTransform;
    [Tooltip("If empty, .GetComponent() is used on the .gameObject of this component.")]
    [SerializeField] CharacterInput characterInput;
    [Tooltip("If false, the character won't move.")]
    [SerializeField] bool actionsEnabled = false;

    private StopMethod actionsDisabledStop = StopMethod.NoStop;

    // Additional velocity (added on top of the current velocity each frame)
    [HideInInspector] public AdditionalVelocityCombined additionalVelocity = new AdditionalVelocityCombined();

    // Distinguish between controlling movement of the player or the opponents
    [HideInInspector] public bool isPlayer = true;

    // Components
    Rigidbody rb;
    PlayerCameraController cameraController; // available only for the player (not opponents)

    float maxSpeed;

    // Current values (for gradual change)
    float currentForwardSpeed = 0; // current speed
    float currentYaw = 0, currentRoll = 0, currentPitch = 0; // between -1 and 1
    float previousForwardInput; // direction of the movement, -1, 0 or 1

    // Bonus speed (added on top of the current speed)
    bool hasBonusSpeed = false;
    float maxBonusSpeed = 0;
    float bonusSpeed = 0;
    float bonusSpeedDuration = 0;
    DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> bonusSpeedTween;


    // Enabling/disabling the action inputs
    public void EnableActions() {
        actionsEnabled = true;
    }
    public enum StopMethod {
        ImmediateStop,
        BrakeStop,
        NoStop
    }
    public void DisableActions(StopMethod stopMethod = StopMethod.ImmediateStop) {
        if (actionsEnabled && stopMethod == StopMethod.ImmediateStop)
            ResetMovement();
        actionsDisabledStop = stopMethod;
        actionsEnabled = false;
    }

    // The following 5 methods are used from the broom upgrades
    public void SetForwardResponsiveness(float responsiveness) {
        this.forwardResponsiveness = responsiveness;
    }
    public void SetYawResponsiveness(float responsiveness) {
        this.yawResponsiveness = responsiveness;
    }
    public void SetPitchResponsiveness(float responsiveness) {
        this.pitchResponsiveness = responsiveness;
    }
    // Parameter is number between 0 and 1 describing current maximum speed as a fraction of the maximum possible speed, which is currently 16
    public void SetMaxSpeed(float maxSpeed) {
        this.maxSpeed = maxSpeed * MAX_SPEED;
    }
    public float GetMaxSpeed() {
        return this.maxSpeed;
    }
    public void SetMaxAltitude(float maxAltitude) {
        if (isPlayer)
            PlayerState.Instance.maxAltitude = maxAltitude;
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

        previousForwardInput = 0;

        characterTransform.localEulerAngles = characterTransform.localEulerAngles.WithX(0).WithZ(0);
        currentPitch = 0;
        currentRoll = 0;
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

    private CharacterMovementValues GetMovementInputConsideringDisabledActions() {
        // Get current input
        CharacterMovementValues movementInput = characterInput.GetMovementInput();
        // Override it if actions are disabled
        if (!actionsEnabled) {
            switch (actionsDisabledStop) {
                case StopMethod.BrakeStop:
                    movementInput = new CharacterMovementValues(ForwardMotion.Brake, YawMotion.None, PitchMotion.None);
                    break;
                case StopMethod.ImmediateStop:
                case StopMethod.NoStop:
                    movementInput = new CharacterMovementValues(ForwardMotion.None, YawMotion.None, PitchMotion.None);
                    break;
            }
        }
        return movementInput;
    }

    private void HandleForwardMovement(CharacterMovementValues movementInput) {
        // Forward input
        float forwardInput = (float)movementInput.forwardMotion * movementInput.forwardValue;
        if (!actionsEnabled && actionsDisabledStop == StopMethod.NoStop) // use the previous input value to keep moving with the same velocity
            forwardInput = previousForwardInput;
        previousForwardInput = forwardInput;
        // Resolve bonus speed (if any)
        if (hasBonusSpeed)
            UpdateBonusSpeed(forwardInput);
        // Forward
        currentForwardSpeed += ((forwardInput * maxSpeed) - currentForwardSpeed) * forwardResponsiveness; // change slowly, not immediately
        if (currentForwardSpeed < 0) currentForwardSpeed = 0; // not allowing reverse, only brake
        rb.velocity = characterTransform.forward * (currentForwardSpeed + bonusSpeed); // add also the bonus speed, if any
    }

    private void ApplyAdditionalVelocity() {
        // Additional velocity in this frame (e.g. from a spell effect)
        additionalVelocity.UpdateAdditionalVelocities(Time.fixedDeltaTime);
        rb.velocity += additionalVelocity.GetCurrentAdditionalVelocity();
    }

    // Returns pitch input which is then used for rotation
    private float LimitAltitude(CharacterMovementValues movementInput) {
        float pitchInput = (float)movementInput.pitchMotion * movementInput.pitchValue;
        if (transform.position.y >= PlayerState.Instance.maxAltitude && rb.velocity.y > 0) {
            rb.velocity = rb.velocity.WithY(0);
            if (pitchInput == -1) pitchInput = 0;
        }
        if (transform.position.y <= 0.5 && pitchInput == 1) // TODO: Change this later to distance from the terrain underneath (instead of fixed y)
            pitchInput = 0;
        return pitchInput;
    }

    private void HandleRotations(float pitchInput, float yawInput) {
        // Yaw
        currentYaw += (yawInput - currentYaw) * (isPlayer ? yawResponsiveness : 1); // change slowly, not immediately
        currentRoll += (yawInput - currentRoll) * yawResponsiveness;
        transform.Rotate(Vector3.up, currentYaw * turnSpeed);
        // Roll
        Vector3 eulerAngles = characterTransform.localEulerAngles;
        eulerAngles.z = -currentRoll * maxRollAngle;
        // Pitch
        currentPitch += (pitchInput - currentPitch) * pitchResponsiveness; // change slowly, not immediately
        eulerAngles.x = currentPitch * maxPitchAngle;
        characterTransform.localEulerAngles = eulerAngles;
    }

    private void Awake() {
        if (characterInput == null)
            characterInput = GetComponent<CharacterInput>();
        if (characterInput == null)
            Debug.LogError("An object with CharacterMovementController component must also have any component derived from CharacterInput (e.g. PlayerCharacterInput, OpponentCharacterInput).");
        rb = GetComponent<Rigidbody>();
        isPlayer = gameObject.CompareTag("Player");
        if (isPlayer)
            cameraController = GetComponent<PlayerCameraController>();
        maxSpeed = initialMaxSpeed * MAX_SPEED;
    }

    private void FixedUpdate() {
        // Do nothing if the game is paused
        if (GamePause.pauseState == GamePauseState.Paused) return;

        // Movement
        CharacterMovementValues movementInput = GetMovementInputConsideringDisabledActions();
        HandleForwardMovement(movementInput); // forward (including bonus speed)
        ApplyAdditionalVelocity(); // additional velocity in this frame (e.g. from a spell effect)
        float pitchInput = LimitAltitude(movementInput); // limiting the altitude

        // Rotation
        HandleRotations(pitchInput, (float)movementInput.yawMotion * movementInput.yawValue);
    }
}


// This class is responsible for all additional velocities added on top of the usual velocity (e.g. from input)
public class AdditionalVelocityCombined {

    private List<AdditionalVelocityTweened> additionalVelocities = new List<AdditionalVelocityTweened>();

    public void AddAdditionalVelocity(AdditionalVelocityTweened additionalVelocity) {
        additionalVelocities.Add(additionalVelocity);
    }

    public void UpdateAdditionalVelocities(float deltaTime) {
        // Update all additional velocities and remove those which are finished already
        for (int i = additionalVelocities.Count - 1; i >= 0; i--) {
            additionalVelocities[i].UpdateValue(deltaTime);
            if (additionalVelocities[i].IsFinished)
                additionalVelocities.RemoveAt(i);
        }
    }

    public Vector3 GetCurrentAdditionalVelocity() {
        Vector3 velocity = Vector3.zero;
        foreach (var additionalVelocity in additionalVelocities) {
            velocity += additionalVelocity.CurrentValue;
        }
        return velocity;
    }

}


// This class is responsible for an additional velocity added on top of the usual velocity (e.g. from input)
// Additional velocity is tweened over time
public class AdditionalVelocityTweened {

    public Vector3 CurrentValue => baseVelocity * tweenCurve.Evaluate(currentTimeNormalized);
    public bool IsFinished => (currentTimeNormalized >= 1);

    private Vector3 baseVelocity;
    private float duration;
    private AnimationCurve tweenCurve;

    private float currentTimeNormalized = 0;

    public AdditionalVelocityTweened(Vector3 baseVelocity, float duration, AnimationCurve tweenCurve) {
        this.baseVelocity = baseVelocity;
        this.duration = duration;
        this.tweenCurve = tweenCurve;
    }

    public void UpdateValue(float deltaTime) {
        currentTimeNormalized += (deltaTime / duration);
        if (currentTimeNormalized > 1) currentTimeNormalized = 1;
    }

}