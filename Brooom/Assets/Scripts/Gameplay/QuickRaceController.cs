using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class handles progress of Quick Race, i.e. making sure the race is started and finished, race results are displayed etc.
/// It is derived from <c>RaceControllerBase</c> so it inherites useful methods and basic functionality, but it modifies it (e.g. there are no coins rewards and race starts immediately without training phase).
/// </summary>
public class QuickRaceController : RaceControllerBase {

	public void GoBackToMenu() {
		SceneLoader.Instance.LoadScene(Scene.MainMenu);
	}

	protected override void AfterInitialization() {
		// Go directly to race
		Destroy(FindObjectOfType<StartingZone>().transform.parent.gameObject); // remove starting zone
		playerRacer.characterName = LocalizationManager.Instance.GetLocalizedString("QuickRaceNamePlayer");// rename player (so it is not the name from the Career mode)
		StartRace();
	}

	protected override void OnRaceGivenUp() {
		GoBackToMenu();
	}

	protected override int[] ComputeCoinRewards() {
		return new int[racers.Count]; // all 0
	}

	protected override void InitializeAnythingRelated() {
	}

	protected override void OnDestroy_Derived() {
	}
}
