using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class used when persistently storing current tutorial state.
/// </summary>
[System.Serializable]
public class TutorialSaveData {
	/// <summary>Current stage of the tutorial.</summary>
	public string mainStage;
	/// <summary>Any substate of the current stage (could be empty).</summary>
	public string subState;
}
