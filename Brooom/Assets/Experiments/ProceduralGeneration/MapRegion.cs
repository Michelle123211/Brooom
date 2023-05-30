using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MapRegion", menuName = "Level/Map Region")]
public class MapRegion : ScriptableObject
{
    public string displayName;
    public MapRegionType regionType;
    public Color color;
    // TODO: Add environment features etc. (with some description how to place them - e.g. random rotation in which axis, lower density at the edges of the region, etc.)
}

public enum MapRegionType {
    NONE = 0,
    // Natural
    AboveWater = 1,
    SnowyMountain = 2,
    AboveClouds = 3,
    Tunnel = 4,
    // Artificial
    EnchantedForest = 101,
    AridDesert = 102,
    BloomingMeadow = 103,
    StormyArea = 104
}

public abstract class NaturalMapRegion : MapRegion {
    public abstract void SatisfiesCondition(Vector3 position);
}

public abstract class ArtificialMapRegion : MapRegion { 

}

public class EnchantedForest : ArtificialMapRegion {
    public EnchantedForest() {
        regionType = MapRegionType.EnchantedForest;
        color = Utils.ColorFromRBG256(65, 176, 106);
    }
}

public class AridDesert : ArtificialMapRegion {
    public AridDesert() {
        regionType = MapRegionType.AridDesert;
        color = Utils.ColorFromRBG256(230, 242, 127);
    }
}