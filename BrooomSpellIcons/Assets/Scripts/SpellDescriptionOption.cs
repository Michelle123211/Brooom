using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;



public class SpellDescriptionOption : MonoBehaviour {

	[SerializeField] TextMeshProUGUI letter;
	[SerializeField] TextMeshProUGUI englishDescription;
	[SerializeField] TextMeshProUGUI czechDescription;

	[SerializeField] Image background;

	[SerializeField] Color unselectedColor;
	[SerializeField] Color selectedColor;


	public void Initialize(string letter, string englishDescription, string czechDescription) {
		this.letter.text = letter;
		this.englishDescription.text = englishDescription;
		this.czechDescription.text = czechDescription;
		SelectOrDeselect(false);
	}

	public void SelectOrDeselect(bool select) {
		if (select) background.DOColor(selectedColor, 0.3f);
		else background.DOColor(unselectedColor, 0.3f);
	}

}
