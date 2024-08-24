using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInfluenceEffectColorInitializer : MonoBehaviour {

	[Tooltip("Particle system whose color will be affected.")]
	[SerializeField] ParticleSystem particles;

	[Tooltip("Particles' color will be initialized based on color assigned to a spell with the given identifier.")]
	[SerializeField] string spellIdentifier;

	private void Awake() {
		Spell spell = SpellManager.Instance.GetSpellFromIdentifier(spellIdentifier);
		Color color = spell.BaseColor;
		// Set start color
		ParticleSystem.MainModule mainModule = particles.main;
		mainModule.startColor = color.WithA(0.5f); // TODO: Change the alpha value if necessary (based on sprites used)
		// Set color over lifetime
		Gradient gradient = new Gradient();
		gradient.SetKeys(
			new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 1f) },
			new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.5f), new GradientAlphaKey(0f, 1f) });
		ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = particles.colorOverLifetime;
		colorOverLifetimeModule.color = gradient;
	}
}
