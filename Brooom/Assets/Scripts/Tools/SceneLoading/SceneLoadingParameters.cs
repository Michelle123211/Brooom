using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneLoadingParameters : MonoBehaviour {

	public virtual void PassBoolParameter(string parameterName, bool parameterValue) {}
	public virtual void PassIntParameter(string parameterName, int parameterValue) {}
	public virtual void PassFloatParameter(string parameterName, float parameterValue) {}
	public virtual void PassStringParameter(string parameterName, string parameterValue) {}

}
