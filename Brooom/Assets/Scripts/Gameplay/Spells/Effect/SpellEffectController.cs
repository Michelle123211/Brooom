using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpellEffectController : MonoBehaviour {

    [Tooltip("A component derived from SpellEffect which is responsible for applying the actual spell effect.")]
    [SerializeField] private SpellEffect actualSpellEffect;

    [Tooltip("A visual effect used when the spell is casted towards its target.")]
    [SerializeField] private SpellTrajectoryVisualEffect spellTrajectoryVisualEffect;
    [Tooltip("A visual effect used when the spell hits its target (it may be even the racer casting the spell).")]
    [SerializeField] private CustomVisualEffect targetHitVisualEffect;

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
        // Based on the spell target handle the visual effect of casting the spell (if it is not null)
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
                currentState = SpellCastState.HIT;
            }
        }
        // Handle hitting the target and invoking the actual effect
        if (currentState == SpellCastState.HIT) {
            if (targetHitVisualEffect != null)
                targetHitVisualEffect.StartPlaying();
            currentState = SpellCastState.EFFECT;
            actualSpellEffect.ApplySpellEffect(castParameters);
        }
        // Handle the actual functional spell effect
        if (currentState == SpellCastState.EFFECT) {
            if (IsSpellFinished()) {
                currentState = SpellCastState.FINISHED;
                // Schedule destroying this spell instance
                Destroy(gameObject, 1f);
            }
        }
	}

	private void LateUpdate() {
        // Update position to follow the target if the target is self
        if (currentState == SpellCastState.EFFECT && castParameters.Spell.TargetType == SpellTargetType.Self) {
            transform.position = castParameters.GetCastPoint();
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