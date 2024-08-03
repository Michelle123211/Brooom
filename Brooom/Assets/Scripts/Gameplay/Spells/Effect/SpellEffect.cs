using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellEffect : MonoBehaviour {

    protected Spell spell;
    protected SpellTarget spellTarget;

    public abstract bool IsComplete();

    public void ApplySpellEffect(Spell spell, SpellTarget spellTarget) {
        this.spell = spell;
        this.spellTarget = spellTarget;
        ApplySpellEffect_Internal();
    }

    protected abstract void ApplySpellEffect_Internal();

}

// A spell effect which is one shot and performed immediately
public abstract class OneShotSpellEffect : SpellEffect {
    public override bool IsComplete() {
        return true; // always complete because it is immediate
    }
}

// A spell effect which is one shot but takes some time to finish
public abstract class DurativeSpellEffect : SpellEffect {

    [Tooltip("Duration for which the spell takes effect in seconds.")]
    [SerializeField] protected float effectDuration = 4;

    private float currentTime = -1;

    private bool isComplete = false;

	public override bool IsComplete() {
        return isComplete;
	}

	protected override void ApplySpellEffect_Internal() {
        // Start applying the actual spell effect
        StartApplyingSpellEffect();
        currentTime = 0;
    }

    // Initialize everything necessary for applying the spell effect
    protected abstract void StartApplyingSpellEffect();
    // Finalize everything necessary when the spell effect has finished (e.g. stop any visual effects)
    protected abstract void FinishApplyingSpellEffect();

    // Called from Update to perform one iteration of the spell effect
    // Parameter time is between 0 and 1 (indicates how far we are in the total duration)
    protected abstract void ApplySpellEffect_OneIteration(float time);

    private void Update() {
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

// A spell effect which is applied and after some time reversed
public abstract class ReversibleSpellEffect : SpellEffect {

    [Tooltip("Duration for which the spell takes effect in seconds.")]
    [SerializeField] protected float effectDuration = 4;

    private bool isComplete = false;

	public override bool IsComplete() {
        return isComplete;
	}

    protected override void ApplySpellEffect_Internal() {
        // Apply the actual spell effect and schedule reversion of the effect
        StartSpellEffect();
        StartCoroutine(nameof(ScheduleEndOfSpellEffect));
    }

    protected abstract void StartSpellEffect();
    protected abstract void EndSpellEffect();

    private IEnumerator ScheduleEndOfSpellEffect() {
        yield return new WaitForSeconds(effectDuration);
        EndSpellEffect();
        isComplete = true;
    }

}

// A spell effect which is applied to racers and after some time reversed
public abstract class RacerAffectingSpellEffect : ReversibleSpellEffect {

    [Tooltip("A visual effect which is displayed around the racer while they are affected by the spell.")]
    [SerializeField] private SelfDestructiveVisualEffect spellInfluenceVisualEffectPrefab;

    [Tooltip("Whether this spell has a positive or negative effect on the target racer.")]
    [SerializeField] private bool isPositive = false;

	protected override void StartSpellEffect() {
        EffectibleCharacter targetRacer = null;
        if (spellTarget.target != null) targetRacer = spellTarget.target.GetComponent<EffectibleCharacter>();
        if (spellTarget.target == null || targetRacer == null)
            throw new System.NotSupportedException($"{nameof(RacerAffectingSpellEffect)} and derived classes may be used only for spells casted at other racers.");
        // Add the spell among effects affecting the target racer (+ add the visual effect)
        targetRacer.AddEffect(new CharacterEffect(spell.Icon, effectDuration, isPositive), spellInfluenceVisualEffectPrefab);
        // Start the actual effect
        StartSpellEffect_Internal();
	}

	protected override void EndSpellEffect() {
        // Removing the effect from the character effects is handled automatically
        // Just stop the actual effect
        StopSpellEffect_Internal();
	}

    protected abstract void StartSpellEffect_Internal();
    protected abstract void StopSpellEffect_Internal();

}
