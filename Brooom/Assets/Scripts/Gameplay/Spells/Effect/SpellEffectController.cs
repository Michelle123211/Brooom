using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component handling the whole lifetime of spell effect after a spell is cast.
/// It makes sure the spell object moves to the target and on target hit the spell effect is applied.
/// It also plays visual effects in corresponding times.
/// At the end, the spell object is destroyed.
/// </summary>
public class SpellEffectController : MonoBehaviour {

    /// <summary>Called when the spell cast has finished, i.e. the spell reached its target. Parameter is this <c>SpellEffectController</c> component.</summary>
    public event Action<SpellEffectController> onSpellCastFinished;
	/// <summary>Called when the spell hits its target. Parameter is this <c>SpellEffectController</c> component.</summary>
	public event Action<SpellEffectController> onSpellHit;

    /// <summary>The spell this <c>SpellEffectController</c> component controls.</summary>
    public Spell Spell => castParameters.Spell;

    [Tooltip("A component derived from SpellEffect which is responsible for applying the actual spell effect.")]
    [SerializeField] private SpellEffect actualSpellEffect;

    [Tooltip("A visual effect used when the spell is cast towards its target.")]
    [SerializeField] private SpellTrajectoryVisualEffect spellTrajectoryVisualEffect;
    [Tooltip("A visual effect used when the spell hits its target (it may be even the racer casting the spell).")]
    [SerializeField] private CustomVisualEffect targetHitVisualEffect;

    private bool isDestroyScheduled = false;

    private enum SpellCastState { // the cast spell goes through several states over its lifetime
        NOT_STARTED,
        CAST, // moving towards the target object/position
        HIT, // hitting the target
        EFFECT, // applying the spell effect
        FINISHED
	}
    private SpellCastState currentState = SpellCastState.NOT_STARTED;

    private SpellCastParameters castParameters;

	/// <summary>
	/// Starts invoking a spell effect, starting from actually casting the spell towards its target together with a visual effect.
    /// If the spell's target is another racer, this method adds the spell among the racer's incoming spells.
	/// </summary>
	/// <param name="castParameters">Parameters of the spell cast (e.g., spell, source object, target object/direction).</param>
	public void InvokeSpellEffect(SpellCastParameters castParameters) {
        this.castParameters = castParameters;
        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.Events.Game.SpellCast, gameObject);
        // Let the target racer know there is an incoming spell
        if (castParameters.Spell.TargetType == SpellTargetType.Opponent)
            castParameters.Target.TargetObject.GetComponentInChildren<IncomingSpellsTracker>().AddIncomingSpell(this);
        // Based on the spell target, handle the visual effect of casting the spell (if it is not null)
        if (castParameters.Spell.TargetType != SpellTargetType.Self && spellTrajectoryVisualEffect != null) {
            currentState = SpellCastState.CAST;
            // Cast spell
            spellTrajectoryVisualEffect.transform.position = transform.position;
            spellTrajectoryVisualEffect.InitializeStartAndTarget(castParameters);
            spellTrajectoryVisualEffect.StartPlaying();
        } else
            currentState = SpellCastState.HIT;
    }

	private void Update() {
        // Handle finishing casting the spell
        if (currentState == SpellCastState.CAST) {
            if (!spellTrajectoryVisualEffect.IsPlaying) {
                onSpellCastFinished?.Invoke(this);
                currentState = SpellCastState.HIT;
            }
        }
        // Handle hitting the target and invoking the actual effect
        if (currentState == SpellCastState.HIT) {
            // If the target has shield and is not the racer themselves, don't continue and finish casting the spell
            GameObject targetObject = castParameters.Target.TargetObject;
            if (castParameters.Spell.TargetType != SpellTargetType.Self && targetObject != null && targetObject.GetComponentInChildren<SpellShield>() != null) {
                AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.Events.Game.SpellBlocked, targetObject);
                currentState = SpellCastState.FINISHED;
            } else {
                // Otherwise, continue invoking the spell effect
                if (targetHitVisualEffect != null)
                    targetHitVisualEffect.StartPlaying();
                if (castParameters.Spell.TargetType != SpellTargetType.Self && targetObject != null)
                    AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.Events.Game.SpellHit, targetObject);
                onSpellHit?.Invoke(this);
                currentState = SpellCastState.EFFECT;
                actualSpellEffect.ApplySpellEffect(castParameters);
            }
        }
        // Handle the actual functional spell effect
        if (currentState == SpellCastState.EFFECT) {
            if (IsSpellFinished()) {
                currentState = SpellCastState.FINISHED;
            }
        }
        // Handle destruction of this spell instance
        if (currentState == SpellCastState.FINISHED && !isDestroyScheduled) {
            Destroy(gameObject, 1f);
            isDestroyScheduled = true;
        }
	}

	private void LateUpdate() {
        // Update position to follow the target if the target is self
        if (currentState == SpellCastState.EFFECT && castParameters.Spell.TargetType == SpellTargetType.Self) {
            transform.position = castParameters.GetCastPosition();
        }
	}

	private bool IsSpellFinished() {
        // The actual spell effect must finish
        if (!actualSpellEffect.IsComplete()) return false;
        // All visual effects must stop playing
        if (spellTrajectoryVisualEffect != null && spellTrajectoryVisualEffect.IsPlaying) return false;
        if (targetHitVisualEffect != null && targetHitVisualEffect.IsPlaying) return false;
        return true;
    }

}