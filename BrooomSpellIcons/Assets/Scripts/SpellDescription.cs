using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spell", order = 1)]
public class SpellDescription : ScriptableObject {

	public string spellName;

	public Sprite spellIcon;

	public Color spellIconBackgroundColor;

	[TextArea]
	public string spellDescriptionEnglish;
	[TextArea]
	public string spellDescriptionCzech;

}
