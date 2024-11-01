using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewLoadingParameters : SceneLoadingParameters {

	[SerializeField] ShowHidePanelUI shopUI;

	public override void PassBoolParameter(string parameterName, bool parameterValue) {
		if (parameterName == "OpenShop" && parameterValue == true) {
			shopUI.ShowPanel();
		} else {
			base.PassBoolParameter(parameterName, parameterValue);
		}
	}

}
