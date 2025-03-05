using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpellEffectController : MonoBehaviour {

    public event Action<SpellEffectController> onSpellCastFinished;
    public event Action<SpellEffectController> onSpellHit;

    public Spell Spell => castParameters.Spell;

    [Tooltip("A component derived from SpellEffect which is responsible for applying the actual spell effect.")]
    [SerializeField] private SpellEffect actualSpellEffect;

    [Tooltip("A visual effect used when the spell is casted towards its target.")]
    [SerializeField] private SpellTrajectoryVisualEffect spellTrajectoryVisualEffect;
    [Tooltip("A visual effect used when the spell hits its target (it may be even the racer casting the spell).")]
    [SerializeField] private CustomVisualEffect targetHitVisualEffect;

    private bool isDestroyScheduled = false;

    private enum SpellCastState {
        NOT_STARTED,
        CAST,
        HIT,
        EFFECT,
        FINISHED
	}
    private SpellCastState currentState = SpellCastState.NOT_STARTED;

    private SpellCastParameters castParameters;


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