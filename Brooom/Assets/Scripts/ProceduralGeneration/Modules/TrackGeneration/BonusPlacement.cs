using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusPlacement : LevelGeneratorModule {

    [Tooltip("Placement parameters for all the specific types of bonuses. The top ones have higher priority when occupying a spot.")]
    public List<BonusPlacementParameters> bonuses;

    [Tooltip("An object which will be parent of all the bonus objects in the hierarchy.")]
    public Transform bonusParent;


    public override void Generate(LevelRepresentation level) {
        // Remove any previously instantiated bonuses
        for (int i = bonusParent.childCount - 1; i >= 0; i--) {
            Destroy(bonusParent.GetChild(i).gameObject);
        }
        // Place bonus objects
        FillTheBonusSpots(level);
        RemoveEmptySpots(level);
    }

    private void FillTheBonusSpots(LevelRepresentation level) {
        // Fill the spots according to bonus patterns
        foreach (var bonus in bonuses) {
            for (int i = 0; i < level.bonuses.Count; i++) {
                if (level.bonuses[i].isEmpty && bonus.pattern[i % bonus.pattern.Count]) {  // populate empty spots according to the pattern
                    CreateBonusInstance(level.bonuses[i], bonus);
                }
            }
        }
    }

    private void CreateBonusInstance(BonusSpot spot, BonusPlacementParameters bonus) {
        // Create an instance of the bonus
        Bonus bonusInstance = Instantiate<Bonus>(bonus.bonusPrefab, spot.position, Quaternion.identity, bonusParent);
        spot.isEmpty = false;
    }

    private void RemoveEmptySpots(LevelRepresentation level) {
        // Remove all spots without assigned bonus object
        for (int i = level.bonuses.Count - 1; i >= 0; i--) {
            if (level.bonuses[i].isEmpty)
                level.bonuses.RemoveAt(i);
        }
    }
}

// Describes placement parameters for a specific type of bonus
[System.Serializable]
public class BonusPlacementParameters {
    public string visibleName;
    [Tooltip("Prefab of the bonus object.")]
    public Bonus bonusPrefab;
    [Tooltip("How many instances should be next to each other in one spot (min and max).")]
    public Vector2Int countRange;
    [Tooltip("Pattern describing frequency of occurences (e.g. [false, true] means every other, e.g. [true, false, true] means every first and third).")]
    public List<bool> pattern;
}

