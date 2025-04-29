using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class for passing parameters between scenes when loading them.
/// Derived classes should override necessary methods to do something based on parameters passed in from the previous scene.
/// <c>SceneLoader</c> then provides methods for passing parameters to the next scene,
/// and when that next scene is loaded, it finds <c>SceneLoadingParameters</c> implementation in the scene and passes the parameters in.
/// </summary>
public abstract class SceneLoadingParameters : MonoBehaviour {

	/// <summary>
	/// Receives the <c>bool</c> parameter from the previous scene and reacts accordingly.
	/// </summary>
	/// <param name="parameterName">Name of the parameter which is passed in.</param>
	/// <param name="parameterValue">Value of the parameter which is passed in.</param>
	public virtual void PassBoolParameter(string parameterName, bool parameterValue) {}
	/// <summary>
	/// Receives the <c>int</c> parameter from the previous scene and reacts accordingly.
	/// </summary>
	/// <param name="parameterName">Name of the parameter which is passed in.</param>
	/// <param name="parameterValue">Value of the parameter which is passed in.</param>
	public virtual void PassIntParameter(string parameterName, int parameterValue) {}
	/// <summary>
	/// Receives the <c>float</c> parameter from the previous scene and reacts accordingly.
	/// </summary>
	/// <param name="parameterName">Name of the parameter which is passed in.</param>
	/// <param name="parameterValue">Value of the parameter which is passed in.</param>
	public virtual void PassFloatParameter(string parameterName, float parameterValue) {}
	/// <summary>
	/// Receives the <c>string</c> parameter from the previous scene and reacts accordingly.
	/// </summary>
	/// <param name="parameterName">Name of the parameter which is passed in.</param>
	/// <param name="parameterValue">Value of the parameter which is passed in.</param>
	public virtual void PassStringParameter(string parameterName, string parameterValue) {}

}
