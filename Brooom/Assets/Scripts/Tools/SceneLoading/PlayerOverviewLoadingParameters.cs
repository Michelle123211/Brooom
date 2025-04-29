using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class receiving parameters from the previous scene affecting initialization of PlayerOverview scene.
/// Based on the parameter it can decide to initialize the scene with Shop opened.
/// </summary>
public class PlayerOverviewLoadingParameters : SceneLoadingParameters {

	[Tooltip("Panel containing all UI elements which are part of the Shop.")]
	[SerializeField] ShowHidePanelUI shopUI;

	///<inheritdoc/>
	public override void PassBoolParameter(string parameterName, bool parameterValue) {
		if (parameterName == "OpenShop" && parameterValue == true) {
			shopUI.ShowPanel();
		} else {
			base.PassBoolParameter(parameterName, parameterValue);
		}
	}

}
