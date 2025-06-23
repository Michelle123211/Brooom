using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A visual effect which linearly interpolates given shader parameters between initial and target values over time.
/// </summary>
public class ShaderAffectingVisualEffect : DurativeVisualEffect {

	[Tooltip("Mesh Renderer component whose material is going to be affected.")]
	[SerializeField] MeshRenderer meshRenderer;

	[Tooltip("Float parameters of the shader which should be interpolated.")]
	[SerializeField] List<InterpolatedShaderParameter<float>> floatParameters = new List<InterpolatedShaderParameter<float>>();
	[Tooltip("Color parameters of the shader which should be interpolated.")]
	[SerializeField] List<InterpolatedShaderParameter<Color>> colorParameters = new List<InterpolatedShaderParameter<Color>>();
	[Tooltip("Bool parameters of the shader which should be interpolated.")]
	[SerializeField] List<InterpolatedShaderParameter<bool>> boolParameters = new List<InterpolatedShaderParameter<bool>>();

	/// <inheritdoc/>
	protected override void StartPlaying_AfterDurativeInit() {
		// Set initial values
		foreach (var parameter in floatParameters)
			meshRenderer.material.SetFloat(parameter.parameterName, parameter.initialValue);
		foreach (var parameter in colorParameters)
			meshRenderer.material.SetColor(parameter.parameterName, parameter.initialValue);
		foreach (var parameter in boolParameters)
			meshRenderer.material.SetInt(parameter.parameterName, parameter.initialValue == false ? 0 : 1);
	}

	/// <inheritdoc/>
	protected override void StopPlaying_AfterDurativeFinish() {
		// Set target values
		foreach (var parameter in floatParameters)
			meshRenderer.material.SetFloat(parameter.parameterName, parameter.targetValue);
		foreach (var parameter in colorParameters)
			meshRenderer.material.SetColor(parameter.parameterName, parameter.targetValue);
		foreach (var parameter in boolParameters)
			meshRenderer.material.SetInt(parameter.parameterName, parameter.targetValue == false ? 0 : 1);
	}

	/// <inheritdoc/>
	protected override void UpdatePlaying_WithNormalizedTime(float currentTimeNormalized) {
		// Interpolate values
		foreach (var parameter in floatParameters)
			meshRenderer.material.SetFloat(parameter.parameterName, parameter.initialValue + currentTimeNormalized * (parameter.targetValue - parameter.initialValue));
		foreach (var parameter in colorParameters)
			meshRenderer.material.SetColor(parameter.parameterName, parameter.initialValue + currentTimeNormalized * (parameter.targetValue - parameter.initialValue));
	}
}

/// <summary>
/// A struct describing a single shader parameter which should be interpolated during a <c>ShaderAffectingVisualEffect</c>.
/// It contains the parameter name, initial value and target value.
/// </summary>
/// <typeparam name="T">Type of the shader parameter (e.g., <c>float</c>, <c>bool</c>).</typeparam>
[System.Serializable]
public struct InterpolatedShaderParameter<T> {
	[Tooltip("Name of the shader parameter to be interpolated.")]
	public string parameterName;
	[Tooltip("The initial value of the parameter (will be set at the start of the interpolation).")]
	public T initialValue;
	[Tooltip("The target value of the parameter (will be reached at the end of the interpolation).")]
	public T targetValue;
}



