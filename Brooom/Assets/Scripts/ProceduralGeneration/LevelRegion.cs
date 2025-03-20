using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelRegion", menuName = "Level/Level Region")]
public class LevelRegion : ScriptableObject
{
    public string displayName;
    public LevelRegionType regionType;
    public Color color;
    public Sprite regionImage;
    // TODO: Add environment features etc. (with some description how to place them - e.g. random rotation in which axis, lower density at the edges of the region, etc.)
}

public enum LevelRegionType {
    NONE = 0,
    // Terrain - default
    AboveWater = 101,
    // Terrain - unlocked by tutorial
    EnchantedForest = 111, // after first race
    // Terrain - unlocked by level length
    AridDesert = 121,
    BloomingMeadow = 122,
    StormyArea = 123,
    // Terrain - unlocked by broom upgrade
    SnowyMountain = 131,
    // Track - default
    MysteriousTunnel = 201,
    // Track - unlocked by broom upgrade
    AboveClouds = 211
}