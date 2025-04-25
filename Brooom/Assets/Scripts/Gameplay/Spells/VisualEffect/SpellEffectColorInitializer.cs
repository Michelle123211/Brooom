using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component which initializes visual effect colors (in shaders and particles) based on spell identifier.
/// </summary>
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

	// Sets all shaders' color parameters
	private void SetShaderColorParameters(SpellShaderColorInitializer shaderInitializer) {
		if (shaderInitializer.shaderParameters == null) return;
		Material material = shaderInitializer.renderer.material;
		// Set each parameter
		foreach (var shaderParameter in shaderInitializer.shaderParameters) {
			material.SetColor(shaderParameter.shaderParameterName, GetNewColor(shaderParameter));
		}
	}

	// Sets all particle systems' start color
	private void SetParticlesStartColor(SpellParticlesColorInitializer particlesInitializer) {
		// Set start color
		ParticleSystem.MainModule mainModule = particlesInitializer.particles.main;
		mainModule.startColor = GetNewColor(particlesInitializer.colorParameter);
	}

	// Gets new color based on the spell, considering whether base or emmissive color should be used, and overriding any components if required
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

/// <summary>
/// Which color assigned to the spell should be used when initializing a color of a visual effect.
/// </summary>
public enum SpellColorInitializerType { 
	BaseColor,
	EmissionColor
}

/// <summary>
/// A class describing how a specific color parameter should be initialized based on corresponding spell's color.
/// It allows to choose between base color and emmissive color, and also to override individual color components with a specific cstom value.
/// </summary>
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

/// <summary>
/// A class describing a shader color parameter whos color should be initialized based on spell's color.
/// </summary>
[System.Serializable]
public class SpellShaderColorInitializerParameter : SpellColorInitializerParameter {
	
	[Tooltip("Name of the shader parameter which will be set.")]
	public string shaderParameterName;

}

/// <summary>
/// A class describing a shader whose color parameter should be initialized based on spell's color.
/// </summary>
[System.Serializable]
public class SpellShaderColorInitializer {
	
	[Tooltip("Renderer (e.g. Mesh Renderer or Trail Renderer) whose shader color parameter will be affected.")]
	public Renderer renderer;

	[Tooltip("A list of shader parameters to set.")]
	public List<SpellShaderColorInitializerParameter> shaderParameters;

}

/// <summary>
/// A class describing a particle system whose start color should be initialized based on spell's color.
/// </summary>
[System.Serializable]
public class SpellParticlesColorInitializer {
	
	[Tooltip("Particle system whose start color will be affected.")]
	public ParticleSystem particles;

	[Tooltip("Color to be set as start color.")]
	public SpellColorInitializerParameter colorParameter;

}