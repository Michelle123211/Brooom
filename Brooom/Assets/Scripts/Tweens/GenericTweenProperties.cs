using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;


#region Properties which can be tweened

/// <summary>
/// A base class for representations of properties which can be tweened in <c>GenericTween</c>.
/// </summary>
/// <typeparam name="TValue"></typeparam>
[System.Serializable]
public abstract class TweenProperty<TValue> {

    [Tooltip("If this particular property should be tweened.")]
    public bool tweenThisProperty;

    [Tooltip("Object whose property is tweened. It has to be set only if it is different than the one with the GenericTween component.")]
    public GameObject tweenTarget;

    // Generic fields are not serialized so they must be in the derived classes explicitly

    /// <summary>
    /// Sets the tweened property's value to the value of the tween (used for tweening this property) in the given time.
    /// </summary>
    /// <param name="defaultTarget">Object whose property is tweened if <c>tweenTarget</c> is not set for this property.</param>
    /// <param name="time">Number between 0 and 1 (i.e. normalized time).</param>
    public void SetTweenedProperty(GameObject defaultTarget, float time) {
        TweenPropertyValues<TValue> tweenPropertyValues = GetTweenPropertyValues();
        SetTweenedPropertyInternal(tweenTarget != null ? tweenTarget : defaultTarget, time);
    }

    /// <summary>
    /// Gets a representation of a tween (containing start value, end value, interpolation curve(s)) used for tweening this property.
    /// </summary>
    /// <returns>Tween values for the property.</returns>
    protected abstract TweenPropertyValues<TValue> GetTweenPropertyValues();
    /// <summary>
    /// Sets the tweened property's value to the value of the tween (used for tweening this property) in the given time.
    /// </summary>
    /// <param name="target">Object whose property is tweened.</param>
    /// <param name="time">Number between 0 and 1 (i.e. normalized time).</param>
    protected abstract void SetTweenedPropertyInternal(GameObject target, float time);

}

/// <summary>
/// A class used from <c>GenericTween</c> to tween <c>position</c> property of <c>Transform</c> component.
/// </summary>
[System.Serializable]
public class TweenPropertyPosition : TweenProperty<Vector3> {

    /// <summary>Tween values (i.e. start value, end value, interpolation curve(s)) used for tweening this property.</summary>
    public TweenPropertyValuesVector3 tweenPropertyValues = new TweenPropertyValuesVector3();

    private Vector3 initialPosition;

    /// <summary>
    /// Sets initial position. Tweened value is then added to this position as a relative offset.
    /// </summary>
    /// <param name="position">Initial position.</param>
    public void SetInitialPosition(Vector3 position) {
        initialPosition = position;
    }

    /// <inheritdoc/>
    protected override TweenPropertyValues<Vector3> GetTweenPropertyValues() {
        return tweenPropertyValues;
    }
    /// <inheritdoc/>
    protected override void SetTweenedPropertyInternal(GameObject target, float time) {
        target.transform.position = initialPosition + tweenPropertyValues.GetTweenValue(time);
    }
}

/// <summary>
/// A class used from <c>GenericTween</c> to tween rotation property (<c>localEulerAngles</c>) of <c>Transform</c> component.
/// </summary>
[System.Serializable]
public class TweenPropertyRotation : TweenProperty<Vector3> {

    /// <summary>Tween values (i.e. start value, end value, interpolation curve(s)) used for tweening this property.</summary>
    public TweenPropertyValuesVector3 tweenPropertyValues = new TweenPropertyValuesVector3();

    private Vector3 initialRotation;

    /// <summary>
    /// Sets initial rotation. Tweened value is then added to this rotation as a relative offset.
    /// </summary>
    /// <param name="rotation">Initial rotation as Euler angles.</param>
    public void SetInitialRotation(Vector3 rotation) {
        initialRotation = rotation;
    }

    /// <inheritdoc/>
    protected override TweenPropertyValues<Vector3> GetTweenPropertyValues() {
        return tweenPropertyValues;
    }
    /// <inheritdoc/>
    protected override void SetTweenedPropertyInternal(GameObject target, float time) {
        target.transform.localEulerAngles = initialRotation + tweenPropertyValues.GetTweenValue(time);
    }
}

/// <summary>
/// A class used from <c>GenericTween</c> to tween scale property (<c>localScale</c>) of <c>Transform</c> component.
/// </summary>
[System.Serializable]
public class TweenPropertyScale : TweenProperty<Vector3> {

    /// <summary>Tween values (i.e. start value, end value, interpolation curve(s)) used for tweening this property.</summary>
    public TweenPropertyValuesVector3 tweenPropertyValues = new TweenPropertyValuesVector3();

    /// <inheritdoc/>
    protected override TweenPropertyValues<Vector3> GetTweenPropertyValues() {
        return tweenPropertyValues;
    }
    /// <inheritdoc/>
    protected override void SetTweenedPropertyInternal(GameObject target, float time) {
        target.transform.localScale = tweenPropertyValues.GetTweenValue(time);
    }
}

