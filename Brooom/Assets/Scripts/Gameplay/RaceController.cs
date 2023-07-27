using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RaceController : MonoBehaviour {
    [Header("Level length (Endurance)")]
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 0.")]
    public int initialNumberOfCheckpoints = 4;
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 100.")]
    public int finalNumberOfCheckpoints = 20;

    [Header("Direction change (Dexterity)")]
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 0.")]
    public Vector2 initialDirectionChange = new Vector2(10, 20);
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 100.")]
    public Vector2 finalDirectionChange = new Vector2(30, 45);

    [Header("Hoop scale (Precision)")]
    [Tooltip("Scale of hoops when the player's Precision stat is 0.")]
    public float initialHoopScale = 1f;
    [Tooltip("Scale of hoops when the player's Precision stat is 100.")]
    public float finalHoopScale = 0.4f;

    [Header("Hoop distance (Speed)")]
    [Tooltip("The approximate minimum and maximum distance between two hoops when the player's Speed stat is 0.")]
    public Vector2 initialHoopDistanceRange = new Vector2(40, 50);
    [Tooltip("The approximate minimum and maximum distance between two hoops when the player's Speed stat is 100.")]
    public Vector2 finalHoopDistanceRange = new Vector2(80, 100);

    [Header("Regions")]
    public List<LevelRegionType> defaultRegions;
    public List<RegionUnlockValue> regionsUnlockedByEndurance;
    public List<RegionUnlockValue> regionsUnlockedByAltitude;


    // Related objects
    private RaceUI raceHUD;
    private LevelGenerationPipeline levelGenerator;
    private TrackPointsGenerationRandomWalk trackGenerator;
    private TrackHoopsPlacement hoopsPlacement;
    private MaximumAngleCorrection angleCorrection;
    private PlayerController player;

    private bool raceStarted = false; // distinguish between training and race
    private float raceTime = 0;
    private int checkpointsPassed = 0;
    private int hoopsPassed = 0;

    void Start()
    {
        raceHUD = FindObjectOfType<RaceUI>();
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
        trackGenerator = FindObjectOfType<TrackPointsGenerationRandomWalk>();
        hoopsPlacement = FindObjectOfType<TrackHoopsPlacement>();
        angleCorrection = FindObjectOfType<MaximumAngleCorrection>();
        player = FindObjectOfType<PlayerController>();
        // Initialize state at the beginning
        PlayerState.Instance.raceState.Reset();
        // Generate level (terrain + track)
        SetLevelGeneratorParameters();
        PlayerState.Instance.raceState.level = levelGenerator.GenerateLevel();
        // Place the player
        player.ResetPosition(PlayerState.Instance.raceState.level.playerStartPosition);
        // Initialize HUD
        int checkpointsTotal = 0, hoopsTotal = 0;
        foreach (var trackPoint in PlayerState.Instance.raceState.level.track) {
            if (trackPoint.isCheckpoint) checkpointsTotal++;
            else hoopsTotal++;
        }
        raceHUD.InitializeCheckpointsAndHoops(checkpointsTotal, hoopsTotal);
    }

	private void Update() {
        // TODO: DEBUG only, remove
        if (!raceStarted && Input.GetMouseButtonDown(1)) {
            StartRace();
        }

        PlayerState.Instance.raceState.UpdateRaceState();

        // Update player's position relatively to the hoops
        // - check whether the player is after the next hoop
        int nextHoopIndex = PlayerState.Instance.raceState.previousTrackPointIndex + 1;
        if (nextHoopIndex < PlayerState.Instance.raceState.level.track.Count) {
            HoopRelativePosition relativePosition = GetHoopRelativePosition(nextHoopIndex);
            if (relativePosition == HoopRelativePosition.After) {
                PlayerState.Instance.raceState.previousTrackPointIndex = nextHoopIndex;
                // TODO: Remove after debugging (now even missed checkpoints/hoops are counted)
                if (PlayerState.Instance.raceState.level.track[nextHoopIndex].isCheckpoint)
                    checkpointsPassed++;
                else
                    hoopsPassed++;
                // TODO: Higlight the next hoop
            }
        }
        // - check whether the player is before the previous hoop
        int previousHoopIndex = PlayerState.Instance.raceState.previousTrackPointIndex;
        if (previousHoopIndex >= 0) {
            HoopRelativePosition relativePosition = GetHoopRelativePosition(previousHoopIndex);
            if (relativePosition == HoopRelativePosition.Before) {
                PlayerState.Instance.raceState.previousTrackPointIndex = previousHoopIndex - 1;
                // TODO: Remove after debugging (now even missed checkpoints/hoops are counted)
                if (PlayerState.Instance.raceState.level.track[previousHoopIndex].isCheckpoint)
                    checkpointsPassed--;
                else
                    hoopsPassed--;
            }
        }
        // - otherwise the player is still between the same pair of hoops
        PlayerState.Instance.raceState.UpdatePlayerPositionWithinRace(checkpointsPassed, hoopsPassed);

        if (raceStarted) { // during race
            // TODO: Update player's place
            // Compare player's previous hoop with other racers, then compare distance to the next hoop
        } else { // during training
            if (InputManager.Instance.GetBoolValue("Restart")) {
                player.ResetPosition(PlayerState.Instance.raceState.level.playerStartPosition);
                PlayerState.Instance.raceState.previousTrackPointIndex = -1;
            }
        }

        // Update UI
        if (raceHUD != null) {
            raceHUD.UpdatePlayerState(player.GetCurrentSpeed(), player.GetCurrentAltitude());
            // TODO: Change to the actual time from the start of the race (not including training)
            raceTime += Time.deltaTime;
            raceHUD.UpdateTime(raceTime);
        }
    }

    // TODO: Call when entering the race
    private void StartRace() {
        // TODO: Add everything related to the start of the race
        raceStarted = true;
        PlayerState.Instance.raceState.StartRace();
        raceHUD.StartRace();
    }

    private void SetLevelGeneratorParameters() {
        // Compute parameters based on player's stats
        // ... number of checkpoints from Endurance
        int numOfCheckpoints = Mathf.RoundToInt(Mathf.Lerp(initialNumberOfCheckpoints, finalNumberOfCheckpoints, PlayerState.Instance.CurrentStats.endurance / 100f));
        // ... maximum direction change from Dexterity
        Vector2 directionChange = Vector2.Lerp(initialDirectionChange, finalDirectionChange, PlayerState.Instance.CurrentStats.dexterity / 100f);
        // ... hoop scale from Precision
        float hoopScale = Mathf.Lerp(initialHoopScale, finalHoopScale, PlayerState.Instance.CurrentStats.precision / 100f);
        // ... distance between adjacent hoops from Speed
        Vector2 distanceRange = Vector2.Lerp(initialHoopDistanceRange, finalHoopDistanceRange, PlayerState.Instance.CurrentStats.speed / 100f);
        // ... available regions from Endurance
        foreach (var region in defaultRegions)
            PlayerState.Instance.raceState.regionsAvailability[region] = true;
        foreach (var regionWithValue in regionsUnlockedByEndurance)
            PlayerState.Instance.raceState.SetRegionAvailability(
                regionWithValue.region, 
                PlayerState.Instance.CurrentStats.endurance >= regionWithValue.minValue ? true : false
            );
        // ... available regions from max altitude
        foreach (var regionWithValue in regionsUnlockedByAltitude)
            PlayerState.Instance.raceState.SetRegionAvailability(
                regionWithValue.region,
                PlayerState.Instance.maxAltitude >= regionWithValue.minValue ? true : false
            );
        // And set them
        levelGenerator.regionsAvailability = PlayerState.Instance.raceState.regionsAvailability;
        if (trackGenerator != null) {
            trackGenerator.numberOfCheckpoints = numOfCheckpoints;
            trackGenerator.maxDirectionChangeAngle = directionChange;
            trackGenerator.distanceRange = distanceRange;
        }
        if (hoopsPlacement != null)
            hoopsPlacement.hoopScale = hoopScale;
        if (angleCorrection != null)
            angleCorrection.maxAngle = directionChange.x;
    }

    private enum HoopRelativePosition { 
        Before,
        At,
        After
    }
    // Checks whether the player is in the space before or after the hoop with the given index
    private HoopRelativePosition GetHoopRelativePosition(int hoopIndex) {
        TrackPoint nextHoopPoint = PlayerState.Instance.raceState.level.track[hoopIndex];
        Vector3 dividingVector = nextHoopPoint.assignedObject.transform.right.WithY(0); // vector dividing space into two parts (before/after the hoop)
        Vector3 playerVector = player.transform.position.WithY(0) - nextHoopPoint.position.WithY(0); // vector from the hoop to the player
        float angle = Vector3.SignedAngle(playerVector, dividingVector, Vector3.up); // angle between the two vectors
        if (angle < 0) return HoopRelativePosition.Before;
        if (angle > 0) return HoopRelativePosition.After;
        else return HoopRelativePosition.At;
    }
}

