using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


/// <summary>
/// A class representing a progress in race of a particular racer.
/// It stores information such as which hoops were passed or missed, which hoop is the next one, what is the place and finish time (including penalization).
/// It also provides option for registering callbacks on several different events (e.g., on place changed, on hoop missed).
/// </summary>
public class CharacterRaceState : MonoBehaviour {
    // Assigned color on the minimap
    /// <summary>Color assigned to the racer which is used for minimap icon and is also displayed in race results.</summary>
    [HideInInspector] public Color assignedColor;

    // Hoops/checkpoints
    /// <summary>Number of checkpoints passed.</summary>
    [HideInInspector] public int checkpointsPassed = 0;
    /// <summary>Number of hoops passed (excluding checkpoints).</summary>
    [HideInInspector] public int hoopsPassed = 0;
    /// <summary>Number of hoops missed.</summary>
    [HideInInspector] public int hoopsMissed = 0;
    /// <summary>Each value indicates whether the hoop on that index has been passed (<c>true</c>), or not.</summary>
    [HideInInspector] public bool[] hoopsPassedArray;

    // Position within the race
    /// <summary>Index of the track point in front of which tha racer is located (indicating racer's position within the track).</summary>
    [HideInInspector] public int followingTrackPoint = 0;
    /// <summary>Index of the track point which is to be passed next (the racer should fly through it next).</summary>
    [HideInInspector] public int trackPointToPassNext = 0;
    /// <summary>Whether the racer is currently flying in wrong direction (away from the track point to be passed next).</summary>
    [HideInInspector] public bool isWrongDirection = true;
    /// <summary>Whether the racer has already finished the race.</summary>
    public bool HasFinished { get => this.finishTime > 0; }

    // Place
    /// <summary>The racer's place in race (updated also during the race, but then time penalization for missed hoops is not considered).</summary>
    [HideInInspector] public int place;

    // Time and penalization
    /// <summary>Time penalization (in seconds) for missed hoops.</summary>
    [HideInInspector] public float timePenalization = 0;
    /// <summary>Finish time (in seconds) of the racer (or -1 if the racer hasn't finished yet). It doesn't include time penalization.</summary>
    [HideInInspector] public float finishTime = -1;
    /// <summary>The time (in seconds) from the start of the race in which last hoop has been passed or missed (or -1 if none has been missed/passed yet).</summary>
    [HideInInspector] public float lastHoopTime = -1;

    // Callbacks on state change (used e.g. for HUD update)
    /// <summary>Called when the racer's place changes (also during the race). Parameter is the new place.</summary>
    public event Action<int> onPlaceChanged;
    /// <summary>Called when the number of checkpoints passed so far changes. Parameter is current number of checkpoints passed so far.</summary>
    public event Action<int> onPassedCheckpointsChanged;
    /// <summary>Called when the number of hoops passed so far changes. Parameter is current number of hoops passed so far.</summary>
    public event Action<int> onPassedHoopsChanged;
    /// <summary>Called when the number of hoops missed so far changes. Parameter is current number of hoops missed so far.</summary>
    public event Action<int> onMissedHoopsChanged;
    /// <summary>Called when the racer's time penalization changes. Parameter is the new penalization value.</summary>
    public event Action<int> onTimePenalizationChanged;

    // Callbacks on progress within the track (used e.g. for visual feedback)
    /// <summary>Called when the racer advanced to the next hoop/checkpoint (regardless of whether the last one was passed or missed). Could be used e.g. to highlight the next one.</summary>
    public event Action onHoopAdvance;
    /// <summary>Called when the racer missed a checkpoint. Could be used e.g. to make screen red.</summary>
    public event Action onCheckpointMissed;
    /// <summary>Called when the racer missed a hoop.</summary>
    public event Action onHoopMissed;
    /// <summary>Called when the racer started or stopped flying in wrong direction. Parameter is whether the direction is now wrong.</summary>
    public event Action<bool> onWrongDirectionChanged; // parameter: new value of the direction wrongness

    private bool isPlayer = false;
    private bool raceStarted = false;

    /// <summary>
    /// Initializes the race state to initial values.
    /// </summary>
    /// <param name="numOfHoops">Total number of hoops on the track (including checkpoints).</param>
    public void Initialize(int numOfHoops) {
        hoopsPassedArray = new bool[numOfHoops];
    }

    /// <summary>
    /// Marks the race as started (so that race state can start being updated), or not started (so that the race state stops being updated).
    /// </summary>
    /// <param name="raceStarted">Whether the race has started, or not (i.e. has finished).</param>
    public void SetRaceStarted(bool raceStarted) {
        this.raceStarted = raceStarted;
    }

