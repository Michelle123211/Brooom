using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class for a spell effect handling what eactly should happen when the spell is cast and hits its target.
/// </summary>
public abstract class SpellEffect : MonoBehaviour {

    /// <summary>Who cast which spell and at which target or in which direction.</summary>
    protected SpellCastParameters castParameters;

    /// <summary>
    /// Checks whether the spell effect has been completed.
    /// </summary>
    /// <returns><c>true</c> if the spell effect has finished, <c>false</c> otherwise.</returns>
    public abstract bool IsComplete();

    /// <summary>
    /// Applies the spell effect when the spell hits its target.
    /// </summary>
    /// <param name="castParameters">Parameters of the spell cast (e.g., source object, target object/direction).</param>
    public void ApplySpellEffect(SpellCastParameters castParameters) {
        this.castParameters = castParameters;
        ApplySpellEffect_Internal();
    }

	/// <summary>
	/// Applies a spell-specific effect when the spell hits its target. Different derived classes have specific implementation according to the spell they represent.
	/// </summary>
	protected abstract void ApplySpellEffect_Internal();

}

/// <summary>
/// A base class for a spell effect which is one shot and is complete immediately after being applied.
/// </summary>
public abstract class OneShotSpellEffect : SpellEffect {
    public override bool IsComplete() {
        return true; // always complete because it is immediate
    }
}

/// <summary>
/// A base class for a spell effect which is one shot but takes some time to finish.
/// </summary>
public abstract class DurativeSpellEffect : SpellEffect {

    [Tooltip("Duration for which the spell takes effect in seconds.")]
    [SerializeField] protected float effectDuration = 4;

    private float currentTime = -1;

    private bool isComplete = false;

	public override bool IsComplete() {
        return isComplete;
	}

	/// <summary>
	/// Starts applying a spell-specific effect when the spell hits its target.
    /// The effect is then continued from <c>Update()</c> method to ensure updates throughout the whole duration.
	/// </summary>
	protected override void ApplySpellEffect_Internal() {
        // Start applying the actual spell effect
        StartApplyingSpellEffect();
        currentTime = 0;
    }

    /// <summary>
    /// Initializes everything necessary for applying the spell effect.
    /// </summary>
    protected abstract void StartApplyingSpellEffect();
    /// <summary>
    /// Finalizes everything necesssary when the spell effect has finished (e.g., stops any visual effects).
    /// </summary>
    protected abstract void FinishApplyingSpellEffect();

    /// <summary>
    /// Performs one iteration of the spell effect. It is called from <c>Update()</c>.
    /// </summary>
    /// <param name="time">Number between 0 and 1 (indicating how far we are in the total duration).</param>
    protected abstract void ApplySpellEffect_OneIteration(float time);

    private void Update() {
        // Update the spell effect and check if it is complete
        if (currentTime >= 0 && currentTime < effectDuration) {
            currentTime += Time.deltaTime;
            ApplySpellEffect_OneIteration(Mathf.Clamp(currentTime / effectDuration, 0f, 1f));
            if (currentTime >= effectDuration) {
                FinishApplyingSpellEffect();
                isComplete = true;
            }
        }
    }

}

/// <summary>
/// A base class for a spell effect which is applied and then reversed after some time.
/// </summary>
public abstract class ReversibleSpellEffect : SpellEffect {

    [Tooltip("Duration for which the spell takes effect in seconds.")]
    [SerializeField] protected float effectDuration = 4;

    private bool isComplete = false;

	public override bool IsComplete() {
        return isComplete;
	}

    /// <summary>
    /// Starts applying a spell-specific effect when the spell hits its target and schedules revesion of the effect at the end of its duration.
    /// </summary>
    protected override void ApplySpellEffect_Internal() {
        // Apply the actual spell effect and schedule reversion of the effect
        StartSpellEffect();
        StartCoroutine(nameof(ScheduleEndOfSpellEffect));
    }

    /// <summary>
    /// Starts applying the spell effect.
    /// </summary>
    protected abstract void StartSpellEffect();
    /// <summary>
    /// Stops applying the spell effect (i.e. reverses it).
    /// </summary>
    protected abstract void EndSpellEffect();

    // Wait for the spell effect duration and then finish it
    private IEnumerator ScheduleEndOfSpellEffect() {
        yield return new WaitForSeconds(effectDuration);
        EndSpellEffect();
        isComplete = true;
    }

}

/// <summary>
/// A base class for a spell effect which is appied to a racer and then reversed after some time.
/// </summary>
public abstract class RacerAffectingSpellEffect : ReversibleSpellEffect {

    [Tooltip("A visual effect which is displayed around the racer while they are affected by the spell.")]
    [SerializeField] private SelfDestructiveVisualEffect spellInfluenceVisualEffectPrefab;

    /// <summary>
    /// Adds the spell effect among effects affecting the target racer and registers callbacks for its start and end (to actually start/stop the effect).
    /// </summary>
    /// <exception cref="System.NotSupportedException">Throws <c>NotSupportedException</c> when the target object is not set or it doesn't have <c>EffectibleCharacter</c> component.</exception>
	protected override void StartSpellEffect() {
        EffectibleCharacter targetRacer = null;
        if (castParameters.Target.TargetObject != null) targetRacer = castParameters.Target.TargetObject.GetComponent<EffectibleCharacter>();
        if (castParameters.Target.TargetObject == null || targetRacer == null)
            throw new System.NotSupportedException($"{nameof(RacerAffectingSpellEffect)} and derived classes may be used only for spells cast at other racers.");
        // Add the spell among effects affecting the target racer (+ add the visual effect)
        CharacterEffect characterEffect = new CharacterEffect(castParameters.Spell.Icon, effectDuration, castParameters.Spell.IsPositive);
        characterEffect.onEffectStart += StartSpellEffect_Internal;
        characterEffect.onEffectEnd += StopSpellEffect_Internal;
        targetRacer.AddEffect(characterEffect, spellInfluenceVisualEffectPrefab);
	}

	/// <summary>
	/// Performs anything necesssary when the spell effect has finished.
	/// </summary>
	protected override void EndSpellEffect() {
        // Removing the effect from the character effects is handled automatically
        // The actual effect is stopped at the same time as well
	}

	/// <summary>
	/// Starts applying the spell effect.
	/// </summary>
	protected abstract void StartSpellEffect_Internal();
	/// <summary>
	/// Stops applying the spell effect (i.e. reverses it).
	/// </summary>
	protected abstract void StopSpellEffect_Internal();

}
