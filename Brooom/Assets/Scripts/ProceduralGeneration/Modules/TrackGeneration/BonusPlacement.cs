using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusPlacement : LevelGeneratorModule {

    [Tooltip("Placement parameters for all the specific types of bonuses. The top ones have higher priority when occupying a spot.")]
    public List<BonusPlacementParameters> bonuses;

    [Tooltip("The spacing between bonuses in a single row.")]
    public float spacing = 2f;

    [Tooltip("An object which will be parent of all the bonus objects in the hierarchy.")]
    public Transform bonusParent;


    public override void Generate(LevelRepresentation level) {
        // Remove any previously instantiated bonuses
        UtilsMonoBehaviour.RemoveAllChildren(bonusParent);
        // Place bonus objects
        FillTheBonusSpots(level);
        RemoveEmptySpots(level);
    }

    private void FillTheBonusSpots(LevelRepresentation level) {
        // Fill the spots according to bonus patterns
        foreach (var bonus in bonuses) {
            if (!bonus.bonusPrefab.IsAvailable()) continue; // skip bonuses which are not available (yet)
            for (int i = 0; i < level.bonuses.Count; i++) {
                if (level.bonuses[i].isEmpty && bonus.pattern[i % bonus.pattern.Count]) {  // populate empty spots according to the pattern
                    CreateBonusInstances(level, level.bonuses[i], bonus);
                }
            }
        }
    }

    private void CreateBonusInstances(LevelRepresentation level, BonusSpot spot, BonusPlacementParameters bonus) {
        // Choose number of bonuses in a single row
        int min = Mathf.Max(Mathf.Min(bonus.countRange.x, bonus.countRange.y), 0); // at least 0 and make it swap-resistant
        int max = Mathf.Max(Mathf.Max(bonus.countRange.x, bonus.countRange.y), 0);
        int count = Random.Range(min, max);
        if (count == 0) return;
        // Compute position parameters
        Vector3 forwardProjected = (level.track[spot.previousHoopIndex + 1].position - level.track[spot.previousHoopIndex].position).WithY(0); // forward vector projected to the XZ plane
        Vector3 direction = Vector3.Cross(forwardProjected, Vector3.up).normalized; // right vector (in the direction of a row of bonuses)
        Vector3 leftPosition = spot.position - direction * spacing * ((count - 1) / 2f);
        // Create instances of the bonus
        for (int i = 0; i < count; i++) {
            Instantiate<BonusEffect>(bonus.bonusPrefab, leftPosition + direction * spacing * i, Quaternion.identity, bonusParent);
        }
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
    public BonusEffect bonusPrefab;
    [Tooltip("How many instances should be next to each other in one spot (min and max, must be non-negative).")]
    public Vector2Int countRange;
    [Tooltip("Pattern describing frequency of occurences (e.g. [false, true] means every other, e.g. [true, false, true] means every first and third).")]
    public List<bool> pattern;
}