    /// <summary>
    /// Updates the race state if the hoop/checkpoint to be passed next was passed and calls corresponding callbacks.
    /// </summary>
    /// <param name="hoopIndex">Index of the hoop/checkpoint which was passed.</param>
    public void OnHoopPassed(int hoopIndex) {
        // Make sure only the next (highlighted) hoop is considered
        if (hoopIndex == trackPointToPassNext) {
            lastHoopTime = RaceControllerBase.Instance.raceTime;
            hoopsPassedArray[hoopIndex] = true;
            if (RaceControllerBase.Instance.Level.Track[hoopIndex].isCheckpoint) {
                if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.CheckpointPassed);
                checkpointsPassed++;
                onPassedCheckpointsChanged?.Invoke(checkpointsPassed);
            } else {
                if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.HoopPassed);
                hoopsPassed++;
                onPassedHoopsChanged?.Invoke(hoopsPassed);
            }
            AdvanceToNextHoop(true);
        }
    }

    /// <summary>
    /// Gets the current time from the <c>RaceControllerBase</c> implementation and stores it as finish time.
    /// Also notifies <c>RaceControllerBase</c> implementation that the racer has finished (and in case of player tells it that the race has ended).
    /// </summary>
    public void OnFinishPassed() {
        // Get the current time from the RaceController and store it
        if (!HasFinished && trackPointToPassNext >= hoopsPassedArray.Length) { // only the first time and only when the player did not miss any checkpoint
            finishTime = RaceControllerBase.Instance.raceTime;
            // Player ends the race
            if (isPlayer) RaceControllerBase.Instance.EndRace();
            else RaceControllerBase.Instance.OnRacerFinished(this);
        }
    }

    private int lastCheckpointMissed = -1; // index of the last checkpoint which was missed
    /// <summary>
    /// Displays a warning on the screen and plays a sound effect, if the checkpoint was missed for the first time.
    /// </summary>
    /// <param name="hoopIndex">Index of the checkpoint which was missed.</param>
    public void OnCheckpointMissed(int hoopIndex) {
        if (hoopIndex != lastCheckpointMissed) {  // Should be warned only once for the same checkpoint
            lastCheckpointMissed = hoopIndex;
            if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.CheckpointMissed);
            onCheckpointMissed?.Invoke();
            // Checkpoint cannot be missed, so no progress is made
        }
    }

    /// <summary>
    /// Notes down that a hoop has been missed, invokes corresponding callbacks, updates time penalization and advances to the next hoop.
    /// </summary>
    /// <param name="hoopIndex">Index of the hoop which was missed.</param>
    public void OnHoopMissed(int hoopIndex) {
        lastHoopTime = RaceControllerBase.Instance.raceTime;
        hoopsMissed++;
        if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.HoopMissed);
        onMissedHoopsChanged?.Invoke(hoopsMissed);
        onHoopMissed?.Invoke();
        AddTimePenalization(RaceControllerBase.Instance.missedHoopPenalization);
        // Hoops can be missed, the player still makes a progress
        AdvanceToNextHoop(false);
    }

    /// <summary>
    /// Stores the new place value and invokes corresponding callbacks if the value changed.
    /// </summary>
    /// <param name="place">The new place value.</param>
    public void UpdatePlace(int place) {
        bool valueChanged = this.place != place;
        this.place = place;
        if (valueChanged) onPlaceChanged?.Invoke(place);
    }

    /// <summary>
    /// Adds given time penalization and invokes corresponding callbacks.
    /// </summary>
    /// <param name="penalization">Time penalization (in seconds) to be added.</param>
    public void AddTimePenalization(float penalization) {
        float currentPenalization = timePenalization + penalization;
        onTimePenalizationChanged?.Invoke(Mathf.RoundToInt(currentPenalization));
        // Tween the penalization so it is added gradually (reflected also in the HUD)
        DOTween.To(() => timePenalization, x => timePenalization = x, currentPenalization, 0.5f);
    }

    /// <summary>
    /// Updates the race state to move on to the next hoop, also invokes corresponding callbacks.
    /// </summary>
    /// <param name="hoopWasPassed"></param>
    private void AdvanceToNextHoop(bool hoopWasPassed) {
        trackPointToPassNext++;
        onHoopAdvance?.Invoke();
        if (isPlayer) Messaging.SendMessage("HoopAdvance", hoopWasPassed);
    }

    /// <summary>
    /// Detects missing a checkpoint and a hoop and updates everything accordingly.
    /// </summary>
    private void UpdateHoops() {
        List<TrackPoint> trackPoints = RaceControllerBase.Instance.Level.Track;
        if (trackPointToPassNext < trackPoints.Count) {
            // Detect missing a checkpoint/hoop
            HoopRelativePosition relativePosition = GetHoopRelativePosition(trackPoints[trackPointToPassNext]);
            if (relativePosition == HoopRelativePosition.After) { // The character got after the next hoop
                if (trackPoints[trackPointToPassNext].isCheckpoint) {
                    OnCheckpointMissed(trackPointToPassNext);
                } else {
                    OnHoopMissed(trackPointToPassNext);
                }
            }
        }
    }
    /// <summary>Different options of racer's position relative to a hoop.</summary>
    private enum HoopRelativePosition {
        Before,
        At,
        After
    }
    /// <summary>
    /// Checks whether the races is in the space in front of or behind the hoop with the given index.
    /// </summary>
    /// <param name="nextHoopPoint">Index of the hoop to check.</param>
    /// <returns>Racer's position relative to the hoop.</returns>
    private HoopRelativePosition GetHoopRelativePosition(TrackPoint nextHoopPoint) {
        Vector3 dividingVector = nextHoopPoint.assignedHoop.transform.right.WithY(0); // vector dividing space into two parts (before/after the hoop)
        Vector3 playerVector = transform.position.WithY(0) - nextHoopPoint.position.WithY(0); // vector from the hoop to the player
        float angle = Vector3.SignedAngle(playerVector, dividingVector, Vector3.up); // angle between the two vectors
        if (angle < 0) return HoopRelativePosition.Before;
        if (angle > 0) return HoopRelativePosition.After;
        else return HoopRelativePosition.At;
    }

    /// <summary>
    /// Updates racer's position within race relatively to the hoops.
    /// It does so by finding out which hoop is the next one on the track (but not necessarily the next one to be passed).
    /// </summary>
    private void UpdatePositionWithinRace() {
        List<TrackPoint> trackPoints = RaceControllerBase.Instance.Level.Track;
        // Find between which hoops the player is located
        // ... whether he is after the following hoop
        if (followingTrackPoint < trackPoints.Count &&
                GetHoopRelativePosition(trackPoints[followingTrackPoint]) == HoopRelativePosition.After) {
            followingTrackPoint += 1;
        }
        // ... or whether he returned before the previous hoop
        else if (followingTrackPoint - 1 >= 0 &&
                GetHoopRelativePosition(trackPoints[followingTrackPoint - 1]) == HoopRelativePosition.Before) {
            followingTrackPoint -= 1;
        }
    }

    /// <summary>
    /// Detects whether the racer is flying in the opposite direction than they should (based on which hoop whould be passed next).
    /// </summary>
    private void UpdateDirection() {
        List<TrackPoint> trackPoints = RaceControllerBase.Instance.Level.Track;

        // If the racer has completed the track there is nothing more to be done
        if (trackPointToPassNext >= trackPoints.Count)
            return;

        // The racer is now between the nextPoint and previousPoint
        int nextPoint = followingTrackPoint;
        int previousPoint = followingTrackPoint - 1;

        // Determine whether the racer needs to fly to the higher or lower index
        bool needsToGoForward = !(trackPointToPassNext <= previousPoint);

        // Get direction from one hoop to the other
        Vector3 direction;
        // ... resolve corner cases first
        if (previousPoint < 0) // it is always forward to the first hoop
            direction = Vector3.forward;
        else if (nextPoint >= trackPoints.Count) // directly between the last hoop and the player
            direction = transform.position.WithY(0) - trackPoints[previousPoint].position.WithY(0);
        // ... then the standard case
        else
            direction = trackPoints[nextPoint].position.WithY(0) - trackPoints[previousPoint].position.WithY(0);

        // Reverse if necessary
        if (!needsToGoForward) direction *= -1;

        // Compare with the player's current direction
        float angle = Vector3.Angle(direction, transform.forward.WithY(0));

        // Handle direction change
        if ((angle > 100 && !isWrongDirection) || (angle < 80 && isWrongDirection)) {
            isWrongDirection = !isWrongDirection;
            onWrongDirectionChanged?.Invoke(isWrongDirection);
        }
    }

    private void Awake() {
        isPlayer = gameObject.CompareTag("Player");
	}

    // Updates values which needs to be checked regularly
	private void Update() {
        if (raceStarted) {
            UpdateHoops();
            UpdatePositionWithinRace();
            UpdateDirection();
        }
	}
}
