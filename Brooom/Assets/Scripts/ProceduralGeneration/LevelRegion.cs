using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelRegion", menuName = "Level/Level Region")]
public class LevelRegion : ScriptableObject
{
    public string displayName;
    public LevelRegionType regionType;
    public Color color;
    // TODO: Add environment features etc. (with some description how to place them - e.g. random rotation in which axis, lower density at the edges of the region, etc.)
}

public enum LevelRegionType {
    NONE = 0,
    // Terrain - default
    AboveWater = 101,
    EnchantedForest = 102,
    // Terrain - unlocked by level length
    AridDesert = 111,
    BloomingMeadow = 112,
    StormyArea = 113,
    // Terrain - unlocked by broom upgrade
    SnowyMountain = 121,
    // Track - default
    Tunnel = 201,
    // Track - unlocked by broom upgrade
    AboveClouds = 211
}