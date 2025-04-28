using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for instantiating bonuses in bonus spots which have already been determined and are specified in the level representation.
/// Each bonus type has different placement parameters and based on them a particular bonus type is assigned to a bonus spot
/// and a certain number of bonus instances are spawned in a row.
/// </summary>
public class BonusPlacement : LevelGeneratorModule {

    [Tooltip("Placement parameters for all the specific types of bonuses. The top ones have higher priority when occupying a spot where more types are applicable.")]
    public List<BonusPlacementParameters> bonuses;

    [Tooltip("The spacing between bonuses in a single row.")]
    public float spacing = 2f;

    [Tooltip("An object which will be parent of all the bonus objects in the hierarchy.")]
    public Transform bonusParent;

    [Tooltip("Whether all bonuses should be instantiated regardless of whether they are available in the current game state (can be used for experimentation).")]
    public bool allBonusesAvailable = false;


    /// <summary>
    /// Instantiates bonuses in bonus spots specified in the level representation, based on some placement parameters of each bonus type.
    /// </summary>
    /// <param name="level"><inheritdoc/></param>
    public override void Generate(LevelRepresentation level) {
        // Remove any previously instantiated bonuses
        UtilsMonoBehaviour.RemoveAllChildren(bonusParent);
        // Place bonus objects
        FillTheBonusSpots(level);
        RemoveEmptySpots(level);
    }

    // For each bonus spots, selects the type of bonus to spawn there based on patterns (if any), chooses a number of instances and then creates them
    private void FillTheBonusSpots(LevelRepresentation level) {
        // Fill the spots according to bonus patterns
        foreach (var bonus in bonuses) {
            if (allBonusesAvailable || bonus.bonusPrefab.IsAvailable()) { // bonuses which are not available (yet) are skipped
                for (int i = 0; i < level.bonuses.Count; i++) {
                    if (level.bonuses[i].isEmpty && bonus.pattern[i % bonus.pattern.Count]) {  // populate empty spots according to the pattern
                        CreateBonusInstances(level, level.bonuses[i], bonus);
                    }
                }
            }
        }
    }

    // Chooses a random number of instances (within an allowed range), instantiates them in a row and assignes them to the bonus spot in the level representation
    private void CreateBonusInstances(LevelRepresentation level, BonusSpot spot, BonusPlacementParameters bonus) {
        // Choose number of bonuses in a single row
        int min = Mathf.Max(Mathf.Min(bonus.countRange.x, bonus.countRange.y), 0); // at least 0 and make it swap-resistant
        int max = Mathf.Max(Mathf.Max(bonus.countRange.x, bonus.countRange.y), 0);
        int count = Random.Range(min, max);
        if (count == 0) return;
        // Compute position parameters
        Vector3 forwardProjected = (level.Track[spot.previousHoopIndex + 1].position - level.Track[spot.previousHoopIndex].position).WithY(0); // forward vector projected to the XZ plane
        Vector3 direction = Vector3.Cross(forwardProjected, Vector3.up).normalized; // right vector (in the direction of a row of bonuses)
        Vector3 leftPosition = spot.position - direction * spacing * ((count - 1) / 2f);
        // Create instances of the bonus
        BonusEffect bonusInstance;
        for (int i = 0; i < count; i++) {
            bonusInstance = Instantiate<BonusEffect>(bonus.bonusPrefab, leftPosition + direction * spacing * i, Quaternion.identity, bonusParent);
            spot.bonusInstances.Add(bonusInstance);
        }
        spot.isEmpty = false;
    }

    // Removes bonus spots without any bonus instances from the level representation
    private void RemoveEmptySpots(LevelRepresentation level) {
        // Remove all spots without assigned bonus object
        for (int i = level.bonuses.Count - 1; i >= 0; i--) {
            if (level.bonuses[i].isEmpty)
                level.bonuses.RemoveAt(i);
        }
    }
}

/// <summary>
/// A class describing placement parameters for a particular type of bonus (i.e. bonus prefab, number of instances, which bonus spots to populate).
/// </summary>
[System.Serializable]
public class BonusPlacementParameters {
    [Tooltip("A name which is displayed in the Inspector when this class' instance is added to a list.")]
    public string visibleName;
    [Tooltip("Prefab of the bonus object of a particular type for which placement parameters are being specified.")]
    public BonusEffect bonusPrefab;
    [Tooltip("How many instances should be next to each other in one spot (min and max, must be non-negative).")]
    public Vector2Int countRange;
    [Tooltip("Pattern describing frequency of occurences (e.g., [false, true] means every other, [true, false, true] means every first and third).")]
    public List<bool> pattern;
}

