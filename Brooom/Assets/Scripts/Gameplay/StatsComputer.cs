using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsComputer : MonoBehaviour {

    private bool isComputing = false;

    // Called from RaceController at the start of the race
    public void StartComputingStats() {
        isComputing = true;
        // TODO: Initialize everything necessary
    }

    // Called from RaceController at the end of the race
    public void StopComputingAndUpdateStats() {
        isComputing = false;
        // Finalize stats values
        PlayerStats newValues = new PlayerStats {
            endurance = ComputeEnduranceValue(),
            speed = ComputeSpeedValue(),
            dexterity = ComputeDexterityValue(),
            precision = ComputePrecisionValue(),
            magic = ComputeMagicValue()
        };
        // TODO: Compute weighted average of the old and new stats
        // --- current value usually has more weight than the previous values
        // --- weight of current values depends on: place, stat category (for precision and dexterity, the old value has more weight)
        PlayerStats oldValues = PlayerState.Instance.CurrentStats;
        PlayerStats combinedValues = new PlayerStats {
            endurance = CombineEnduranceValues(oldValues.endurance, newValues.endurance),
            speed = CombineSpeedValues(oldValues.speed, newValues.speed),
            dexterity = CombineDexterityValues(oldValues.dexterity, newValues.dexterity),
            precision = CombinePrecisionValues(oldValues.precision, newValues.precision),
            magic = CombineMagicValues(oldValues.magic, newValues.magic)
        };
        // Store new values in PlayerState
        PlayerState.Instance.CurrentStats = combinedValues;
    }

    // Called from TODO: ??? when player gives up the race
    public void LowerAllStatsOnRaceGivenUp() { 
        // TODO: 5 % decrease in all stats
    }

    private int ComputeEnduranceValue() {
        // TODO
        return 0;
    }

    private int ComputePrecisionValue() {
        // TODO
        return 0;
    }

    private int ComputeSpeedValue() {
        // TODO
        return 0;
    }

    private int ComputeDexterityValue() {
        // TODO
        return 0;
    }

    private int ComputeMagicValue() {
        // TODO
        return 0;
    }

    private int CombineEnduranceValues(int oldValue, int newValue) {
        // TODO
        return (oldValue + newValue) / 2;
    }

    private int CombinePrecisionValues(int oldValue, int newValue) {
        // TODO
        return (oldValue + newValue) / 2;
    }

    private int CombineSpeedValues(int oldValue, int newValue) {
        // TODO
        return (oldValue + newValue) / 2;
    }

    private int CombineDexterityValues(int oldValue, int newValue) {
        // TODO
        return (oldValue + newValue) / 2;
    }

    private int CombineMagicValues(int oldValue, int newValue) {
        // TODO
        return (oldValue + newValue) / 2;
    }

    private void Update() {
        if (isComputing) { 
            // TODO: Update stats intermediate results
            // --- sample speed
            // --- sample distance from "ideal" trajectory between hoops
        }
	}
}
