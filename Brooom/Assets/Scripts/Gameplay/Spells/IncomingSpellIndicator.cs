using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A component representing an indicator of an incoming spell which is displayed on the screen.
/// Scale and opacity are determined from the spell's distance from the player.
/// </summary>
public class IncomingSpellIndicator : MonoBehaviour {

    /// <summary>The incoming spell this indicator represents.</summary>
    public IncomingSpellInfo IncomingSpellInfo { get; private set; }

    [Tooltip("An Image component used as an arrow indicating from which direction the spell is coming.")]
    [SerializeField] Image arrowIcon;

    [Tooltip("An Image component used to cut out an incoming spell icon out of colored background circle.")]
    [SerializeField] Image spellIndicatorIcon;
    [Tooltip("An Image component used as a background cicle from which the spell icon is cut out.")]
    [SerializeField] Image spellIndicatorBackground;

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

    public void Initialize(IncomingSpellInfo incomingSpellInfo) {
        this.IncomingSpellInfo = incomingSpellInfo;
        spellIndicatorIcon.sprite = incomingSpellInfo.SpellObject.Spell.IndicatorIcon;
        spellIndicatorBackground.color = incomingSpellInfo.SpellObject.Spell.BaseColor;
        arrowIcon.color = incomingSpellInfo.SpellObject.Spell.BaseColor;
    }

    // Changes icon size/opacity according to distance from the player
    private void UpdateVisual() {
        float oneMinusDistanceNormalized = 1f - IncomingSpellInfo.DistanceNormalized;
        // Scale - the closer, the bigger
        transform.localScale = Vector3.one * (scaleKeyframes.x + scaleTweenCurve.Evaluate(oneMinusDistanceNormalized) * (scaleKeyframes.y - scaleKeyframes.x));
        // Opacity - the closer, the less transparent
        float alpha = opacityKeyframes.x + opacityTweenCurve.Evaluate(oneMinusDistanceNormalized) * (opacityKeyframes.y - opacityKeyframes.x);
        spellIndicatorBackground.color = spellIndicatorBackground.color.WithA(alpha);
        arrowIcon.color = arrowIcon.color.WithA(alpha);
        // Rotation of the arrow
        float angle = IncomingSpellInfo.GetAngleFromDirection(false);
        arrowIcon.transform.localEulerAngles = arrowIcon.transform.localEulerAngles.WithZ(angle);
    }

	private void Update() {
        UpdateVisual();
	}

	private void Awake() {
        transform.localScale = Vector3.zero;
    }

}
