using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// A component handling movement of a racer based on an input from <c>CharacterInput</c> implementation.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CharacterMovementController : MonoBehaviour {

    public static float MAX_SPEED = 16;

    // Parameters
    [Header("Forward movement")]
    [Tooltip("Number between 0 and 1 describing initial speed as a fraction of the maximum possible speed, which is currently 16.")]
    public float initialMaxSpeed = 0.75f;
    [Tooltip("How quickly the character reacts to a change along the forward axis (1 for immediate change, 0 for no change).")]
    public float forwardResponsiveness = 0.01f;
    [Header("Yaw and Roll")]
    [Tooltip("How quickly the character turns around the yaw axis.")]
    public float turnSpeed = 2;
    [Tooltip("How quickly the character reacts to a change around the yaw axis (1 for immediate change, 0 for no change).")]
    public float yawResponsiveness = 0.01f;
    [Tooltip("What is the maximum angle to which the character will rotate around the roll axis when turning.")]
    public float maxRollAngle = 30;
    [Header("Pitch")]
    public float maxPitchAngle = 35;
    [Tooltip("How quickly the character reacts to a change around the pitch axis (1 for immediate change, 0 for no change).")]
    public float pitchResponsiveness = 0.01f;

    [Header("---Other---")]
    [Tooltip("Roll and pitch angle of this transform is changed when turning. The Transform should therefore be a parent of all the visual components. If empty, local transform is used instead.")]
    [SerializeField] Transform characterTransform;
    [Tooltip("A component for getting desired movement values. If empty, .GetComponent() is used on the .gameObject of this component.")]
    [SerializeField] CharacterInput characterInput;
    [Tooltip("Whether the actions are enabled. If false, the character won't move.")]
    [SerializeField] bool actionsEnabled = false;

    private StopMethod actionsDisabledStop = StopMethod.NoStop; // stop method used when actions are disabled

    /// <summary>Additional velocity added on top of the current velocity each frame. Used e.g. for Flante spell.</summary>
    [HideInInspector] public AdditionalVelocityCombined additionalVelocity = new();

    /// <summary>Whether this component is controlling movement of the player or the opponents.</summary>
    [HideInInspector] public bool isPlayer = true;

    // Components
    private Rigidbody rb;
    private PlayerCameraController cameraController; // available only for the player (not opponents)

    private float maxSpeed; // number between 0 and 1 describing current maximum speed as a fraction of the maximum possible speed (MAX_SPEED)

    // Current values (for gradual change)
    private float currentForwardSpeed = 0; // current speed
    private float currentYaw = 0, currentRoll = 0, currentPitch = 0; // between -1 and 1
    private float previousForwardInput; // direction of the movement, -1, 0 or 1

    // Bonus speed (added on top of the current speed) - used e.g. for bonus increasing racer's speed
    private bool hasBonusSpeed = false;
    private float maxBonusSpeed = 0;
    private float bonusSpeed = 0; // current bonus speed, tweened gradually over time until it reaches maxBonusSpeed or 0
    private float bonusSpeedDuration = 0; // for how many more seconds the bonus speed will be applied
    private DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> bonusSpeedTween; // storing reference so it can be killed when another tween should be started

	#region Enabling/disabling the action inputs
	/// <summary>
	/// Enables actions so that the racer can start moving.
	/// </summary>
	public void EnableActions() {
        actionsEnabled = true;
    }
    /// <summary>Different options of stopping the racer.</summary>
    public enum StopMethod {
        ImmediateStop,
        BrakeStop,
        NoStop
    }
    /// <summary>
    /// Disables actions so that the racer can no longer move.
    /// </summary>
    /// <param name="stopMethod">How to stop the racer's movement once the actions are disabled.</param>
    public void DisableActions(StopMethod stopMethod = StopMethod.ImmediateStop) {
        if (stopMethod == StopMethod.ImmediateStop)
            ResetMovement();
        actionsDisabledStop = stopMethod;
        actionsEnabled = false;
    }
	#endregion

	#region Methods for broom upgrades
    /// <summary>
    /// Sets forward responsiveness (i.e. how quickly the racer reacts to a change along the forward axis) to the given value.
    /// </summary>
    /// <param name="responsiveness">Number between 0 (no change at all) and 1 (immediate change).</param>
	public void SetForwardResponsiveness(float responsiveness) {
        this.forwardResponsiveness = responsiveness;
    }
    /// <summary>
    /// Sets yaw responsiveness (i.e. how quickly the racer reacts to a change around the yaw axis) to the given value.
    /// </summary>
    /// <param name="responsiveness">Number between 0 (no change at all) and 1 (immediate change).</param>
    public void SetYawResponsiveness(float responsiveness) {
        this.yawResponsiveness = responsiveness;
    }
    /// <summary>
    /// Sets yaw responsiveness (i.e. how quickly the racer reacts to a change around the pitch axis) to the given value.
    /// </summary>
    /// <param name="responsiveness">Number between 0 (no change at all) and 1 (immediate change).</param>
    public void SetPitchResponsiveness(float responsiveness) {
        this.pitchResponsiveness = responsiveness;
    }
    /// <summary>
    /// Sets the maximum speed to the given value.
    /// </summary>
    /// <param name="maxSpeed">Number between 0 and 1 as a fraction of the maximum speed possible (stored in <c>MAX_SPEED</c>).</param>
    public void SetMaxSpeed(float maxSpeed) {
        this.maxSpeed = maxSpeed * MAX_SPEED;
    }
    /// <summary>
    /// Gets the current maximum speed.
    /// </summary>
    /// <returns>The maximum achievable speed (the actual speed value, not a fraction of <c>MAX_SPEED</c>).</returns>
    public float GetMaxSpeed() {
        return this.maxSpeed;
    }
    /// <summary>
    /// Sets the maximum altitude the racer can get to. This affects only the player because it is changed directly in <c>PlayerState</c> instance.
    /// </summary>
    /// <param name="maxAltitude">Maximum altitude allowed.</param>
    public void SetMaxAltitude(float maxAltitude) {
        if (isPlayer)
            PlayerState.Instance.maxAltitude = maxAltitude;
    }
	#endregion

    /// <summary>
    /// Adds a bonus speed of the given value affecting the racer for the given duration. This bonus speed is always added on top of the current base speed.
    /// It us used e.g. from a speed-up bonus.
    /// </summary>
    /// <param name="value">The amount of speed to add on top of the base value.</param>
    /// <param name="duration">For how many seconds this bonus speed is added.</param>
	public void SetBonusSpeed(float value, float duration) {
        maxBonusSpeed = Mathf.Max(maxBonusSpeed, value);
        bonusSpeedDuration = Mathf.Max(bonusSpeedDuration, duration); // don't add it but override it so that the bonus does not accumulate for too long
        hasBonusSpeed = true;
        TweenBonusSpeed(maxBonusSpeed, 1f);
        if (isPlayer && cameraController != null)
            cameraController.ZoomIn(true); // zoom the camera in to make the effect stronger
    }

    /// <summary>
    /// Sets the racer's position to the given position, resets rotation to <c>Quaternity.Identity</c> and resets all movement values (so the movements stops immediately).
    /// </summary>
    /// <param name="position">Position to which the racer should be moved.</param>
    public void ResetPosition(Vector3 position) {
        transform.SetPositionAndRotation(position, Quaternion.identity);
        ResetMovement();
    }

    // Resets all values, so the movement is stopped immediately
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

    /// <summary>
    /// Gets basic values which can be displayed in HUD, i.e. current speed, maximum speed, current altitude, maximum altitude.
    /// </summary>
    /// <returns>An array of values in the following order: current speed, max speed, current altitude, max altitude.</returns>
    public float[] GetValuesForDisplay() {
        float[] result = new float[] {
            rb.velocity.magnitude, // current speed
            maxSpeed, //max speed
            transform.position.y, // current altitude
            PlayerState.Instance.maxAltitude // max altitude
        };
        return result;
    }

    /// <summary>
    /// Gets current speed as a magnitude of attached Rigidbody's velocity.
    /// </summary>
    /// <returns>Current speed of the racer.</returns>
    public float GetCurrentSpeed() {
        return rb.velocity.magnitude;
    }
    /// <summary>
    /// Gets current altitude of the racer.
    /// </summary>
    /// <returns>Current altitude (above sea level, not above terrain).</returns>
    public float GetCurrentAltitude() {
        return transform.position.y;
    }

    /// <summary>
    /// Changes the bonus speed added on top of the base speed gradually over time.
    /// </summary>
    /// <param name="targetValue">The target bonus speed value.</param>
    /// <param name="duration">How long (in seconds) it takes to reach the target value.</param>
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
            if (isPlayer && cameraController != null)
                cameraController.ZoomIn(false);
        } else if (previousForwardInput != forwardInput) { // change of direction, tween the bonus speed accordingly, TODO: Will not work with controllers, sign must be compared then
            TweenBonusSpeed(Mathf.Clamp(forwardInput, 0, 1) * maxBonusSpeed, 1f); // tween to 0 if braking or stopping, tween to maxBonusSpeed if going forward
        }
    }

    // Gets current input from CharacterInput and adjusts it according to selected stop method if actions are disabled
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

    // Computes new forward speed (based on movement values) and sets Rigidbody's velocity accordingly
    // Also considers bonus speed, if any
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

    // Gets additional velocity affecting the racer in this frame and set's Rigidbody's velocity accordingly
    private void ApplyAdditionalVelocity() {
        // Additional velocity in this frame (e.g. from a spell effect)
        additionalVelocity.UpdateAdditionalVelocities(Time.fixedDeltaTime);
        rb.velocity += additionalVelocity.GetCurrentAdditionalVelocity();
    }

    // Returns pitch input considering maximum altitude (it does not allow to go above it)
    private float LimitAltitude(CharacterMovementValues movementInput) {
        float pitchInput = (float)movementInput.pitchMotion * movementInput.pitchValue;
        if (transform.position.y >= PlayerState.Instance.maxAltitude && rb.velocity.y > 0) {
            rb.velocity = rb.velocity.WithY(0);
            if (pitchInput == -1) pitchInput = 0;
        }
        // TODO: Automatically level the broom at a certain altitue above ground
        //if (transform.position.y <= 0.5 && pitchInput == 1) // TODO: Change this to distance from the terrain underneath (instead of fixed y)
        //    pitchInput = 0;
        return pitchInput;
    }

    // Handles all rotations (yaw, roll, pitch) of the character
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

    // Movement is handled in FixedUpdate() because it is using Rigidbody
    private void FixedUpdate() {
        // Do nothing if the game is paused
        if (GamePause.PauseState == GamePauseState.Paused) return;

        // Movement
        CharacterMovementValues movementInput = GetMovementInputConsideringDisabledActions();
        HandleForwardMovement(movementInput); // forward (including bonus speed)
        ApplyAdditionalVelocity(); // additional velocity in this frame (e.g. from a spell effect)
        float pitchInput = LimitAltitude(movementInput); // limiting the altitude

        // Rotation
        HandleRotations(pitchInput, (float)movementInput.yawMotion * movementInput.yawValue);
    }
}


