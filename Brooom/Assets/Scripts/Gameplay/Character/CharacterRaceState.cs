using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class CharacterRaceState : MonoBehaviour {
    // Assigned color on the minimap
    [HideInInspector] public Color assignedColor;

    // Hoops/checkpoints
    [HideInInspector] public int checkpointsPassed = 0;
    [HideInInspector] public int hoopsPassed = 0;
    [HideInInspector] public int hoopsMissed = 0;
    [HideInInspector] public bool[] hoopsPassedArray;

    // Position within the race
    [HideInInspector] public int followingTrackPoint = 0; // position of the player within the track (they are before the hoop with this index)
    [HideInInspector] public int trackPointToPassNext = 0; // index of the following hoop the player should fly through
    [HideInInspector] public bool isWrongDirection = true;
    public bool HasFinished { get => this.finishTime > 0; }

    // Place
    [HideInInspector] public int place;

    // Time and penalization
    [HideInInspector] public float timePenalization = 0;
    [HideInInspector] public float finishTime = -1;
    [HideInInspector] public float lastHoopTime = -1;

    // Callbacks on state change (used e.g. for HUD update)
    public event Action <int> onPlaceChanged; // parameter: new place
    public event Action <int> onPassedCheckpointsChanged; // parameter: number of passed checkpoints
    public event Action <int> onPassedHoopsChanged; // parameter: number of passed hoops
    public event Action<int> onMissedHoopsChanged; // parameter: number of missed hoops
    public event Action <int> onTimePenalizationChanged; // parameter: new penalization value

    // Callbacks on progress within the track (used e.g. for visual feedback)
    public event Action onHoopAdvance; // used e.g. to highlight the next one
    public event Action onCheckpointMissed; // used e.g. to make screen red
    public event Action onHoopMissed;
    public event Action<bool> onWrongDirectionChanged; // parameter: new value of the direction wrongness

    private bool isPlayer = false;
    private bool raceStarted = false;

    public void Initialize(int numOfHoops) {
        hoopsPassedArray = new bool[numOfHoops];
    }

    public void SetRaceStarted(bool raceStarted) {
        this.raceStarted = raceStarted;
    }

    public void OnHoopPassed(int hoopIndex) {
        // Make sure only the next (highlighted) hoop is considered
        if (hoopIndex == trackPointToPassNext) {
            lastHoopTime = RaceController.Instance.raceTime;
            hoopsPassedArray[hoopIndex] = true;
            if (RaceController.Instance.Level.Track[hoopIndex].isCheckpoint) {
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

    public void OnFinishPassed() {
        // Get the current time from the RaceController and store it
        if (!HasFinished && trackPointToPassNext >= hoopsPassedArray.Length) { // only the first time and only when the player did not miss any checkpoint
            finishTime = RaceController.Instance.raceTime;
            // Player ends the race
            if (isPlayer) RaceController.Instance.EndRace();
            else RaceController.Instance.OnRacerFinished(this);
        }
    }

    private int lastCheckpointMissed = -1;
    public void OnCheckpointMissed(int hoopIndex) {
        if (hoopIndex != lastCheckpointMissed) {  // Should be warned only once for the same checkpoint
            lastCheckpointMissed = hoopIndex;
            if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.CheckpointMissed);
            onCheckpointMissed?.Invoke();
            // Checkpoint cannot be missed, so no progress is made
        }
    }

    public void OnHoopMissed(int hoopIndex) {
        lastHoopTime = RaceController.Instance.raceTime;
        hoopsMissed++;
        if (isPlayer) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.HoopMissed);
        onMissedHoopsChanged?.Invoke(hoopsMissed);
        onHoopMissed?.Invoke();
        AddTimePenalization(RaceController.Instance.missedHoopPenalization);
        // Hoops can be missed, the player still makes a progress
        AdvanceToNextHoop(false);
    }

    public void UpdatePlace(int place) {
        bool valueChanged = this.place != place;
        this.place = place;
        if (valueChanged) onPlaceChanged?.Invoke(place);
    }

    public void AddTimePenalization(float penalization) {
        float currentPenalization = timePenalization + penalization;
        onTimePenalizationChanged?.Invoke(Mathf.RoundToInt(currentPenalization));
        // Tween the penalization so it is added gradually (reflected also in the HUD)
        DOTween.To(() => timePenalization, x => timePenalization = x, currentPenalization, 0.5f);
    }

    private void AdvanceToNextHoop(bool hoopWasPassed) {
        trackPointToPassNext++;
        onHoopAdvance?.Invoke();
        if (isPlayer) Messaging.SendMessage("HoopAdvance", hoopWasPassed);
    }

    // Updates passed checkpoints and hoops
    private void UpdateHoops() {
        List<TrackPoint> trackPoints = RaceController.Instance.Level.Track;
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
    private enum HoopRelativePosition {
        Before,
        At,
        After
    }
    // Checks whether the character is in the space before or after the hoop with the given index
    private HoopRelativePosition GetHoopRelativePosition(TrackPoint nextHoopPoint) {
        Vector3 dividingVector = nextHoopPoint.assignedHoop.transform.right.WithY(0); // vector dividing space into two parts (before/after the hoop)
        Vector3 playerVector = transform.position.WithY(0) - nextHoopPoint.position.WithY(0); // vector from the hoop to the player
        float angle = Vector3.SignedAngle(playerVector, dividingVector, Vector3.up); // angle between the two vectors
        if (angle < 0) return HoopRelativePosition.Before;
        if (angle > 0) return HoopRelativePosition.After;
        else return HoopRelativePosition.At;
    }

    // Updates player's position relatively to the hoops
    private void UpdatePositionWithinRace() {
        List<TrackPoint> trackPoints = RaceController.Instance.Level.Track;
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

    // Detects if player is flying in the opposite direction than they should
    private void UpdateDirection() {
        List<TrackPoint> trackPoints = RaceController.Instance.Level.Track;

        // If the player has completed the track there is nothing more to be done
        if (trackPointToPassNext >= trackPoints.Count)
            return;

        // The player is now between the nextPoint and previousPoint
        int nextPoint = followingTrackPoint;
        int previousPoint = followingTrackPoint - 1;

        // Determine whether the player needs to fly to the higher or lower index
        // ... if they does not need to go to the lower index (so some corner cases are interpreted as going forward, without warning)
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

	private void Update() {
        if (raceStarted) {
            UpdateHoops();
            UpdatePositionWithinRace();
            UpdateDirection();
        }
	}
}
