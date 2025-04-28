using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class representing an instance of a diagram similar to Whittaker's biome diagram,
/// describing different regions' placement in a 2D space based on two parameter values (one for each axis, between 0 and 1).
/// </summary>
[CreateAssetMenu(fileName = "RegionDiagram", menuName = "Level/Region Diagram")]
public class RegionDiagram : ScriptableObject {

    [Tooltip("List of region descriptions creating a diagram with functionality similar to Whittaker's biome diagram")]
    public List<WhittakerRegion> diagramRegions;

    [Tooltip("Any notes related to this particular region diagram.")]
    [TextArea(10, 15)]
    public string notes;
}


/// <summary>
/// A region which is part of a diagram similar to Whittaker's biome diagram.
/// It occupies a range of values in a 2D space parametrized by two different parameters (one for each axis, values between 0 and 1).
/// </summary>
[System.Serializable]
public class WhittakerRegion {
    [Tooltip("Region whose parameters are described.")]
    public LevelRegionType regionType;
    [Tooltip("Minimum values on the X and Y axes occupied by the region in the diagram (between 0 and 1).")]
    public Vector2 minValues;
    [Tooltip("Maximum values on the X and Y axes occupied by the region in the diagram (between 0 and 1).")]
    public Vector2 maxValues;
}
