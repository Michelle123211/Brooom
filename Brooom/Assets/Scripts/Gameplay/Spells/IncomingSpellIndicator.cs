using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncomingSpellIndicator : MonoBehaviour {

    [Tooltip("An Image component used to cut out an incoming spell icon out of colored background circle.")]
    [SerializeField] Image spellIndicatorIcon;
    [Tooltip("An Image component used as a background cicle from which the spell icon is cut out.")]
    [SerializeField] Image spellIndicatorBackground;

    [Tooltip("Distance from which the indicator will start being visible.")]
    [SerializeField] float maxDistance;

    [Header("Icon scale")]
    [Tooltip("Curve describing interpolation of icon scale based on spell distance. The higher the x, the shorter the distance.")]
    [SerializeField] AnimationCurve scaleTweenCurve;
    [Tooltip("Start and end scale values between which the interpolation would take place.")]
    [SerializeField] Vector2 scaleKeyframes;

    [Header("Icon opacity")]
    [Tooltip("Curve describing interpolation of icon opacity based on spell distance. The higher the x, the shorter the distance.")]
    [SerializeField] AnimationCurve opacityTweenCurve;
    [Tooltip("Start and end opacity values between which the interpolation would take place.")]
    [SerializeField] Vector2 opacityKeyframes;

    public Vector3 SpellDirection { get; private set; }
    public float SpellDistance { get; private set; }
    public float SpellDistanceNormalized => (SpellDistance / maxDistance);
    public SpellEffectController SpellObject { get; private set; }

    private IncomingSpellsTracker spellsTrackerObject;

    public void Initialize(SpellEffectController spellObject, IncomingSpellsTracker spellsTrackerObject) {
        spellIndicatorIcon.sprite = spellObject.Spell.IndicatorIcon;
        spellIndicatorBackground.color = spellObject.Spell.BaseColor;
        this.SpellObject = spellObject;
        this.spellsTrackerObject = spellsTrackerObject;
        RecomputeState();
    }

    // Computes spell distance and direction
    private void RecomputeState() {
        SpellDistance = Vector3.Distance(spellsTrackerObject.transform.position, SpellObject.transform.position);
        SpellDirection = spellsTrackerObject.transform.InverseTransformPoint(SpellObject.transform.position).normalized;
    }

    // Changes icon size/opacity according to distance
    private void UpdateVisual() {
        float oneMinusDistanceNormalized = 1f - (Mathf.Clamp(SpellDistance, 0f, maxDistance) / maxDistance);
        // Scale
        transform.localScale = Vector3.one * (scaleKeyframes.x + scaleTweenCurve.Evaluate(oneMinusDistanceNormalized) * (scaleKeyframes.y - scaleKeyframes.x));
        // Opacity
        spellIndicatorBackground.color = spellIndicatorBackground.color.WithA(opacityKeyframes.x + opacityTweenCurve.Evaluate(oneMinusDistanceNormalized) * (opacityKeyframes.y - opacityKeyframes.x));
    }

	private void Update() {
        RecomputeState();
        UpdateVisual();
	}

	private void Awake() {
        transform.localScale = Vector3.zero;
    }

}