/// <summary>
/// This class is responsible for handling all additional velocities (e.g. from spell effects) added on top of the usual velocity (e.g. from input) in <c>CharacterMovementController</c>.
/// It stores a list of such velocities and combines them into one affecting the racer.
/// </summary>
public class AdditionalVelocityCombined {

    // List of all additional velocities which should be considered.
    private List<AdditionalVelocityTweened> additionalVelocities = new();

    /// <summary>
    /// Adds a new additional velocity to a list of currently active additional velocities.
    /// </summary>
    /// <param name="additionalVelocity">Additional velocity (tweened over time) to be added.</param>
    public void AddAdditionalVelocity(AdditionalVelocityTweened additionalVelocity) {
        additionalVelocities.Add(additionalVelocity);
    }

    /// <summary>
    /// Updates all currently active additional velocities. If tween of any velocity finishes, that velocity is removed from a list.
    /// </summary>
    /// <param name="deltaTime">Time elapsed from the last call.</param>
    public void UpdateAdditionalVelocities(float deltaTime) {
        // Update all additional velocities and remove those which are finished already
        for (int i = additionalVelocities.Count - 1; i >= 0; i--) {
            additionalVelocities[i].UpdateValue(deltaTime);
            if (additionalVelocities[i].IsFinished)
                additionalVelocities.RemoveAt(i);
        }
    }

