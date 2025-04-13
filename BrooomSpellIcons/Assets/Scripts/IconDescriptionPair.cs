using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IconDescriptionPair : MonoBehaviour {

	[SerializeField] Image spellIconBackground;
	[SerializeField] Image spellIcon;
	[SerializeField] TMP_Dropdown descriptionDropdown;

	private ConnectingPairs experimentPart;
	private int spellIndex;

	public void Initialize(int spellIndex, SpellDescription spell, List<string> dropdownOptions, ConnectingPairs experimentPartController) {
		this.spellIndex = spellIndex;
		this.experimentPart = experimentPartController;

		spellIconBackground.color = spell.spellIconBackgroundColor;
		spellIcon.sprite = spell.spellIcon;

		descriptionDropdown.ClearOptions();
		descriptionDropdown.AddOptions(dropdownOptions);
	}

	public void OnValueChanged(int value) {
		experimentPart.OnSpellDescriptionAssigned(spellIndex, value);
	}

}
