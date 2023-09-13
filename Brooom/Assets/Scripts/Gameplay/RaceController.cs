using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

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

    [Header("Penalizations")]
    [Tooltip("How many seconds are added to the time when player misses a hoop.")]
    public int missedHoopPenalization = 5;

    [Header("Regions")]
    public List<LevelRegionType> defaultRegions;
    public List<RegionUnlockValue> regionsUnlockedByEndurance;
    public List<RegionUnlockValue> regionsUnlockedByAltitude;


    // Related objects
    private RaceUI raceHUD;
    private LevelGenerationPipeline levelGenerator;
    private Transform bonusParent;
    private TrackPointsGenerationRandomWalk trackGenerator;
    private TrackObjectsPlacement hoopsPlacement;
    private MaximumAngleCorrection angleCorrection;
    private List<CharacterMovementController> characters;
    private int playerIndex = 0;


    private bool raceStarted = false; // distinguish between training and race

    private float raceTime = 0;
    private float timePenalization = 0; // added to the raceTime

    private int checkpointsPassed = 0;
    private int hoopsPassed = 0;
    private int hoopsMissed = 0;


    // Called when entering the race
    public void StartRace() {
        // TODO: Add everything related to the start of the race
        raceStarted = true;
        PlayerState.Instance.raceState.StartRace();
        raceHUD.StartRace(); // also resets e.g. hoop progress and mana
        // Show bonuses
        if (bonusParent != null) bonusParent.gameObject.SetActive(true);
        // Activate the hoops
        foreach (var hoop in PlayerState.Instance.raceState.level.track) {
            hoop.assignedHoop.Activate();
        }
        // Highlight the first hoop
        PlayerState.Instance.raceState.level.track[0].assignedHoop.StartHighlighting();
        // Place the player
        characters[playerIndex].ResetPosition(PlayerState.Instance.raceState.level.playerStartPosition);
        // TODO: Disable player actions
        // TODO: Place the opponents (with disabled actions, random appearance (character + broom), minimap icon color)
        // TODO: Start animation sequence
        // TODO: At the end of the sequence, show the race countdown
        // TODO: At the end of the countdown, enable player actions and enable opponents actions
    }

    private void StartTraining() {
        // Hide bonuses
        bonusParent = levelGenerator.transform.Find("Bonus");
        if (bonusParent != null) bonusParent.gameObject.SetActive(false);
        // Place the player
        characters[playerIndex].ResetPosition(PlayerState.Instance.raceState.level.playerStartPosition);
        // Enable player actions
        characters[playerIndex].ActionsEnabled = true;
        // Get reference to CharacterMovementComponent, if isPlayer == true, it is the player, otherwise it is the opponent
    }

    void Start()
    {
        raceHUD = FindObjectOfType<RaceUI>();
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
        trackGenerator = FindObjectOfType<TrackPointsGenerationRandomWalk>();
        hoopsPlacement = FindObjectOfType<TrackObjectsPlacement>();
        angleCorrection = FindObjectOfType<MaximumAngleCorrection>();
        // Initialize state at the beginning
        PlayerState.Instance.raceState.ResetAll();
        // Generate level (terrain + track)
        SetLevelGeneratorParameters();
        PlayerState.Instance.raceState.level = levelGenerator.GenerateLevel();
        // Get references to the characters
        characters = Utils.FindObject<CharacterMovementController>();
        for (int i = 0; i < characters.Count; i++)
            if (characters[i].isPlayer) playerIndex = i;
        // Initialize HUD
        int checkpointsTotal = 0, hoopsTotal = 0;
        foreach (var trackPoint in PlayerState.Instance.raceState.level.track) {
            if (trackPoint.isCheckpoint) checkpointsTotal++;
            else hoopsTotal++;
        }
        raceHUD.InitializeCheckpointsAndHoops(checkpointsTotal, hoopsTotal);
        // Initialize training
        StartTraining();
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

    private void Update() {
        if (raceStarted) { // during race
            PlayerState.Instance.raceState.UpdateRaceState();
            UpdateHoops();
            UpdatePlayerPositionAndDirection();
            DetectWrongDirection();
            // TODO: Update player's place
            // Compare player's next hoop with other racers, then compare distance to the next hoop
        } else { // during training
            if (InputManager.Instance.GetBoolValue("Restart")) {
                characters[playerIndex].ResetPosition(PlayerState.Instance.raceState.level.playerStartPosition);
                PlayerState.Instance.raceState.trackPointToPassNext = 0;
            }
        }

        // Update UI
        if (raceHUD != null) {
            raceHUD.UpdatePlayerState(characters[playerIndex].GetCurrentSpeed(), characters[playerIndex].GetCurrentAltitude());
            // TODO: Change to the actual time from the start of the race (not including training)
            raceTime += Time.deltaTime;
            raceHUD.UpdateTime(raceTime + timePenalization);
        }
    }

    private enum HoopRelativePosition { 
        Before,
        At,
        After
    }
    // Checks whether the player is in the space before or after the hoop with the given index
    private HoopRelativePosition GetHoopRelativePosition(int hoopIndex) {
        TrackPoint nextHoopPoint = PlayerState.Instance.raceState.level.track[hoopIndex];
        Vector3 dividingVector = nextHoopPoint.assignedHoop.transform.right.WithY(0); // vector dividing space into two parts (before/after the hoop)
        Vector3 playerVector = characters[playerIndex].transform.position.WithY(0) - nextHoopPoint.position.WithY(0); // vector from the hoop to the player
        float angle = Vector3.SignedAngle(playerVector, dividingVector, Vector3.up); // angle between the two vectors
        if (angle < 0) return HoopRelativePosition.Before;
        if (angle > 0) return HoopRelativePosition.After;
        else return HoopRelativePosition.At;
    }

    // Updates passed checkpoints and hoops
    // Highlights the next one
    private void UpdateHoops() {
        int nextHoopIndex = PlayerState.Instance.raceState.trackPointToPassNext;
        if (nextHoopIndex < PlayerState.Instance.raceState.level.track.Count) {
            HoopRelativePosition relativePosition = GetHoopRelativePosition(nextHoopIndex);
            if (relativePosition == HoopRelativePosition.After) { // The player got after the next hoop
                bool shouldHighlightNext = false;
                if (PlayerState.Instance.raceState.level.track[nextHoopIndex].assignedHoop.playerDetected) { // Player went through and did not miss
                    // Update hoop/checkpoint count
                    if (PlayerState.Instance.raceState.level.track[nextHoopIndex].isCheckpoint) {
                        checkpointsPassed++;
                    } else {
                        hoopsPassed++;
                    }
                    shouldHighlightNext = true;
                    // Notify anyone interested that a hoop has been passed
                    Messaging.SendMessage("HoopAdvance", true);
                } else { // Player missed
                    if (PlayerState.Instance.raceState.level.track[nextHoopIndex].isCheckpoint) {
                        // Checkpoint cannot be missed - it stays highlighted
                        ReactOnCheckpointMissed();
                    } else {
                        // Hoops can be missed
                        ReactOnHoopMissed();
                        shouldHighlightNext = true;
                        // Notify anyone interested that a hoop has been missed
                        Messaging.SendMessage("HoopAdvance", false);
                    }
                }
                // Highlight the next hoop
                if (shouldHighlightNext) {
                    PlayerState.Instance.raceState.trackPointToPassNext = nextHoopIndex + 1;
                    PlayerState.Instance.raceState.level.track[nextHoopIndex].assignedHoop.StopHighlighting();
                    if (nextHoopIndex + 1 < PlayerState.Instance.raceState.level.track.Count)
                        PlayerState.Instance.raceState.level.track[nextHoopIndex + 1].assignedHoop.StartHighlighting();
                }
                PlayerState.Instance.raceState.UpdatePlayerPositionWithinRace(checkpointsPassed, hoopsPassed, hoopsMissed);
            }
        }
    }

    private int lastCheckpointMissed = -1;
    private void ReactOnCheckpointMissed() {
        if (PlayerState.Instance.raceState.trackPointToPassNext != lastCheckpointMissed) { // Player should be warned only once for the same checkpoint
            lastCheckpointMissed = PlayerState.Instance.raceState.trackPointToPassNext;
            // Make the screen red briefly
            raceHUD.FlashScreenColor(Color.red);
            // Warn the player that they must return to the checkpoint
            raceHUD.ShowMissedCheckpointWarning();
        }
    }

    private void ReactOnHoopMissed() {
        // Update the missed hoops counter and add penalization
        hoopsMissed++;
        AddTimePenalization(missedHoopPenalization);
        // Make the screen red briefly
        raceHUD.FlashScreenColor(Color.red);

    }

    private void AddTimePenalization(float penalization) {
        float currentPenalization = timePenalization + penalization;
        raceHUD.UpdateTimePenalization(Mathf.RoundToInt(currentPenalization));
        DOTween.To(() => timePenalization, x => timePenalization = x, currentPenalization, 0.5f);
    }

    // Updates player's position relatively to the hoops
    private void UpdatePlayerPositionAndDirection() {
        // Find between which hoops the player is located
        // ... whether he is after the following hoop
        if (PlayerState.Instance.raceState.followingTrackPoint < PlayerState.Instance.raceState.level.track.Count &&
            GetHoopRelativePosition(PlayerState.Instance.raceState.followingTrackPoint) == HoopRelativePosition.After) {
            PlayerState.Instance.raceState.followingTrackPoint += 1;
        }
        // ... or whether he returned before the previous hoop
        else if (PlayerState.Instance.raceState.followingTrackPoint - 1 >= 0 &&
                GetHoopRelativePosition(PlayerState.Instance.raceState.followingTrackPoint - 1) == HoopRelativePosition.Before) {
            PlayerState.Instance.raceState.followingTrackPoint -= 1;
        }
    }

    // Detects if player is flying in the opposite direction
    private void DetectWrongDirection() {
        // If the player has completed the track there is nothing more to be done
        if (PlayerState.Instance.raceState.trackPointToPassNext >= PlayerState.Instance.raceState.level.track.Count)
            return;

        // The player is now between the nextPoint and previousPoint
        int nextPoint = PlayerState.Instance.raceState.followingTrackPoint;
        int previousPoint = PlayerState.Instance.raceState.followingTrackPoint - 1;

        // Determine whether the player needs to fly to the higher or lower index
        // ... if they does not need to go to the lower index (so some corner cases are interpreted as going forward, without warning)
        bool needsToGoForward = !(PlayerState.Instance.raceState.trackPointToPassNext <= previousPoint);

        // Get direction from one hoop to the other
        Vector3 direction;
        // ... resolve corner cases first
        if (previousPoint < 0) // it is always forward to the first hoop
            direction = Vector3.forward;
        else if (nextPoint >= PlayerState.Instance.raceState.level.track.Count) // directly between the last hoop and the player
            direction = characters[playerIndex].transform.position.WithY(0) - PlayerState.Instance.raceState.level.track[previousPoint].position.WithY(0);
        // ... then the standard case
        else
            direction = PlayerState.Instance.raceState.level.track[nextPoint].position.WithY(0) - PlayerState.Instance.raceState.level.track[previousPoint].position.WithY(0);

        // Reverse if necessary
        if (!needsToGoForward) direction *= -1;

        // Compare with the player's current direction
        float angle = Vector3.Angle(direction, characters[playerIndex].transform.forward.WithY(0));
        // Show warning if necessary
        if (angle > 100) raceHUD.ShowWrongDirectionWarning();
        // Hide warning if necessary
        else if (angle < 80) raceHUD.HideWringDirectionWarning();
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