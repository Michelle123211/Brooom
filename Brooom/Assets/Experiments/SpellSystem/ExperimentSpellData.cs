using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ExperimentSpell", menuName = "Experiment Spell System / Experiment Spell")]
public class ExperimentSpellData : ScriptableObject {

	public string spellName;
	public string description;
	public int mana;
	public Sprite icon;
	public ExperimentSpellEffect effect;
	//public Action<GameObject> effectAction;
	//public UnityEvent effectEvent;

}
