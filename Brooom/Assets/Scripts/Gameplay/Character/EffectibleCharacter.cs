using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// A component managing all effects currently acting on the character (could be positive or negative).
/// It keeps track of all these effects, adds new ones when requested and removes the ones which have finished already.
/// </summary>
public class EffectibleCharacter : MonoBehaviour {

	/// <summary>A list of effects acting on the character, either positively, or negatively.</summary>
	public List<CharacterEffect> effects = new List<CharacterEffect>();

	/// <summary>Called when a new effect starts affecting the character. Parameter is the new effect added.</summary>
	public event Action<CharacterEffect> onNewEffectAdded;

	[Tooltip("A parent object of all visual effect instances corresponding to effects acting on the character (e.g. particles).")]
	[SerializeField] private Transform visualEffectsParent;

	/// <summary>
	/// Adds a new effect acting on the character into the list.
	/// If an effect of the same type already exists, its remaining duration is simply increased to the duration of the newly added effect.
	/// </summary>
	/// <param name="effect">The new effect to be added.</param>
	/// <param name="visualEffectPrefab">Visual effect to be instantiated around the character to indicate that the effect is acting on them.</param>
	public void AddEffect(CharacterEffect effect, CustomVisualEffect visualEffectPrefab = null) {
		// If there is already the same effect, increase only the duration
		foreach (var existingEffect in effects) {
			if (existingEffect == effect) {
				existingEffect.OverrideDuration(Mathf.Max(effect.TimeLeft, existingEffect.TimeLeft));
				return;
			}
		}
		// Otherwise add the new effect, setup its visual effect (if any) and call its start action
		effects.Add(effect);
		if (visualEffectPrefab != null) {
			CustomVisualEffect visualEffectInstance = Instantiate<CustomVisualEffect>(visualEffectPrefab, visualEffectsParent);
			effect.SetupVisualEffect(visualEffectInstance);
		}
		onNewEffectAdded?.Invoke(effect);
		effect.onEffectStart?.Invoke();
	}

	/// <summary>
	/// Resets all currently active effects, i.e. reverses the effects if necessary and removes them from a list.
	/// </summary>
	public void ResetAllEffects() {
		for (int i = effects.Count - 1; i >= 0; i--) {
			effects[i].onEffectEnd?.Invoke(); // reverse the effects if any
			effects.RemoveAt(i);
		}
	}

	/// <summary>
	/// Resets only effects which are considered negative, i.e. reverses these effects if necessary and removes them from a list.
	/// </summary>
	public void ResetAllNegativeEffects() {
		for (int i = effects.Count - 1; i >= 0; i--) {
			if (!effects[i].IsPositive) {
				effects[i].onEffectEnd?.Invoke(); // reverse the effects if any
				effects.RemoveAt(i);
			}
		}
	}


	private void Update() {
		// Update effects
		for (int i = effects.Count - 1; i >= 0; i--) {
			effects[i].UpdateEffect(Time.deltaTime);
			if (effects[i].IsFinished()) {
				effects[i].onEffectEnd?.Invoke();
				effects.RemoveAt(i);
			}
		}
	}
}

/// <summary>
/// A class representing an effect (e.g. from spell or bonus) affecting the character.
/// </summary>
public class CharacterEffect {
	/// <summary>Icon of the effect which is displayed in HUD when the effect is active.</summary>
	public Sprite Icon { get; private set; }
	/// <summary>For how many more seconds the effect will be active.</summary>
	public float TimeLeft { get; private set; }
	/// <summary>Whether the effect is positive, otherwise negative.</summary>
	public bool IsPositive { get; private set; }

	/// <summary>Called when the effect starts affecting the character.</summary>
	public Action onEffectStart;
	/// <summary>Called when the effect stops affecting the character.</summary>
	public Action onEffectEnd;

	private CustomVisualEffect visualEffect;

	public CharacterEffect(Sprite icon, float duration, bool isPositive) {
		this.Icon = icon;
		this.TimeLeft = duration;
		this.IsPositive = isPositive;
	}

	/// <summary>
	/// Changes the effect's time left to the new duration.
	/// </summary>
	/// <param name="newDuration">New duration to be set for the effect.</param>
	public void OverrideDuration(float newDuration) {
		TimeLeft = newDuration;
	}

	/// <summary>
	/// Updates the effect (its remaining time) based on the time elapsed from the last call.
	/// </summary>
	/// <param name="deltaTime">Time elapsed from the last call.</param>
	public void UpdateEffect(float deltaTime) {
		TimeLeft -= deltaTime;
	}

	/// <summary>
	/// Checks whether the effect is still active or it has finished already.
	/// </summary>
	/// <returns><c>true</c> if the effect has finished already, <c>false</c> otherwise.</returns>
	public bool IsFinished() => TimeLeft < 0;

	/// <summary>
	/// Sets the visual effect for the effect and registers callbacks for starting/stopping it when the effect starts/stops.
	/// </summary>
	/// <param name="visualEffect">Visual effect indicating the effect acting on the character.</param>
	public void SetupVisualEffect(CustomVisualEffect visualEffect) {
		this.visualEffect = visualEffect;
		// Register callbacks for starting/stopping the visual effect
		onEffectStart += StartPlayingVisualEffect;
		onEffectEnd += StopPlayingVisualEffect;
	}

	/// <summary>
	/// Compares two effects based solely on icon, because ucon specifies effect type and there cannot be two effects of the same type affecting the character at the same time.
	/// </summary>
	/// <param name="obj">The other effect to compare this one with.</param>
	/// <returns><c>true</c> if the two effects are equal, <c>false</c> otherwise.</returns>
	public override bool Equals(object obj) {
		if (obj == null) return false;

		CharacterEffect other = obj as CharacterEffect;
		if ((object)other == null) return false;

		return Icon == other.Icon;
	}
	/// <summary>
	/// Compares two effects based solely on icon, because ucon specifies effect type and there cannot be two effects of the same type affecting the character at the same time.
	/// </summary>
	/// <param name="other">The other effect to compare this one with.</param>
	/// <returns><c>true</c> if the two effects are equal, <c>false</c> otherwise.</returns>
	public bool Equals(CharacterEffect other) {
		if ((object)other == null) return false;

		return Icon == other.Icon;
	}
	public override int GetHashCode() {
		return Icon.GetHashCode();
	}
	/// <summary>
	/// Compares two effects based solely on icon, because ucon specifies effect type and there cannot be two effects of the same type affecting the character at the same time.
	/// </summary>
	/// <param name="a">The first effect to compare.</param>
	/// <param name="b">The second effect to compare</param>
	/// <returns><c>true</c> if the two effects are equal, <c>false</c> otherwise.</returns>
	public static bool operator ==(CharacterEffect a, CharacterEffect b) {
		if (ReferenceEquals(a, b)) return true;
		if ((object)a == null || (object)b == null) return false;
		return a.Icon == b.Icon;
	}
	public static bool operator !=(CharacterEffect a, CharacterEffect b) {
		return !(a == b);
	}

	private void StartPlayingVisualEffect() {
		visualEffect?.StartPlaying();
	}

	private void StopPlayingVisualEffect() {
		visualEffect?.StopPlaying();
	}
}