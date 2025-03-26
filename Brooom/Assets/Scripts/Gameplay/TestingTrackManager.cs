using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Ensures the mana is unlimited and there is no spell cooldown
public class TestingTrackManager : MonoBehaviour {

	SpellController playerSpellController;

	private void ScheduleFillingManaUp(int _) { // the parameter must be there because this method is used as a callback on mana value change
		// Schedule filling mana up so that it is done independently of the current callbacks being invoked
		Invoke(nameof(FillManaUp), 0.1f);
	}

	private void FillManaUp() {
		if (playerSpellController.CurrentMana < playerSpellController.MaxMana) {
			playerSpellController.ChangeManaAmount(playerSpellController.MaxMana - playerSpellController.CurrentMana);
		}
	}

	private void RechargeAllSpells(int _) { // the parameter must be there because this method is used as a callback on spell cast
		playerSpellController.RechargeAllSpells();
	}

	private void ReturnBack() {
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
		SceneLoader.Instance.SetBoolParameterForNextScene("OpenShop", true); // go straight to the Shop
		SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
	}

	private void Update() {
		if (InputManager.Instance.GetBoolValue("Pause"))
			ReturnBack();
	}

	private void Start() {
		playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
		// Disable mana - it will be unlimited
		//	- Whenever a mana amount changes, fill it up
		playerSpellController.onManaAmountChanged -= ScheduleFillingManaUp; // try to unregister in case it wal already registered
		playerSpellController.onManaAmountChanged += ScheduleFillingManaUp;
		FillManaUp();
		// Disable spell cooldown - spells will be recharged immediately after use
		//	- Whenever a spell is cast, recharge all spells
		playerSpellController.onSpellCast -= RechargeAllSpells; // try to unregister in case it wal already registered
		playerSpellController.onSpellCast += RechargeAllSpells;
		playerSpellController.RechargeAllSpells();
	}

	private void OnDestroy() {
		// Unregister callbacks
		playerSpellController.onManaAmountChanged -= ScheduleFillingManaUp;
		playerSpellController.onSpellCast -= RechargeAllSpells;
	}

}
