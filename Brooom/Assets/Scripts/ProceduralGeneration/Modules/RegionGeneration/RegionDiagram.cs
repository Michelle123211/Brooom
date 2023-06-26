using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionDiagram", menuName = "Level/Region Diagram")]
public class RegionDiagram : ScriptableObject
{
    [Tooltip("List of region descriptions creating a diagram with functionality similar to Whittaker's biome diagram")]
    public List<WhittakerRegion> diagramRegions;

    [TextArea(10, 15)]
    public string notes;
}


[System.Serializable]
public class WhittakerRegion {
    [Tooltip("Region whose parameters are described.")]
    public LevelRegionType regionType;
    [Tooltip("Minimum values on the X and Y axes occupied by the region in the diagram (between 0 and 1).")]
    public Vector2 minValues;
    [Tooltip("Minimum values on the X and Y axes occupied by the region in the diagram (between 0 and 1).")]
    public Vector2 maxValues;
}
