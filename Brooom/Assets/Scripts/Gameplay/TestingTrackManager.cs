using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component managing everything in the Testing Track.
/// It ensures the mana is unlimited and there is no spell cooldown.
/// </summary>
public class TestingTrackManager : MonoBehaviour {

	SpellController playerSpellController;

	// Schedules filling mana up so that it is done independently of the callbacks being invoked (to prevent infinite loop)
	private void ScheduleFillingManaUp(int _) { // the parameter must be there because this method is used as a callback on mana value change
		Invoke(nameof(FillManaUp), 0.1f);
	}

	// Completely fills player's mana
	private void FillManaUp() {
		if (playerSpellController.CurrentMana < playerSpellController.MaxMana) {
			playerSpellController.ChangeManaAmount(playerSpellController.MaxMana - playerSpellController.CurrentMana);
		}
	}

	// Recharges all equipped spells
	private void RechargeAllSpells(int _) { // the parameter must be there because this method is used as a callback on spell cast
		playerSpellController.RechargeAllSpells();
	}

	// Goes back to PlayerOverview scene while restoring opened Shop
	private void ReturnBack() {
		Analytics.Instance.LogEvent(AnalyticsCategory.TestingTrack, "Testing track left.");
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
		SceneLoader.Instance.SetBoolParameterForNextScene("OpenShop", true); // go straight to the Shop
		SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
	}

	private void Update() {
		// Leave if requested
		if (InputManager.Instance.GetBoolValue("Pause"))
			ReturnBack();
	}

	private void Start() {
		playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
		// Disable mana - it will be unlimited
		//	- Whenever a mana amount changes, fill it up
		playerSpellController.onManaAmountChanged -= ScheduleFillingManaUp; // try to unregister in case it was already registered
		playerSpellController.onManaAmountChanged += ScheduleFillingManaUp;
		FillManaUp();
		// Disable spell cooldown - spells will be recharged immediately after use
		//	- Whenever a spell is cast, recharge all spells
		playerSpellController.onSpellCast -= RechargeAllSpells; // try to unregister in case it was already registered
		playerSpellController.onSpellCast += RechargeAllSpells;
		playerSpellController.RechargeAllSpells();
	}

	private void OnDestroy() {
		// Unregister callbacks
		playerSpellController.onManaAmountChanged -= ScheduleFillingManaUp;
		playerSpellController.onSpellCast -= RechargeAllSpells;
	}

}