/// <summary>
/// A class used from <c>GenericTween</c> to tween alpha property of <c>CanvasGroup</c>, <c>SpriteRenderer</c>, or <c>Image</c> component.
/// </summary>
[System.Serializable]
public class TweenPropertyAlpha : TweenProperty<float> {

    /// <summary>Tween values (i.e. start value, end value, interpolation curve(s)) used for tweening this property.</summary>
    public TweenPropertyValuesFloat tweenPropertyValues = new TweenPropertyValuesFloat();

    /// <inheritdoc/>
    protected override TweenPropertyValues<float> GetTweenPropertyValues() {
        return tweenPropertyValues;
    }
    /// <inheritdoc/>
    protected override void SetTweenedPropertyInternal(GameObject target, float time) {
        // Find a suitable component and set its alpha
        float currentAlpha = tweenPropertyValues.GetTweenValue(time);
        var canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup != null) {
            canvasGroup.alpha = currentAlpha;
            return;
        }
        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.color = spriteRenderer.color.WithA(currentAlpha);
            return;
        }
        var image = target.GetComponent<Image>();
        if (image != null) {
            image.color = image.color.WithA(currentAlpha);
            return;
        }
        Debug.LogWarning($"Target object ({target.name}) for tweening the alpha does not have a suitable component (e.g. Sprite Renderer, Image, CanvasGroup).");
    }
}

/// <summary>
/// A class used from <c>GenericTween</c> to tween color property of <c>SpriteRenderer</c>, <c>Image</c>, 
/// <c>Light2D</c>, <c>Light</c>, <c>TextMeshProUGUI</c>, or <c>Text</c> component.
/// </summary>
[System.Serializable]
public class TweenPropertyColor : TweenProperty<Color> {

    /// <summary>Tween values (i.e. start value, end value, interpolation curve(s)) used for tweening this property.</summary>
    public TweenPropertyValuesColor tweenPropertyValues = new TweenPropertyValuesColor();

    /// <inheritdoc/>
    protected override TweenPropertyValues<Color> GetTweenPropertyValues() {
        return tweenPropertyValues;
    }
    /// <inheritdoc/>
    protected override void SetTweenedPropertyInternal(GameObject target, float time) {
        // Find a suitable component and set its color
        Color currentColor = tweenPropertyValues.GetTweenValue(time);
        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.color = currentColor;
            return;
        }
        var image = target.GetComponent<Image>();
        if (image != null) {
            image.color = currentColor;
            return;
        }
        var light2d = target.GetComponent<Light2D>();
        if (light2d != null) {
            light2d.color = currentColor;
            return;
        }
        var light = target.GetComponent<Light>();
        if (light != null) {
            light.color = currentColor;
            return;
        }
        var tmpro = target.GetComponent<TextMeshProUGUI>();
        if (tmpro != null) {
            tmpro.color = currentColor;
            return;
        }
        var text = target.GetComponent<Text>();
        if (text != null) {
            text.color = currentColor;
            return;
        }
        Debug.LogWarning($"Target object ({target.name}) for tweening the color does not have a suitable component (e.g. SpriteRenderer, Image, Light2D, Light, TextMeshProUGUI, Text).");
    }
}

/// <summary>
/// A class used from <c>GenericTween</c> to tween intensity property of <c>Light2D</c>, or <c>Light</c> component.
/// </summary>
[System.Serializable]
public class TweenPropertyIntensity : TweenProperty<float> {

    /// <summary>Tween values (i.e. start value, end value, interpolation curve(s)) used for tweening this property.</summary>
    public TweenPropertyValuesFloat tweenPropertyValues = new TweenPropertyValuesFloat();

    /// <inheritdoc/>
    protected override TweenPropertyValues<float> GetTweenPropertyValues() {
        return tweenPropertyValues;
    }
    /// <inheritdoc/>
    protected override void SetTweenedPropertyInternal(GameObject target, float time) {
        // Find a suitable component and set its intensity
        var light2d = target.GetComponent<Light2D>();
        if (light2d != null) {
            light2d.intensity = tweenPropertyValues.GetTweenValue(time);
            return;
        }
        var light = target.GetComponent<Light>();
        if (light != null) {
            light.intensity = tweenPropertyValues.GetTweenValue(time);
            return;
        }
        Debug.LogWarning($"Target object ({target.name}) for tweening the intensity does not have a suitable component (e.g. Light2D, Light).");
    }
}

#endregion



#region Values of the tweened properties

/// <summary>
/// A base class representing a simple tween based on the type of the tweened value.
/// Start value and end value are specified here as well as a curve (or multiple curves, one for each component) describing the interpolation.
/// Derived classes are then specialized on a specific type.
/// </summary>
/// <typeparam name="TValue">Type of the tweened value.</typeparam>
[System.Serializable]
public abstract class TweenPropertyValues<TValue> {

    [Tooltip("Initial value from which the tween starts.")]
    public TValue startValue;
    [Tooltip("End value in which the tween ends.")]
    public TValue endValue;
    [Tooltip("A curve or a set of curves (one for each component) describing the interpolation between the start and the end value over time.")]
    public TweenVectorCurves tweenCurves;

