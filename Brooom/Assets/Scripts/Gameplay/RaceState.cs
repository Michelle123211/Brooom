using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// A class representing state during the race
public class RaceState {
    // Regions
    public Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();

    public void SetRegionAvailability(LevelRegionType region, bool availability) {
        if (regionsAvailability.ContainsKey(region) && availability && !regionsAvailability[region] && availability) { // a new region became available
            // Notify anyone interested that a new region has been unlocked
            Messaging.SendMessage("NewRegionAvailable");
        }
        regionsAvailability[region] = availability;
    }
}
