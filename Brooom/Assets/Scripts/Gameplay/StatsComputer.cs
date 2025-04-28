using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class responsible for computing the player's stats values based on their performance during the race.
/// </summary>
public class StatsComputer : MonoBehaviour {

    // Parameters for Endurance
    [Header("Endurance")]
    [Tooltip("Curve describing amount of change in the Endurance stat based on the player's place in the race.")]
    [SerializeField] AnimationCurve enduranceDeltaBasedOnPlace;
    [Tooltip("Maximum amount of change in the Endurance stat. Exact value depends on the AnimationCurve (this value is multiplied by the one from the AnimationCurve).")]
    [SerializeField] float maxEnduranceDelta = 5;

    // Parameters for Speed
    [Header("Speed")]
    [Tooltip("Time interval (in seconds) between two consecutive speed samples.")]
    [SerializeField] float speedSamplingInterval = 3;

    // Parameters for Dexterity
    [Header("Distance from 'ideal' trajectory")]
    [Tooltip("Time interval (in seconds) between two consecutive samples of distance from the 'ideal' trajectory.")]
    [SerializeField] float distanceSampleInterval = 3;
    [Tooltip("Curve describing dependency of penalization (between 0 and 1) on the distance from the 'ideal' trajectory.")]
    [SerializeField] AnimationCurve penalizationBasedOnDistance;
    [Tooltip("Range of distances in which the exact penalization is computed from the AnimationCurve. Lower distances are not penalized, higher are given the maximum penalization.")]
    [SerializeField] Vector2 distanceRange = new Vector2(0.5f, 10f);

    // Parameters for Dexterity and Precision
    [Header("Obstacle collisions")]
    [Tooltip("Curve describing dependency of collision penalization (between 0 and 1) based on track length (between 0 and 1).")]
    [SerializeField] AnimationCurve collisionPenalizationBasedOnTrackLength;

    // Parameters for Precision
    [Header("Wrong direction")]
    [Tooltip("Curve describing dependency of wrong direction penalization (between 0 and 1) based on track length (between 0 and 1).")]
    [SerializeField] AnimationCurve wrongDirectionPenalizationBasedOnTrackLength;

    // Parameters for Magic
    [Header("Casting spells")]
    [Tooltip("Curve describing fraction of Magic value depending on the percentage of the equipped spells which were used.")]
    [SerializeField] AnimationCurve notUsedSpellPenalization;

    [Header("Error tolerance")]
    [Tooltip("Percentage of errors which is tolerated every time, e.g. if it is 0.2f, then the score will be increased by (100 - score) * 0.2f.")]
    [SerializeField] float errorTolerance;

    [Header("Giving up race")]
    [Tooltip("This fraction of the current stats values will be kept after the player gives up the race.")]
    [SerializeField] float givingUpPenalization = 0.95f;

    private bool isComputing = false;

    // Variables used for stats computation
    private RacerRepresentation playerRepresentation;
    private CharacterRaceState playerRaceState;
    private SpellController playerSpellController;

    private int playerPlace; // Endurance
    private int totalRacers; // Endurance

    private float speedLowerBound; // Speed
    private double currentSpeedSum, maxSpeedSum; // Speed
    private float speedSampleCountdown; // Speed

    private double currentDistancePenalizationSum, maxDistancePenalizationSum; // Dexterity
    private float distanceSampleCountdown; // Dexterity

    private int obstacleCollisionCount; // Dexterity, Precision
    private float obstacleCollisionValue; // Dexterity, Precision

    private int currentEndurance, currentSpeed, currentMagic;
    private float trackLength; // Dexterity, Precision

    private int totalHoops, passedHoops; // Precision
    private int totalBonusWeightSum, pickedUpBonusWeightSum; // Precision

    private int wrongDirectionCount; // Precision

    private int pickedUpMana, usedMana, totalMana; // Magic
    private int currentMana; // Magic

