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

    private double currentSpeedSum, maxSpeedSum; // Speed
    private float speedSampleCountdown; // Speed

    private double currentDistancePenalizationSum, maxDistancePenalizationSum; // Dexterity
    private float distanceSampleCountdown; // Dexterity

    private int obstacleCollisionCount; // Dexterity, Precision
    private float obstacleCollisionValue; // Dexterity, Precision

    private float currentEndurance, currentSpeed;
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


    // Called from RaceController at the start of the race
    public void StartComputingStats() {
        isComputing = true;

        // Initialize everything necessary
        playerRepresentation = RaceController.Instance.playerRacer;
        playerRaceState = playerRepresentation.state;
        playerSpellController = playerRaceState.GetComponent<SpellController>();

        totalRacers = RaceController.Instance.racers.Count;

        currentSpeedSum = 0; maxSpeedSum = 0;
        speedSampleCountdown = speedSamplingInterval;

        currentDistancePenalizationSum = 0; maxDistancePenalizationSum = 0;
        distanceSampleCountdown = distanceSampleInterval;

        obstacleCollisionCount = 0;
        Messaging.RegisterForMessage("ObstacleCollision", OnObstacleCollision);

        currentEndurance = PlayerState.Instance.CurrentStats.endurance;
        currentSpeed = PlayerState.Instance.CurrentStats.speed;
        trackLength = ((currentEndurance * 2 + currentSpeed) / 3) / 100;

        pickedUpBonusWeightSum = 0;
        Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);

        wrongDirectionCount = 0;
        playerRaceState.onWrongDirectionChanged += OnWrongDirectionChanged;

        pickedUpMana = 0; usedMana = 0;
        playerSpellController.onManaAmountChanged += OnManaAmountChanged;

        spellUsed = new bool[playerSpellController.spellSlots.Length];
        spellUsedCount = 0;
        equippedSpellCount = 0;
        foreach (var spell in playerSpellController.spellSlots)
            if (spell != null && !spell.IsEmpty()) equippedSpellCount++;
        playerSpellController.onSpellCasted += OnSpellCasted;
}

    // Called from RaceController at the end of the race
    public void StopComputingAndUpdateStats() {
        isComputing = false;
        // Unregister callbacks
        Messaging.UnregisterFromMessage("ObstacleCollision", OnObstacleCollision);
        Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
        playerRaceState.onWrongDirectionChanged -= OnWrongDirectionChanged;
        playerSpellController.onManaAmountChanged -= OnManaAmountChanged;
        playerSpellController.onSpellCasted -= OnSpellCasted;
        // Finalize stats values
        CompleteComputationParameters();
        PlayerStats newValues = new PlayerStats {
            endurance = ComputeEnduranceValue(),
            speed = ComputeSpeedValue(),
            dexterity = ComputeDexterityValue(),
            precision = ComputePrecisionValue(),
            magic = ComputeMagicValue()
        };
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
        foreach (var bonusSpot in RaceController.Instance.level.bonuses) {
            if (bonusSpot.isEmpty) continue;
            totalBonusWeightSum += bonusSpot.assignedBonus.bonusWeight;
        }
        // Obstacle collisions - value between 0 and 100 representing how well the player evades collisions
        obstacleCollisionValue = Mathf.Clamp(1 - (obstacleCollisionCount * collisionPenalizationBasedOnTrackLength.Evaluate(trackLength)), 0, 1) * 100;
        // Total amount of mana which can be picked up (assuming the player picks up only one instance in each bonus spot)
        totalMana = 0;
        foreach (var bonusSpot in RaceController.Instance.level.bonuses) {
            if (bonusSpot.isEmpty) continue;
            if (bonusSpot.assignedBonus.TryGetComponent<ManaBonusEffect>(out ManaBonusEffect manaBonus))
                totalMana += manaBonus.manaAmount;
        }
        // Spell usage
        equippedSpellUsageValue = notUsedSpellPenalization.Evaluate(spellUsedCount / (float)equippedSpellCount); // number between 0 and 1 describing how diverse spells the player has casted during the race
        totalSpellUsedCount = 0; // TODO: how many of the total spells the player has ever used
        totalSpellCount = 1; // TODO: how many spells are available in the game
        spellUsageValue = (totalSpellUsedCount / (float)totalSpellCount); // number between 0 and 1 describing how diverse spells the player has ever casted
    }

    private int ComputeEnduranceValue() {
        // Change the current Endurance value based on place
        float delta = enduranceDeltaBasedOnPlace.Evaluate(1 - ((playerPlace - 1) / (totalRacers - 1))) * maxEnduranceDelta;
        return Mathf.RoundToInt(currentEndurance + delta);
    }

    private int ComputeSpeedValue() {
        // TODO
        // currentSpeedSum / maxSpeedSum
        return 0;
    }

    private int ComputeDexterityValue() {
        // TODO
        // (1 - (currentDistancePenalizationSum / maxDistancePenalizationSum)) * 100 * 0.4f
        // obstacleCollisionValue * 0.6f
        return 0;
    }

    private int ComputePrecisionValue() {
        // TODO
        // (passedHoops / totalHoops) * 100 * 0.35f
        // Mathf.Clamp(pickedUpBonusWeightSum / totalBonusWeightSum, 0, 1) * 100 * 0.25f // Clamp in case the player picks up more bonuses at the same bonus spot
        // obstacleCollisionValue * 0.25f
        // Mathf.Clamp(1 - (wrongDirectionCount * wrongDirectionPenalizationBasedOnTrackLength.Evaluate(trackLength)), 0, 1) * 100 * 0.15f
        return 0;
    }

    private int ComputeMagicValue() {
        // TODO
        // (Mathf.Clamp(pickedUpMana / totalMana, 0, 1) * 2 + Mathf.Clamp(usedMana / totalMana, 0, 1)) / 3 // Clamp in case the player picks up more bonuses at the same bonus spot
        // * equippedSpellUsageValue
        // * spellUsageValue
        return 0;
    }

    private int CombineEnduranceValues(int oldValue, int newValue) {
        // TODO
        // Weighted average of the old and new stat value
        // Weight of current values depends on: place, stat category
        // Maybe only the new value may be taken instead
        return (oldValue + newValue) / 2;
    }

    private int CombineSpeedValues(int oldValue, int newValue) {
        // TODO
        // Weighted average of the old and new stat value
        // Weight of current values depends on: place, stat category
        return (oldValue + newValue) / 2;
    }

    private int CombineDexterityValues(int oldValue, int newValue) {
        // TODO
        // Weighted average of the old and new stat value
        // Weight of current values depends on: place, stat category
        // Old value has more weight
        return (oldValue + newValue) / 2;
    }

    private int CombinePrecisionValues(int oldValue, int newValue) {
        // TODO
        // Weighted average of the old and new stat value
        // Weight of current values depends on: place, stat category
        // Old value has more weight
        return (oldValue + newValue) / 2;
    }

    private int CombineMagicValues(int oldValue, int newValue) {
        // TODO
        // Weighted average of the old and new stat value
        // Weight of current values depends on: place, stat category
        return (oldValue + newValue) / 2;
    }

    private void Update() {
        if (isComputing) {
            // Update stats intermediate results
            // --- sample speed in regular intervals
            speedSampleCountdown -= Time.deltaTime;
            if (speedSampleCountdown < 0) {
                speedSampleCountdown += speedSamplingInterval;
                currentSpeedSum += playerRepresentation.characterController.GetCurrentSpeed();
                maxSpeedSum += CharacterMovementController.MAX_SPEED;
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
        Vector3 firstPoint = RaceController.Instance.level.playerStartPosition;
        Vector3 secondPoint = RaceController.Instance.level.track[0].position;
        if (playerRaceState.followingTrackPoint >= RaceController.Instance.level.track.Count) { // the player is behind the last hoop
            firstPoint = RaceController.Instance.level.track[RaceController.Instance.level.track.Count - 1].position;
            secondPoint = RaceController.Instance.level.finish.transform.position.WithY(firstPoint.y); // finish line but at the same height as the last hoop
        } else if (playerRaceState.followingTrackPoint > 0) { // the player is somewhere in the middle of the track
            firstPoint = RaceController.Instance.level.track[playerRaceState.followingTrackPoint - 1].position;
            secondPoint = RaceController.Instance.level.track[playerRaceState.followingTrackPoint].position;
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

    private void OnSpellCasted(int index) {
        if (spellUsed[index]) return;
        // The spell has not been casted yet during this race
        spellUsedCount++;
        spellUsed[index] = true;
    }
}