    /// <summary>
    /// Combines all currently active additional velocities into one and returns it.
    /// </summary>
    /// <returns>A single additional velocity which is a combination of all individual additional velocities which are currently active.</returns>
    public Vector3 GetCurrentAdditionalVelocity() {
        Vector3 velocity = Vector3.zero;
        foreach (var additionalVelocity in additionalVelocities) {
            velocity += additionalVelocity.CurrentValue;
        }
        return velocity;
    }

}


/// <summary>
/// This class is responsible for an additional velocity (e.g. from spell effects) added on top of the usual velocity (e.g. from input) in <c>CharacterMovementController</c>.
/// The additional velocity is tweened over time according to an <c>AnimationCurve</c> describing the value based on normalized time.
/// This allows to add velocity which is gradually decreasing until it has no effect anymore.
/// But it can also be constant in which case this only means applying a constant additional velocity for a certain period of time.
/// </summary>
public class AdditionalVelocityTweened {

    /// <summary>Current additional velocity based on base value and current time normalized.</summary>
    public Vector3 CurrentValue => baseVelocity * tweenCurve.Evaluate(currentTimeNormalized);
    /// <summary>Whether tween of this additional velocity has finished (and velocity should be applied anymore).</summary>
    public bool IsFinished => (currentTimeNormalized >= 1);

    private Vector3 baseVelocity; // reference value from which current value is computed
    private float duration; // for how many seconds should the velocity be tweened
    private AnimationCurve tweenCurve; // curve describing multiplier of base velocity based on current time normalized

    private float currentTimeNormalized = 0;

    /// <summary>
    /// Creates a new instance of additional velocity added on top of base velocity in <c>CharacterMovementController</c>.
    /// This velocity is tweened over time.
    /// </summary>
    /// <param name="baseVelocity">Reference velocity values from which current value is computed.</param>
    /// <param name="duration">For how many seconds the velocity should be tweened.</param>
    /// <param name="tweenCurve">Curve describing multiplier of base velocity based on current time normalized.</param>
    public AdditionalVelocityTweened(Vector3 baseVelocity, float duration, AnimationCurve tweenCurve) {
        this.baseVelocity = baseVelocity;
        this.duration = duration;
        this.tweenCurve = tweenCurve;
    }

    /// <summary>
    /// Updates values, necessary to compute current additional velocity value, based on time elapsed from the last call.
    /// </summary>
    /// <param name="deltaTime">Time elapsed from the last time this method was called.</param>
    public void UpdateValue(float deltaTime) {
        // Update current time
        currentTimeNormalized += (deltaTime / duration);
        if (currentTimeNormalized > 1) currentTimeNormalized = 1;
    }

}