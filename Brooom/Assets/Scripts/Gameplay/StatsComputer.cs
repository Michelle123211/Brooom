using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsComputer : MonoBehaviour {

    // Parameters for Endurance[
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


    // Called from RaceController at the start of the race
    public void StartComputingStats() {
        isComputing = true;

        // Initialize everything necessary
        playerRepresentation = RaceController.Instance.playerRacer;
        playerRaceState = playerRepresentation.state;
        playerSpellController = playerRaceState.GetComponent<SpellController>();

        totalRacers = RaceController.Instance.racers.Count;

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

    // Called from RaceController at the end of the race
    public void StopComputing() {
        isComputing = false;
        // Unregister callbacks
        Messaging.UnregisterFromMessage("ObstacleCollision", OnObstacleCollision);
        Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
        playerRaceState.onWrongDirectionChanged -= OnWrongDirectionChanged;
        playerSpellController.onManaAmountChanged -= OnManaAmountChanged;
        playerSpellController.onSpellCast -= OnSpellCast;
    }

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
        newValues.speed = Mathf.Min(Mathf.RoundToInt(newValues.speed + (100 - newValues.speed) * errorTolerance), 100); // must not exceed 100
        newValues.dexterity = Mathf.Min(Mathf.RoundToInt(newValues.dexterity + (100 - newValues.dexterity) * errorTolerance), 100); // must not exceed 100
        newValues.precision = Mathf.Min(Mathf.RoundToInt(newValues.precision + (100 - newValues.precision) * errorTolerance), 100); // must not exceed 100
        if (equippedSpellCount > 0) // don't increment the value if no spells are equipped
            newValues.magic = Mathf.Min(Mathf.RoundToInt(newValues.magic + (100 - newValues.magic) * errorTolerance), 100); // must not exceed 100
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

    // Called when the player gives up the race from the pause menu
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
        // Sum of weights of all the bonuses - from RaceController.Instance.level
        totalBonusWeightSum = 0;
        foreach (var bonusSpot in RaceController.Instance.Level.bonuses) {
            if (bonusSpot.isEmpty) continue;
            totalBonusWeightSum += bonusSpot.bonusInstances[0].bonusWeight;
        }
        // Obstacle collisions - value between 0 and 100 representing how well the player evades collisions
        obstacleCollisionValue = Mathf.Clamp(1 - (obstacleCollisionCount * collisionPenalizationBasedOnTrackLength.Evaluate(trackLength)), 0, 1) * 100;
        // Total amount of mana which can be picked up (assuming the player picks up only one instance in each bonus spot)
        totalMana = 0;
        foreach (var bonusSpot in RaceController.Instance.Level.bonuses) {
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
        // Weight of the new stat value when cimbining it with the old one
        float middle = (totalRacers - 1) / 2f + 1;
        statWeightBasedOnPlace = Mathf.FloorToInt(Mathf.Abs(middle - playerPlace) + 1); // e.g. 3 for 1st place among 5-6 racers
    }

    private int ComputeEnduranceValue() {
        // Change the current Endurance value based on place
        float delta = enduranceDeltaBasedOnPlace.Evaluate(1 - ((playerPlace - 1f) / (totalRacers - 1f))) * maxEnduranceDelta;
        return Mathf.Min(Mathf.RoundToInt(currentEndurance + delta), 100); // must not exceed 100
    }

    private int ComputeSpeedValue() {
        return Mathf.RoundToInt(Mathf.Clamp((float)(currentSpeedSum / maxSpeedSum), 0, 1) * 100); // Clamp in case the player has maximum speed broom upgrade and picks up speed bonuses
    }

    private int ComputeDexterityValue() {
        // Combination of distance from the 'ideal' trajectory and number of collisions with obstacles
        float distancePart = (float)(1 - (currentDistancePenalizationSum / maxDistancePenalizationSum)) * 100;
        float collisionPart = obstacleCollisionValue;
        return Mathf.RoundToInt(distancePart * 0.4f + collisionPart * 0.6f);
    }

    private int ComputePrecisionValue() {
        // Combination of passed/missed hoops, picked/missed bonuses, obstacle collisions and wrong directions
        float hoopPart = (passedHoops / (float)totalHoops) * 100;
        float bonusPart = Mathf.Clamp(pickedUpBonusWeightSum / (float)totalBonusWeightSum, 0, 1) * 100; // Clamp in case the player picks up more bonuses at the same bonus spot
        float collisionPart = obstacleCollisionValue;
        float wrongDirectionPart = Mathf.Clamp(1 - (wrongDirectionCount * wrongDirectionPenalizationBasedOnTrackLength.Evaluate(trackLength)), 0, 1) * 100;
        return Mathf.RoundToInt(hoopPart * 0.35f + bonusPart * 0.25f + collisionPart * 0.25f + wrongDirectionPart * 0.15f);
    }

    private int ComputeMagicValue() {
        // Combination of picked up mana bonuses and diverse spell usage
        if (PlayerState.Instance.availableSpellCount == 0) // no purchased spells, just return 0
            return 0;
        float manaPart = ((Mathf.Clamp(pickedUpMana / (float)totalMana, 0, 1) * 2 + Mathf.Clamp(usedMana / (float)totalMana, 0, 1)) / 3) * 100; // Clamp in case the player picks up more bonuses at the same bonus spot
        return Mathf.RoundToInt(manaPart * equippedSpellUsageValue * spellUsageValue);
    }

    private int CombineEnduranceValues(int oldValue, int newValue) {
        // Only the new value may be taken in this case
        return newValue;
    }

    private int CombineSpeedValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        // Weight of current value depends on place
        return Mathf.RoundToInt((oldValue + newValue * statWeightBasedOnPlace) / (float)(statWeightBasedOnPlace + 1));
    }

    private int CombineDexterityValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        // Weight of current value depends on place
        // Old value has more weight so the stat does not immediately jump to very high values
        return Mathf.RoundToInt((oldValue * (statWeightBasedOnPlace + 2) + newValue * statWeightBasedOnPlace) / (float)(2 * statWeightBasedOnPlace + 2));
    }

    private int CombinePrecisionValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        // Weight of current value depends on place
        // Old value has more weight so the stat does not immediately jump to very high values
        return Mathf.RoundToInt((oldValue * (statWeightBasedOnPlace + 2) + newValue * statWeightBasedOnPlace) / (float)(2 * statWeightBasedOnPlace + 2));
    }

    private int CombineMagicValues(int oldValue, int newValue) {
        // Weighted average of the old and new stat value
        // Weight of current values depends on place
        return Mathf.RoundToInt((oldValue + newValue * statWeightBasedOnPlace) / (float)(statWeightBasedOnPlace + 1));
    }

    private void Update() {
        if (isComputing) {
            // Update stats intermediate results
            // --- sample speed in regular intervals
            speedSampleCountdown -= Time.deltaTime;
            if (speedSampleCountdown < 0) {
                speedSampleCountdown += speedSamplingInterval;
                currentSpeedSum += Mathf.Max(playerRepresentation.characterController.GetCurrentSpeed() - speedLowerBound, 0); // subtracting so the speed from broom upgrade has larger weight
                maxSpeedSum += Mathf.Max(CharacterMovementController.MAX_SPEED - speedLowerBound, 0);
            }
            // --- sample distance from "ideal" trajectory between hoops in regular intervals
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

    private float GetPlayerDistanceFromTrajectory() {
        // Compute distance between the player and the trajectory
        Vector3 playerPosition = playerRaceState.transform.position;
        // Get positions of two track points between which the player is located
        Vector3 firstPoint = RaceController.Instance.Level.playerStartPosition;
        Vector3 secondPoint = RaceController.Instance.Level.Track[0].position;
        if (playerRaceState.followingTrackPoint >= RaceController.Instance.Level.Track.Count) { // the player is behind the last hoop
            firstPoint = RaceController.Instance.Level.Track[RaceController.Instance.Level.Track.Count - 1].position;
            secondPoint = RaceController.Instance.Level.finish.transform.position.WithY(firstPoint.y); // finish line but at the same height as the last hoop
        } else if (playerRaceState.followingTrackPoint > 0) { // the player is somewhere in the middle of the track
            firstPoint = RaceController.Instance.Level.Track[playerRaceState.followingTrackPoint - 1].position;
            secondPoint = RaceController.Instance.Level.Track[playerRaceState.followingTrackPoint].position;
        }
        // Project vector from the first point to the player onto the vector from the first point to the second point
        Vector3 projection = Vector3.Project(playerPosition - firstPoint, (secondPoint - firstPoint).normalized);
        // Distance between the first vector and the projection should be the desired distance
        float distance = Vector3.Distance(playerPosition, firstPoint + projection);
        return distance;
    }

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
}
