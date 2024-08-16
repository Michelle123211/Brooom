using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This Visual Effect interpolates given shader parameters over time
public class ShaderAffectingVisualEffect : DurativeVisualEffect {

	[Tooltip("Mesh Renderer component whose material is going to be affected.")]
	[SerializeField] MeshRenderer meshRenderer;

	[SerializeField] List<InterpolatedShaderParameter<float>> floatParameters = new List<InterpolatedShaderParameter<float>>();
	[SerializeField] List<InterpolatedShaderParameter<Color>> colorParameters = new List<InterpolatedShaderParameter<Color>>();

	protected override void StartPlaying_AfterDurativeInit() {
		// Set initial values
		foreach (var parameter in floatParameters)
			meshRenderer.material.SetFloat(parameter.parameterName, parameter.initialValue);
		foreach (var parameter in colorParameters)
			meshRenderer.material.SetColor(parameter.parameterName, parameter.initialValue);
	}

	protected override void StopPlaying_AfterDurativeFinish() {
		// Set target values
		foreach (var parameter in floatParameters)
			meshRenderer.material.SetFloat(parameter.parameterName, parameter.targetValue);
		foreach (var parameter in colorParameters)
			meshRenderer.material.SetColor(parameter.parameterName, parameter.targetValue);
	}

	protected override void UpdatePlaying_WithNormalizedTime(float currentTimeNormalized) {
		// Interpolate values
		foreach (var parameter in floatParameters)
			meshRenderer.material.SetFloat(parameter.parameterName, parameter.initialValue + currentTimeNormalized * (parameter.targetValue - parameter.initialValue));
		foreach (var parameter in colorParameters)
			meshRenderer.material.SetColor(parameter.parameterName, parameter.initialValue + currentTimeNormalized * (parameter.targetValue - parameter.initialValue));
	}
}

[System.Serializable]
public struct InterpolatedShaderParameter<T> {
	public string parameterName;
	public T initialValue;
	public T targetValue;
}