[System.Serializable]
public class RegionUnlockValue {
    [Tooltip("Region unlocked by a specific value of some stat (e.g. max altitude, endurance).")]
    public LevelRegionType region;
    [Tooltip("If the stat is greater then this value, the region becomes available.")]
    public int minValue;
}


// A class representing an effect (from spell or bonus) affecting the player
public class PlayerEffect {
    public Sprite Icon { get; private set; }
    public float TimeLeft { get; private set; }
    public Action onEffectStart;
    public Action onEffectEnd;

    public PlayerEffect(Sprite icon, float duration) {
        this.Icon = icon;
        this.TimeLeft = duration;
    }

    public void OverrideDuration(float newDuration) {
        TimeLeft = newDuration;
    }

    public void Update(float deltaTime) {
        TimeLeft -= deltaTime;
    }

    public bool IsFinished() => TimeLeft < 0;

	// Equality override based solely on icon
	// Icon specifies effect type and there cannot be two effects of the same type affecting the player at the same time
	public override bool Equals(object obj) {
        if (obj == null) return false;

        PlayerEffect other = obj as PlayerEffect;
        if ((object)other == null)  return false;

        return Icon == other.Icon;
	}
    public bool Equals(PlayerEffect other) {
        if ((object)other == null) return false;

        return Icon == other.Icon;
    }
	public override int GetHashCode() {
        return Icon.GetHashCode();
	}
    public static bool operator ==(PlayerEffect a, PlayerEffect b) {
        if (ReferenceEquals(a, b)) return true;
        if ((object)a == null || (object)b == null) return false;
        return a.Icon == b.Icon;
    }
    public static bool operator !=(PlayerEffect a, PlayerEffect b) {
        return !(a == b);
    }
}