using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Initializes visual effect colors (shaders, particles) based on spell identifier
public class SpellEffectColorInitializer : MonoBehaviour {

	[Tooltip("Target color will be initialized based on color assigned to a spell with the given identifier.")]
	[SerializeField] string spellIdentifier;

	[Tooltip("Renderers (e.g. Mesh Renderer or Trail Renderer) and their shader parameters which will be initialized.")]
	[SerializeField] List<SpellShaderColorInitializer> affectedRenderers;

	[Tooltip("Particle systems whose start color will be initialied.")]
	[SerializeField] List<SpellParticlesColorInitializer> affectedParticles;

	private Color color;
	private Color emissionColor;

	private void Awake() {
		// Get spell and its colors
		Spell spell = SpellManager.Instance.GetSpellFromIdentifier(spellIdentifier);
		this.color = spell.BaseColor;
		this.emissionColor = spell.EmissionColor;
		// Set colors of everything
		foreach (var shaderInitializer in affectedRenderers)
			SetShaderColorParameters(shaderInitializer);
		foreach (var particlesInitializer in affectedParticles)
			SetParticlesStartColor(particlesInitializer);
	}

	private void SetShaderColorParameters(SpellShaderColorInitializer shaderInitializer) {
		if (shaderInitializer.shaderParameters == null) return;
		Material material = shaderInitializer.renderer.material;
		// Set each parameter
		foreach (var shaderParameter in shaderInitializer.shaderParameters) {
			material.SetColor(shaderParameter.shaderParameterName, GetNewColor(shaderParameter));
		}
	}

	private void SetParticlesStartColor(SpellParticlesColorInitializer particlesInitializer) {
		// Set start color
		ParticleSystem.MainModule mainModule = particlesInitializer.particles.main;
		mainModule.startColor = GetNewColor(particlesInitializer.colorParameter);
	}

	private Color GetNewColor(SpellColorInitializerParameter colorInitializer) {
		Color colorToUse = colorInitializer.colorType == SpellColorInitializerType.BaseColor ? color : emissionColor;
		// Modify individual components if necessary
		if (colorInitializer.overrideR) colorToUse.r = colorInitializer.customRValue;
		if (colorInitializer.overrideG) colorToUse.g = colorInitializer.customGValue;
		if (colorInitializer.overrideB) colorToUse.b = colorInitializer.customBValue;
		if (colorInitializer.overrideA) colorToUse.a = colorInitializer.customAValue;
		return colorToUse;
	}

}

public enum SpellColorInitializerType { 
	BaseColor,
	EmissionColor
}

[System.Serializable]
public class SpellColorInitializerParameter {

	[Tooltip("Which spell color should be used, either a base color, or an emmissive HDR color.")]
	public SpellColorInitializerType colorType = SpellColorInitializerType.EmissionColor;

	[Tooltip("Whether the R value of the spell's color should be overriden with a custom value.")]
	public bool overrideR;
	[Tooltip("Custom R value which will be used instead of the one provided by the spell value.")]
	[Range(0, 1)]
	public float customRValue;

	[Tooltip("Whether the G value of the spell's color should be overriden with a custom value.")]
	public bool overrideG;
	[Tooltip("Custom G value which will be used instead of the one provided by the spell value.")]
	[Range(0, 1)]
	public float customGValue;

	[Tooltip("Whether the B value of the spell's color should be overriden with a custom value.")]
	public bool overrideB;
	[Tooltip("Custom B value which will be used instead of the one provided by the spell value.")]
	[Range(0, 1)]
	public float customBValue;

	[Tooltip("Whether the A value of the spell's color should be overriden with a custom value.")]
	public bool overrideA;
	[Tooltip("Custom A value which will be used instead of the one provided by the spell value.")]
	[Range(0, 1)]
	public float customAValue;

}

[System.Serializable]
public class SpellShaderColorInitializerParameter : SpellColorInitializerParameter {
	
	[Tooltip("Name of the shader parameter which will be set.")]
	public string shaderParameterName;

}

[System.Serializable]
public class SpellShaderColorInitializer {
	
	[Tooltip("Renderer (e.g. Mesh Renderer or Trail Renderer) whose shader color parameter will be affected.")]
	public Renderer renderer;

	[Tooltip("A list of shader parameters to set.")]
	public List<SpellShaderColorInitializerParameter> shaderParameters;

}

[System.Serializable]
public class SpellParticlesColorInitializer {
	
	[Tooltip("Particle system whose start color will be affected.")]
	public ParticleSystem particles;

	[Tooltip("Color to be set as start color.")]
	public SpellColorInitializerParameter colorParameter;

}