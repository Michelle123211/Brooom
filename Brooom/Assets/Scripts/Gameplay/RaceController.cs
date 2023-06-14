using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    void Start()
    {
        // Initialize state at the beginning
        PlayerState.Instance.raceState.Reset();
    }

	private void Update() {
        // Update charge of equipped spells
        PlayerState.Instance.raceState.UpdateSpellsCharge(Time.deltaTime);
	}
}


public class RaceState {
    // Mana
    public int currentMana;
    public int maxMana;
    // Spells
    public List<EquippedSpell> spells;
    public int selectedSpell; // index of currently selected spell

    public RaceState(int manaAmount, List<EquippedSpell> equippedSpells) {
        this.maxMana = manaAmount;
        this.currentMana = this.maxMana;
        this.spells = equippedSpells;
        this.selectedSpell = 0;
    }

    public void ChangeManaAmount(int delta) {
        currentMana = Mathf.Clamp(currentMana + delta, 0, maxMana);
    }

    public void UpdateSpellsCharge(float timeDelta) {
        foreach (var spell in spells) {
            spell.UpdateCharge(timeDelta);
        }
    }

    public void RechargeAllSpells() {
        foreach (var spell in spells) {
            spell.Recharge();
        }
    }

    public void Reset() {
        this.currentMana = this.maxMana;
        foreach (var spell in spells) {
            spell.Reset();
        }
        selectedSpell = 0;
    }
}
