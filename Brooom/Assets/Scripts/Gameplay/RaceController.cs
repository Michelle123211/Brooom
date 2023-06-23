using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    // Related objects
    private LevelGenerationPipeline levelGenerator;
    private PlayerController player;

    void Start()
    {
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
        player = FindObjectOfType<PlayerController>();
        // Initialize state at the beginning
        PlayerState.Instance.raceState.Reset();
        // Generate level (terrain + track)
        PlayerState.Instance.raceState.level = levelGenerator.GenerateLevel();
    }

	private void Update() {
        // Update charge of equipped spells
        PlayerState.Instance.raceState.UpdateSpellsCharge(Time.deltaTime);
	}
}

public class RaceState {
    // Level - to get access to track points and record player's position within the track
    public LevelRepresentation level;
    public int previousTrackPointIndex; // position of the player within the track (index of the last hoop they passed)
    // Mana
    public int currentMana;
    public int maxMana;
    // Spells
    public EquippedSpell[] spellSlots;
    public int selectedSpell; // index of currently selected spell

    public RaceState(int manaAmount, EquippedSpell[] equippedSpells) {
        this.maxMana = manaAmount;
        this.currentMana = this.maxMana;
        this.spellSlots = equippedSpells;
        this.selectedSpell = 0;
    }

    public void ChangeManaAmount(int delta) {
        currentMana = Mathf.Clamp(currentMana + delta, 0, maxMana);
    }

    public void UpdateSpellsCharge(float timeDelta) {
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.UpdateCharge(timeDelta);
        }
    }

    public void RechargeAllSpells() {
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.Recharge();
        }
    }

    public void Reset() {
        this.currentMana = this.maxMana;
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.Reset();
        }
        selectedSpell = 0;
    }
}
