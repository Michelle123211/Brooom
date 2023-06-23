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

        // Update player's position relatively to the hoops
        // - check whether the player is after the next hoop
        int nextHoopIndex = PlayerState.Instance.raceState.previousTrackPointIndex + 1;
        if (nextHoopIndex < PlayerState.Instance.raceState.level.track.Count) {
            HoopRelativePosition relativePosition = GetHoopRelativePosition(nextHoopIndex);
            if (relativePosition == HoopRelativePosition.After) {
                PlayerState.Instance.raceState.previousTrackPointIndex = nextHoopIndex;
                // TODO: Higlight the next hoop
            }
        }
        // - check whether the player is before the previous hoop
        int previousHoopIndex = PlayerState.Instance.raceState.previousTrackPointIndex;
        if (previousHoopIndex >= 0) {
            HoopRelativePosition relativePosition = GetHoopRelativePosition(previousHoopIndex);
            if (relativePosition == HoopRelativePosition.Before) {
                PlayerState.Instance.raceState.previousTrackPointIndex = previousHoopIndex - 1;
            }
        }
        // - otherwise the player is still between the same pair of hoops
    }

    private enum HoopRelativePosition { 
        Before,
        At,
        After
    }
    // Checks whether the player is in the space before or after the hoop with the given index
    private HoopRelativePosition GetHoopRelativePosition(int hoopIndex) {
        TrackPoint nextHoopPoint = PlayerState.Instance.raceState.level.track[hoopIndex];
        Vector3 dividingVector = nextHoopPoint.assignedObject.transform.right.WithY(0); // vector dividing space into two parts (before/after the hoop)
        Vector3 playerVector = player.transform.position.WithY(0) - nextHoopPoint.position.WithY(0); // vector from the hoop to the player
        float angle = Vector3.SignedAngle(playerVector, dividingVector, Vector3.up); // angle between the two vectors
        if (angle < 0) return HoopRelativePosition.Before;
        if (angle > 0) return HoopRelativePosition.After;
        else return HoopRelativePosition.At;
    }
}

public class RaceState {
    // Level - to get access to track points and record player's position within the track
    public LevelRepresentation level;
    public int previousTrackPointIndex = -1; // position of the player within the track (index of the last hoop they passed)
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
        level = null;
        previousTrackPointIndex = -1;
        this.currentMana = this.maxMana;
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.Reset();
        }
        selectedSpell = 0;
    }
}
