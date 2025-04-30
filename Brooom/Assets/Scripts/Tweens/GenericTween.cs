using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A component allowing to easily tween different properties of common components with different parameters.
/// It provides method <c>DoTween()</c> for playing the tween and method <c>UndoTween()</c> for playing the tween in reverse.
/// </summary>
public class GenericTween : MonoBehaviour {

    [Header("Parameters")]
    [Tooltip("Duration of the tween in seconds.")]
    public float duration = 0.5f;
    [Tooltip("The tween will start with the given delay (not applied to the reverse).")]
    public float delay = 0f;
    [Tooltip("If the object should be destroyed when the tween is complete.")]
    public bool destroy;
    [Tooltip("If the object should be enabled at the start and disabled when the untween is complete.")]
    public bool enable;
    [Tooltip("After starting the tween, loop it indefinitely (also considering whether the tween should be reverted after finishing or not).")]
    public bool loop = false;
    [Tooltip("If the tween is started immediately from the start of the object.")]
    public bool playOnAwake = false;
    [Tooltip("If the tween should be reverted immediately after finishing.")]
    public bool revertAfter = false;

    [Header("Tweened properties")]
    // position
    [Tooltip("Start and end positions are given as relative displacements.")]
    public TweenPropertyPosition positionTween = new TweenPropertyPosition();
    // rotation
    [Tooltip("Start and end positions are given as relative displacements.")]
    public TweenPropertyRotation rotationTween = new TweenPropertyRotation();
    // scale
    public TweenPropertyScale scaleTween = new TweenPropertyScale();
    // alpha through CanvasGroup or SpriteRenderer or Image
    [Tooltip("Affects alpha property of the CanvasGroup, SpriteRenderer or Image component.")]
    public TweenPropertyAlpha alphaTween = new TweenPropertyAlpha();
    // color through SpriteRenderer or Image or Light2D or TextMeshProUGUI or Text
    [Tooltip("Affects color of the SpriteRenderer, Image, Light2D, Light, TextMeshProUGUI or Text component.")]
    public TweenPropertyColor colorTween = new TweenPropertyColor();
    // intensity through Light2D
    [Tooltip("Affects intensity of the Light2D or Light component.")]
    public TweenPropertyIntensity intensityTween = new TweenPropertyIntensity();

    [Header("Callbacks")]
    [Tooltip("Called when the tween is finished (if the tween should be reverted after, then this is called only after the reversed tween finished).")]
    public UnityEvent onTweenComplete = null;
    [Tooltip("Called when the untween (i.e. reversed tween) is finished.")]
    public UnityEvent onUntweenComplete = null;

    private Vector3 initialPosition;


    private bool tweenIsRunning = false;
    private bool reversed = false; // if the tween in running in reverse

    private float time = 0;

    /// <summary>
    /// Plays the tween according to the parameters set in the component.
    /// </summary>
    public void DoTween() {
        reversed = false;
        if (delay == 0) InitializeTween();
        else Invoke(nameof(InitializeTween), delay);
    }
    /// <summary>
    /// Plays the tween according to the parameters set in the component in reverse.
    /// </summary>
    public void UndoTween() {
        reversed = true;
        InitializeTween();
    }

    /// <summary>
    /// Checks whether the tween is playing right now.
    /// </summary>
    /// <returns><c>true</c> if the tween is playing, <c>false</c> otherwise.</returns>
    public bool IsPlaying() {
        return tweenIsRunning;
    }

    /// <summary>
    /// Sets initial values and makes sure the tween is running.
    /// </summary>
    private void InitializeTween() {
        // Set initial values
        tweenIsRunning = true;
        time = 0;
        float t = reversed ? 1 : 0;
        if (enable && !reversed)
            gameObject.SetActive(true);
        TweenProperties(t);
    }
    /// <summary>
    /// Sets final values.
    /// </summary>
    private void FinalizeTween() {
        // Set final values
        time = 0;
        float t = reversed ? 0 : 1;
        if (enable && reversed && !loop)
            gameObject.SetActive(false);
        TweenProperties(t);
    }

    // Sets initial values and starts the tween if it should be started in Awake()
    private void Awake() {
        positionTween.SetInitialPosition(transform.position);
        rotationTween.SetInitialRotation(transform.localEulerAngles);
        if (playOnAwake)
            DoTween();
    }

    // Updates the tween if it is running
    private void Update() {
        if (tweenIsRunning) {
            time += Time.unscaledDeltaTime;
            if (time >= duration) {
                // Finalize the tween
                tweenIsRunning = false;
                FinalizeTween();
                // Check if the tween should keep running (loop, reverse)
                if (!reversed && revertAfter) {
                    tweenIsRunning = true;
                    reversed = true;
                    return;
                }
                if (loop) {
                    tweenIsRunning = true;
                    reversed = false;
                    return;
                }
                // Invoke callbacks
                if (reversed && !revertAfter) { // untween complete (playing in reverse but not because of the settings to revert right after playing forward)
                    if (!Utils.IsNullEvent(onUntweenComplete)) onUntweenComplete.Invoke();
                }
                if ((!reversed) || (reversed && revertAfter)) // tween complete (either playing forward, or playing in reverse right after playing forward because of the settings)
                    if (!Utils.IsNullEvent(onTweenComplete)) onTweenComplete.Invoke();
                // Destroy if requested
                if (destroy) Invoke(nameof(DestroySelf), 0.5f);
            } else {
                // Perform a single step
                float t = time / duration; // normalize
                if (reversed) t = 1 - t;
                TweenProperties(t);
            }
        }
    }

    // Updates all tweened properties based on the current time (normalized)
    private void TweenProperties(float time) {
        TweenProperty(positionTween, time);
        TweenProperty(rotationTween, time);
        TweenProperty(alphaTween, time);
        TweenProperty(scaleTween, time);
        TweenProperty(colorTween, time);
        TweenProperty(intensityTween, time);
    }

    // Updates the given property based on the current time (normalized), but only if the property should be tweened
    private void TweenProperty<TValue>(TweenProperty<TValue> tweenProperty, float time) {
        if (tweenProperty.tweenThisProperty)
            tweenProperty.SetTweenedProperty(gameObject, time);
    }

    // Destroys itself (used to destroy the object at the end of the tween if requested)
    private void DestroySelf() {
        Destroy(gameObject);
    }
}
