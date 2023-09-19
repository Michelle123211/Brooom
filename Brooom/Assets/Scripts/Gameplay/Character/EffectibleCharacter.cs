using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectibleCharacter : MonoBehaviour
{
    // Effects acting on the character
    public List<CharacterEffect> effects = new List<CharacterEffect>();

    public Action<CharacterEffect> onNewEffectAdded; // parameter: the added effect

    public void AddEffect(CharacterEffect effect) {
        // If there is already the same effect, increase only the duration
        foreach (var existingEffect in effects) {
            if (existingEffect == effect) {
                existingEffect.OverrideDuration(Mathf.Max(effect.TimeLeft, existingEffect.TimeLeft));
                return;
            }
        }
        // Otherwise add the new effect and call its start action
        effects.Add(effect);
        onNewEffectAdded?.Invoke(effect);
        effect.onEffectStart?.Invoke();
    }

    public void ResetAllEffects() {
        for (int i = effects.Count - 1; i >= 0; i--) {
            effects[i].onEffectEnd?.Invoke(); // reverse the effects if any
            effects.RemoveAt(i);
        }
    }


    private void Update() {
        // Update effects
        for (int i = effects.Count - 1; i >= 0; i--) {
            effects[i].Update(Time.deltaTime);
            if (effects[i].IsFinished()) {
                effects[i].onEffectEnd?.Invoke();
                effects.RemoveAt(i);
            }
        }
    }
}

// A class representing an effect (from spell or bonus) affecting the player
public class CharacterEffect {
    public Sprite Icon { get; private set; }
    public float TimeLeft { get; private set; }
    public Action onEffectStart;
    public Action onEffectEnd;

    public CharacterEffect(Sprite icon, float duration) {
        this.Icon = icon;
        this.TimeLeft = duration;
    }

    public void OverrideDuration(float newDuration) {
        TimeLeft = newDuration;
    }

    public void Update(float deltaTime) {
        TimeLeft -= deltaTime;
    }

    public bool IsFinished() => TimeLeft < 0;

    // Equality override based solely on icon
    // Icon specifies effect type and there cannot be two effects of the same type affecting the player at the same time
    public override bool Equals(object obj) {
        if (obj == null) return false;

        CharacterEffect other = obj as CharacterEffect;
        if ((object)other == null) return false;

        return Icon == other.Icon;
    }
    public bool Equals(CharacterEffect other) {
        if ((object)other == null) return false;

        return Icon == other.Icon;
    }
    public override int GetHashCode() {
        return Icon.GetHashCode();
    }
    public static bool operator ==(CharacterEffect a, CharacterEffect b) {
        if (ReferenceEquals(a, b)) return true;
        if ((object)a == null || (object)b == null) return false;
        return a.Icon == b.Icon;
    }
    public static bool operator !=(CharacterEffect a, CharacterEffect b) {
        return !(a == b);
    }
}