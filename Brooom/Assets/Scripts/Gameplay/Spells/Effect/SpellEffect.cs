using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellEffect : MonoBehaviour {

    public abstract bool IsComplete();

    public abstract void ApplySpellEffect(SpellTarget spellTarget);

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
    [SerializeField] protected float effectDuration = 0;

    private SpellTarget spellTarget;

    private float currentTime = -1;

    private bool isComplete = false;

	public override bool IsComplete() {
        return isComplete;
	}

	public override void ApplySpellEffect(SpellTarget spellTarget) {
        this.spellTarget = spellTarget;
        // Start applying the actual spell effect
        StartApplyingSpellEffect(this.spellTarget);
        currentTime = 0;
    }

    // Initialize everything necessary for applying the spell effect
    protected abstract void StartApplyingSpellEffect(SpellTarget spellTarget);
    // Finalize everything necessary when the spell effect has finished (e.g. stop any visual effects)
    protected abstract void FinishApplyingSpellEffect(SpellTarget spellTarget);

    // Called from Update to perform one iteration of the spell effect
    // Parameter time is between 0 and 1 (indicates how far we are in the total duration)
    protected abstract void ApplySpellEffect_OneIteration(SpellTarget spellTarget, float time);

    private void Update() {
        if (currentTime >= 0 && currentTime < effectDuration) {
            currentTime += Time.deltaTime;
            ApplySpellEffect_OneIteration(this.spellTarget, Mathf.Clamp(currentTime / effectDuration, 0f, 1f));
            if (currentTime >= effectDuration) {
                FinishApplyingSpellEffect(this.spellTarget);
                isComplete = true;
            }
        }
    }

}

// A spell effect which is applied and after some time reversed
public abstract class ReversibleSpellEffect : SpellEffect {

    [Tooltip("Duration for which the spell takes effect in seconds.")]
    [SerializeField] protected float effectDuration = 0;

    [Tooltip("A visual effect used for indicating that the target is being affected by the spell.")]
    [SerializeField] private VisualEffect spellInfluenceVisualEffect;

    private bool isComplete = false;

	public override bool IsComplete() {
        return isComplete;
	}

	public override void ApplySpellEffect(SpellTarget spellTarget) {
        // Apply the actual spell effect and schedule reversion of the effect
        StartSpellEffect(spellTarget);
        // TODO: Add the spell among effects affecting the target racer (+ add the visual effect)
        StartCoroutine(nameof(ScheduleEndOfSpellEffect));
    }

    protected abstract void StartSpellEffect(SpellTarget spellTarget);
    protected abstract void EndSpellEffect();

    private IEnumerator ScheduleEndOfSpellEffect() {
        yield return new WaitForSeconds(effectDuration);
        EndSpellEffect();
        isComplete = true;
    }

}
