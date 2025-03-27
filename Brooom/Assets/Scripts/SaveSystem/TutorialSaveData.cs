using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TutorialSaveData {
	// Current stage of the tutorial
	public string mainStage;
	// Any substate of the current stage (could be empty)
	public string subState;
}