    private bool[] spellUsed; // Magic
    private int spellUsedCount; // Magic
    private int equippedSpellCount; // Magic
    private int totalSpellUsedCount, totalSpellCount; // Magic
    private float equippedSpellUsageValue, spellUsageValue; // Magic

    private int statWeightBasedOnPlace; // When combining the old stat value with the new one, the weight of the new value is based on the place


    /// <summary>
    /// Initializes everything necessary and starts collecting data necessary for stats values' computation. This should be called at the start of the race.
    /// </summary>
    public void StartComputingStats() {
        isComputing = true;

        // Initialize everything necessary
        playerRepresentation = RaceControllerBase.Instance.playerRacer;
        playerRaceState = playerRepresentation.state;
        playerSpellController = playerRaceState.GetComponent<SpellController>();

        totalRacers = RaceControllerBase.Instance.racers.Count;

        speedLowerBound = Mathf.Max((playerRepresentation.characterController.initialMaxSpeed - 0.15f) * CharacterMovementController.MAX_SPEED, 0f);
        currentSpeedSum = 0; maxSpeedSum = 0;
        speedSampleCountdown = speedSamplingInterval;

        currentDistancePenalizationSum = 0; maxDistancePenalizationSum = 0;
        distanceSampleCountdown = distanceSampleInterval;

        obstacleCollisionCount = 0;
        Messaging.RegisterForMessage("ObstacleCollision", OnObstacleCollision);

        currentEndurance = PlayerState.Instance.CurrentStats.endurance;
        currentSpeed = PlayerState.Instance.CurrentStats.speed;
        trackLength = ((currentEndurance * 2 + currentSpeed) / 3f) / 100f;

        currentMagic = PlayerState.Instance.CurrentStats.magic;

        pickedUpBonusWeightSum = 0;
        Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);

        wrongDirectionCount = 0;
        playerRaceState.onWrongDirectionChanged += OnWrongDirectionChanged;

        pickedUpMana = 0; usedMana = 0;
        currentMana = 0;
        playerSpellController.onManaAmountChanged += OnManaAmountChanged;