    public TweenPropertyValues() {
        tweenCurves = new TweenVectorCurves(NumOfComponents);
    }

    /// <summary>Number of components in the value type.</summary>
    public abstract int NumOfComponents { get; }
    /// <summary>
    /// Gets the value of the tween in the given time (by interpolating between the start value and end value).
    /// </summary>
    /// <param name="t">Number between 0 and 1 (i.e. normalized time).</param>
    /// <returns>Tweened value in time <c>t</c>.</returns>
    public abstract TValue GetTweenValue(float t);
}

/// <summary>
/// A class representing a simple tween of <c>Vector3</c> value.
/// Start value and end value are specified here as well as a curve (or multiple curves, one for each component) describing the interpolation.
/// This class also provides a method for getting the tweened value in a specific time.
/// </summary>
[System.Serializable]
public class TweenPropertyValuesVector3 : TweenPropertyValues<Vector3> {

    /// <inheritdoc/>
    public override int NumOfComponents => 3;

    /// <inheritdoc/>
    public override Vector3 GetTweenValue(float t) {
        if (tweenCurves.tweenComponentWise) {
            float newX = startValue.x + tweenCurves.tweenCurves[0].Evaluate(t) * (endValue.x - startValue.x);
            float newY = startValue.y + tweenCurves.tweenCurves[1].Evaluate(t) * (endValue.y - startValue.y);
            float newZ = startValue.z + tweenCurves.tweenCurves[2].Evaluate(t) * (endValue.z - startValue.z);
            return new Vector3(newX, newY, newZ);
        } else {
            return startValue + tweenCurves.tweenCurve.Evaluate(t) * (endValue - startValue);
        }
    }
}

/// <summary>
/// A class representing a simple tween of <c>float</c> value.
/// Start value and end value are specified here as well as a curve describing the interpolation.
/// This class also provides a method for getting the tweened value in a specific time.
/// </summary>
[System.Serializable]
public class TweenPropertyValuesFloat : TweenPropertyValues<float> {

    /// <inheritdoc/>
    public override int NumOfComponents => 1;

    /// <inheritdoc/>
    public override float GetTweenValue(float t) {
        return startValue + tweenCurves.tweenCurve.Evaluate(t) * (endValue - startValue);
    }
}

/// <summary>
/// A class representing a simple tween of <c>Color</c> value.
/// Start value and end value are specified here as well as a curve (or multiple curves, one for each component) describing the interpolation.
/// This class also provides a method for getting the tweened value in a specific time.
/// </summary>
[System.Serializable]
public class TweenPropertyValuesColor : TweenPropertyValues<Color> {
    /// <inheritdoc/>
    public override int NumOfComponents => 4;

    /// <inheritdoc/>
    public override Color GetTweenValue(float t) {
        if (tweenCurves.tweenComponentWise) {
            float newR = startValue.r + tweenCurves.tweenCurves[0].Evaluate(t) * (endValue.r - startValue.r);
            float newG = startValue.g + tweenCurves.tweenCurves[1].Evaluate(t) * (endValue.g - startValue.g);
            float newB = startValue.b + tweenCurves.tweenCurves[2].Evaluate(t) * (endValue.b - startValue.b);
            float newA = startValue.a + tweenCurves.tweenCurves[03].Evaluate(t) * (endValue.a - startValue.a);
            return new Color(newR, newG, newB, newA);
        } else {
            return startValue + tweenCurves.tweenCurve.Evaluate(t) * (endValue - startValue);
        }
    }
}

#endregion



/// <summary>
/// A class containing curves describing interpolation between start value and end value.
/// If the value type has multiple components, they may be tweened together (using the same curve), or separately (each having its own curve).
/// </summary>
[System.Serializable]
public class TweenVectorCurves {
    // TODO: If tweenComponentWise == false, show tweenCurve, otherwise tweenCurves

    [Tooltip("A curve describing interpolation between the start value and the end value (as a number between 0 and 1) over time (normalized between 0 and 1).")]
    [ConditionalHide(nameof(tweenComponentWise), true)] // hidden if tweenComponentWise is true
    public AnimationCurve tweenCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // TODO: Show only if numOfComponents is > 1
    [Tooltip("Whether each vector component should be tweened separately according to its own animation curve.")]
    public bool tweenComponentWise; // true = tween each axis separately, false = tween the vector as a whole

    [Tooltip("Curves describing component-wise interpolation between the start value and the end value (as a number between 0 and 1) over time (normalized between 0 and 1).")]
    [ConditionalHide(nameof(tweenComponentWise))]
    public AnimationCurve[] tweenCurves;

    /// <summary>
    /// Creates a new instance of <c>TweenVectorCurves</c> describing a tween of value type with the given number of components 
    /// according to specified curves (initially linear from 0 to 1).
    /// </summary>
    /// <param name="numOfComponents">Number of components in the value type.</param>
    public TweenVectorCurves(int numOfComponents = 3) {
        tweenCurves = new AnimationCurve[numOfComponents];
        for (int i = 0; i < numOfComponents; ++i) {
            tweenCurves[i] = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }

}
