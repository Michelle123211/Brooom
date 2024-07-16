using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpellEffectController : MonoBehaviour {

    [Tooltip("A component derived from SpellEffect which is responsible for applying the actual spell effect.")]
    [SerializeField] private SpellEffect actualSpellEffect;

    [Tooltip("A visual effect used when the spell is casted towards its target.")]
    [SerializeField] private SpellTrajectoryVisualEffect spellTrajectoryVisualEffect;
    [Tooltip("A visual effect used when the spell hits its target (it may be even the racer casting the spell).")]
    [SerializeField] private VisualEffect targetHitVisualEffect;

    private enum SpellCastState {
        NOT_STARTED,
        CAST,
        HIT,
        EFFECT,
        FINISHED
	}
    private SpellCastState currentState = SpellCastState.NOT_STARTED;

    private Spell spell;
    private SpellTarget spellTarget;


    public void InvokeSpellEffect(Spell spell, SpellTarget spellTarget) {
        this.spell = spell;
        this.spellTarget = spellTarget;
        // Based on the spell target handle the visual effect of casting the spell (if it is not null)
        if (spell.targetType != SpellTargetType.Self && spellTrajectoryVisualEffect != null) {
            currentState = SpellCastState.CAST;
            // Cast spell
            spellTrajectoryVisualEffect.transform.position = transform.position;
            spellTrajectoryVisualEffect.InitializeStartAndTarget(spellTarget);
            spellTrajectoryVisualEffect.StartPlaying();
        } else
            currentState = SpellCastState.HIT;
    }

	private void Update() {
        // Handle finishing casting the spell
        if (currentState == SpellCastState.CAST) {
            if (!spellTrajectoryVisualEffect.isPlaying) {
                currentState = SpellCastState.HIT;
            }
        }
        // Handle hitting the target and invoking the actual effect
        if (currentState == SpellCastState.HIT) {
            if (targetHitVisualEffect != null)
                targetHitVisualEffect.StartPlaying();
            currentState = SpellCastState.EFFECT;
            actualSpellEffect.ApplySpellEffect(spell, spellTarget);
        }
        // Handle the actual functional spell effect
        if (currentState == SpellCastState.EFFECT) {
            if (actualSpellEffect.IsComplete()) {
                currentState = SpellCastState.FINISHED;
                // Schedule destroying this spell instance
                Destroy(gameObject, 1f);
            }
        }
	}

}