        spellUsed = new bool[playerSpellController.spellSlots.Length];
        spellUsedCount = 0;
        equippedSpellCount = 0;
        foreach (var spell in playerSpellController.spellSlots)
            if (spell != null && !spell.IsEmpty()) equippedSpellCount++;
        playerSpellController.onSpellCast += OnSpellCast;
}

    /// <summary>
    /// Stops collecting data necessary for stats values' computation. This should be called at the end of the race.
    /// </summary>
    public void StopComputing() {
        isComputing = false;
        // Unregister callbacks
        Messaging.UnregisterFromMessage("ObstacleCollision", OnObstacleCollision);
        Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
        playerRaceState.onWrongDirectionChanged -= OnWrongDirectionChanged;
        playerSpellController.onManaAmountChanged -= OnManaAmountChanged;
        playerSpellController.onSpellCast -= OnSpellCast;
    }

    /// <summary>
    /// Computes new stats values based on collected data, adds some error tolerance, combines the new stats values with the old ones (using weighted average)
    /// and then stores the final values into <c>PlayerState</c>.
    /// </summary>
    public void UpdateStats() {
        // Compute new stats values
        CompleteComputationParameters();
        PlayerStats newValues = new PlayerStats {
            endurance = ComputeEnduranceValue(),
            speed = ComputeSpeedValue(),
            dexterity = ComputeDexterityValue(),
            precision = ComputePrecisionValue(),
            magic = ComputeMagicValue()
        };
        // Finalize stats values with some tolerance for errors
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Speed: Before error tolerance ({errorTolerance}) {newValues.speed} and after {Mathf.Min(Mathf.RoundToInt(newValues.speed + (100 - newValues.speed) * errorTolerance), 100)}.");
        newValues.speed = Mathf.Min(Mathf.RoundToInt(newValues.speed + (100 - newValues.speed) * errorTolerance), 100); // must not exceed 100
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Dexterity: Before error tolerance ({errorTolerance}) {newValues.dexterity} and after {Mathf.Min(Mathf.RoundToInt(newValues.dexterity + (100 - newValues.dexterity) * errorTolerance), 100)}.");
        newValues.dexterity = Mathf.Min(Mathf.RoundToInt(newValues.dexterity + (100 - newValues.dexterity) * errorTolerance), 100); // must not exceed 100
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Precision: Before error tolerance ({errorTolerance}) {newValues.precision} and after {Mathf.Min(Mathf.RoundToInt(newValues.precision + (100 - newValues.precision) * errorTolerance), 100)}.");
        newValues.precision = Mathf.Min(Mathf.RoundToInt(newValues.precision + (100 - newValues.precision) * errorTolerance), 100); // must not exceed 100
        if (equippedSpellCount > 0) { // don't increment the value if no spells are equipped
            Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Magic: Before error tolerance ({errorTolerance}) {newValues.magic} and after {Mathf.Min(Mathf.RoundToInt(newValues.magic + (100 - newValues.magic) * errorTolerance), 100)}.");
            newValues.magic = Mathf.Min(Mathf.RoundToInt(newValues.magic + (100 - newValues.magic) * errorTolerance), 100); // must not exceed 100
        }
        // Compute weighted average of the old and new stats
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

    /// <summary>
    /// Penalizes the player for giving up a race by lowering all of their stats values by a small amount.
    /// </summary>
    public void LowerAllStatsOnRaceGivenUp() {
        // Percentual decrease in all stats
        PlayerStats oldValues = PlayerState.Instance.CurrentStats;
        PlayerStats newValues = new PlayerStats {
            endurance = (int)(oldValues.endurance * givingUpPenalization),
            speed = (int)(oldValues.speed * givingUpPenalization),
            dexterity = (int)(oldValues.dexterity * givingUpPenalization),
            precision = (int)(oldValues.precision * givingUpPenalization),
            magic = (int)(oldValues.magic * givingUpPenalization)
        };
        // Store new values in PlayerState
        PlayerState.Instance.CurrentStats = newValues;
    }

    // Computes all values which are then used as parameters for stats computation
    private void CompleteComputationParameters() {
        // Place
        playerPlace = playerRaceState.place;
        // Number of hoops from CharacterRaceState
        totalHoops = playerRaceState.hoopsPassedArray.Length;
        passedHoops = 0;
        foreach (var isHoopPassed in playerRaceState.hoopsPassedArray)
            if (isHoopPassed) passedHoops++;
        // Sum of weights of all the bonuses (only one instance from each bonus spot) - from RaceController.Instance.level
        totalBonusWeightSum = 0;
        foreach (var bonusSpot in RaceControllerBase.Instance.Level.bonuses) {
            if (bonusSpot.isEmpty) continue;
            totalBonusWeightSum += bonusSpot.bonusInstances[0].bonusWeight;
        }
        // Obstacle collisions - value between 0 and 100 representing how well the player evades collisions
        obstacleCollisionValue = Mathf.Clamp(1 - (obstacleCollisionCount * collisionPenalizationBasedOnTrackLength.Evaluate(trackLength)), 0, 1) * 100;
        // Total amount of mana which can be picked up (assuming the player picks up only one instance in each bonus spot)
        totalMana = 0;
        foreach (var bonusSpot in RaceControllerBase.Instance.Level.bonuses) {
            if (bonusSpot.isEmpty) continue;
            if (bonusSpot.bonusInstances[0].TryGetComponent<ManaBonusEffect>(out ManaBonusEffect manaBonus))
                totalMana += manaBonus.manaAmount;
        }
        ManaRegeneration manaRegeneration = playerSpellController.GetComponentInChildren<ManaRegeneration>();
        totalMana += (manaRegeneration.TotalManaGenerated / 2); // add mana regenerated automatically (divided by two to account for situations when mana is regenerated too fast)
        // Spell usage
        equippedSpellUsageValue = notUsedSpellPenalization.Evaluate(spellUsedCount / (float)equippedSpellCount); // number between 0 and 1 describing how diverse spells the player has cast during the race
        totalSpellUsedCount = 0; // how many of the total spells the player has ever used
        foreach (var spellCast in PlayerState.Instance.spellCast)
            if (spellCast.Value) totalSpellUsedCount++;
        totalSpellCount = Mathf.Max(SpellManager.Instance.AllSpells.Count, 1); // how many spells are available in the game (must be > 0 for further computation)
        spellUsageValue = (totalSpellUsedCount / (float)totalSpellCount); // number between 0 and 1 describing how diverse spells the player has ever cast
        // Weight of the new stat value when combining it with the old one
        float middlePlace = (totalRacers - 1) / 2f + 1;
        statWeightBasedOnPlace = Mathf.FloorToInt(Mathf.Abs(middlePlace - playerPlace) + 1); // e.g. 3 for 1st place among 5-6 racers
    }

    // Computes Endurance value based on collected data
    private int ComputeEnduranceValue() {
        // Change based on place
        float delta = enduranceDeltaBasedOnPlace.Evaluate(1 - ((playerPlace - 1f) / (totalRacers - 1f))) * maxEnduranceDelta;
        int newValue = Mathf.Min(Mathf.RoundToInt(currentEndurance + delta), 100); // must not exceed 100
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Endurance: Player placed {playerPlace}/{totalRacers}, endurance delta is {delta}, new value is {newValue}.");
        return newValue;
    }

    // Computes Speed value based on collected data
    private int ComputeSpeedValue() {
        // Ratio between the sum of measured speed and sum of maximum speed
        int newValue = Mathf.RoundToInt(Mathf.Clamp((float)(currentSpeedSum / maxSpeedSum), 0, 1) * 100); // Clamp in case the player has maximum speed broom upgrade and picks up speed bonuses
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Speed: Current speed sum is  {currentSpeedSum}, maximum speed sum is {maxSpeedSum}, new value is {newValue}.");
        return newValue;
    }

    // Computes Dexterity value based on collected data
    private int ComputeDexterityValue() {
        // Combination of distance from the 'ideal' trajectory and number of collisions with obstacles
        float distancePart = (float)(1 - (currentDistancePenalizationSum / maxDistancePenalizationSum)) * 100;
        float collisionPart = obstacleCollisionValue;
        int newValue = Mathf.RoundToInt(distancePart * 0.4f + collisionPart * 0.6f);
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Dexterity: Distance part is {distancePart} ({currentDistancePenalizationSum}/{maxDistancePenalizationSum}) with weight 0.4, collision part is {collisionPart} ({obstacleCollisionCount}) with weight 0.6, new value is {newValue}.");
        return newValue;
    }

    // Computes Precision value based on collected data
    private int ComputePrecisionValue() {
        // Combination of passed/missed hoops, picked/missed bonuses, obstacle collisions and wrong directions
        float hoopPart = (passedHoops / (float)totalHoops) * 100;
        float bonusPart = Mathf.Clamp(pickedUpBonusWeightSum / (float)totalBonusWeightSum, 0, 1) * 100; // Clamp in case the player picks up more bonuses at the same bonus spot
        float collisionPart = obstacleCollisionValue;
        float wrongDirectionPart = Mathf.Clamp(1 - (wrongDirectionCount * wrongDirectionPenalizationBasedOnTrackLength.Evaluate(trackLength)), 0, 1) * 100;
        int newValue = Mathf.RoundToInt(hoopPart * 0.4f + bonusPart * 0.2f + collisionPart * 0.25f + wrongDirectionPart * 0.15f);
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Precision: Hoop part is {hoopPart} ({passedHoops}/{totalHoops}) with weight 0.4, bonus part is {bonusPart} ({pickedUpBonusWeightSum}/{totalBonusWeightSum}) with weight 0.2, collision part is {collisionPart} ({obstacleCollisionCount}) with weight 0.25, wrong direction part is {wrongDirectionPart} ({wrongDirectionCount}) with weight 0.15, new value is {newValue}.");
        return newValue;
    }

    // Computes Magic value based on collected data
    private int ComputeMagicValue() {
        // Combination of picked up mana bonuses and diverse spell usage
        if (PlayerState.Instance.availableSpellCount == 0) { // no purchased spells, just return 0
            Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Magic: No available spells, new value is 0.");
            return 0;
        }
        float manaPart = ((Mathf.Clamp(pickedUpMana / (float)totalMana, 0, 1) * 2 + Mathf.Clamp(usedMana / (float)totalMana, 0, 1)) / 3) * 100; // Clamp in case the player picks up more bonuses at the same bonus spot
        int newValue = Mathf.RoundToInt(manaPart * equippedSpellUsageValue * spellUsageValue);
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Magic: Mana part is {manaPart} (pickud up mana {pickedUpMana}/{totalMana} with weight 2, used mana {usedMana}/{totalMana} with weight 1), equipped spell usage is {equippedSpellUsageValue} ({spellUsedCount}/{equippedSpellCount}), spell usage is {spellUsageValue} ({totalSpellUsedCount}/{totalSpellCount}), new value is {newValue}.");
        return newValue;
    }

    // Combines old Endurance value with the new one
    private int CombineEnduranceValues(int oldValue, int newValue) {
        // Only the new value may be taken in this case
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Endurance: Old value {oldValue}, new value {newValue}, combined value {newValue}.");
        return newValue;
    }

    // Combines old Speed value with the new one
    private int CombineSpeedValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        //  - weight of current value depends on place
        int combinedValue = Mathf.RoundToInt((oldValue + newValue * statWeightBasedOnPlace) / (float)(statWeightBasedOnPlace + 1));
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Speed: Old value {oldValue} with weight 1, new value {newValue} with weight {statWeightBasedOnPlace}, combined value {combinedValue}.");
        return combinedValue;
    }

    // Combines old Dexterity value with the new one
    private int CombineDexterityValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        //  - weight of current value depends on place
        //  - old value has more weight so the stat does not immediately jump to very high values
        int combinedValue = Mathf.RoundToInt((oldValue * (statWeightBasedOnPlace + 3) + newValue * statWeightBasedOnPlace) / (float)(2 * statWeightBasedOnPlace + 3));
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Dexterity: Old value {oldValue} with weight {statWeightBasedOnPlace + 3}, new value {newValue} with weight {statWeightBasedOnPlace}, combined value {combinedValue}.");
        return combinedValue;
    }

    // Combines old Precision value with the new one
    private int CombinePrecisionValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        //  - weight of current value depends on place
        //  - old value has more weight so the stat does not immediately jump to very high values
        int combinedValue = Mathf.RoundToInt((oldValue * (statWeightBasedOnPlace + 3) + newValue * statWeightBasedOnPlace) / (float)(2 * statWeightBasedOnPlace + 3));
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Precision: Old value {oldValue} with weight {statWeightBasedOnPlace + 3}, new value {newValue} with weight {statWeightBasedOnPlace}, combined value {combinedValue}.");
        return combinedValue;
    }

    // Combines old Magic value with the new one
    private int CombineMagicValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        //  - weight of current value depends on place
        int combinedValue = Mathf.RoundToInt((oldValue + newValue * statWeightBasedOnPlace) / (float)(statWeightBasedOnPlace + 1));
        Analytics.Instance.LogEvent(AnalyticsCategory.Stats, $"Magic: Old value {oldValue} with weight 1, new value {newValue} with weight {statWeightBasedOnPlace}, combined value {combinedValue}.");
        return Mathf.RoundToInt((oldValue + newValue * statWeightBasedOnPlace) / (float)(statWeightBasedOnPlace + 1));
    }

    // Updates collected data
    private void Update() {
        if (isComputing) {
            // Update stats intermediate results
            // --- sample speed in regular intervals
            speedSampleCountdown -= Time.deltaTime;
            if (speedSampleCountdown < 0) {
                speedSampleCountdown += speedSamplingInterval;
                currentSpeedSum += Mathf.Max(playerRepresentation.characterController.GetCurrentSpeed() - speedLowerBound, 0); // decreasing the value by speedLowerBound to increase weight of broom upgrade
                maxSpeedSum += Mathf.Max(CharacterMovementController.MAX_SPEED - speedLowerBound, 0); // decreasing the value by speedLowerBound to increase weight of broom upgrade
            }
            // --- sample distance from "ideal" trajectory between hoops in regular intervals and accumulate penalization
            distanceSampleCountdown -= Time.deltaTime;
            if (distanceSampleCountdown < 0) {
                distanceSampleCountdown += distanceSampleInterval;
                float distance = GetPlayerDistanceFromTrajectory();
                maxDistancePenalizationSum += 1;
                if (distance > distanceRange.x) {
                    distance = Mathf.Clamp(distance, distanceRange.x, distanceRange.y);
                    currentDistancePenalizationSum +=
                        penalizationBasedOnDistance.Evaluate((distance - distanceRange.x) / (distanceRange.y - distanceRange.x));
                }
            }
        }
	}

    // Computes distance between the player and an "ideal" trajectory between hoops
    private float GetPlayerDistanceFromTrajectory() {
        Vector3 playerPosition = playerRaceState.transform.position;
        // Get positions of two track points between which the player is located
        Vector3 firstPoint = RaceControllerBase.Instance.Level.playerStartPosition;
        Vector3 secondPoint = RaceControllerBase.Instance.Level.Track[0].position;
        if (playerRaceState.followingTrackPoint >= RaceControllerBase.Instance.Level.Track.Count) { // the player is behind the last hoop
            firstPoint = RaceControllerBase.Instance.Level.Track[RaceControllerBase.Instance.Level.Track.Count - 1].position;
            secondPoint = RaceControllerBase.Instance.Level.finish.transform.position.WithY(firstPoint.y); // finish line but at the same height as the last hoop
        } else if (playerRaceState.followingTrackPoint > 0) { // the player is somewhere in the middle of the track
            firstPoint = RaceControllerBase.Instance.Level.Track[playerRaceState.followingTrackPoint - 1].position;
            secondPoint = RaceControllerBase.Instance.Level.Track[playerRaceState.followingTrackPoint].position;
        }
        // Project vector from the first point to the player onto the vector from the first point to the second point
        Vector3 projection = Vector3.Project(playerPosition - firstPoint, (secondPoint - firstPoint).normalized);
        // Distance between the player and the projection should be the desired distance
        float distance = Vector3.Distance(playerPosition, firstPoint + projection);
        return distance;
    }

	#region Callbacks

	private void OnObstacleCollision() {
        obstacleCollisionCount++;
    }

    private void OnBonusPickedUp(GameObject bonus) {
        pickedUpBonusWeightSum += bonus.GetComponent<BonusEffect>().bonusWeight;
    }

    private void OnWrongDirectionChanged(bool isWrongDirection) {
        if (isWrongDirection) wrongDirectionCount++;
    }

    private void OnManaAmountChanged(int newValue) {
        if (currentMana < newValue) // mana was picked up
            pickedUpMana += (newValue - currentMana);
        if (currentMana > newValue) // mana was used
            usedMana += (currentMana - newValue);
        currentMana = newValue;
    }

    private void OnSpellCast(int index) {
        if (spellUsed[index]) return;
        // The spell has not been cast yet during this race
        spellUsedCount++;
        spellUsed[index] = true;
    }

	#endregion

}
