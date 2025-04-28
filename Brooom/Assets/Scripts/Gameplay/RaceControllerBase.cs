using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// This abstract class provides a base for any class handling race progress, i.e. making sure the race is started and finished, race results are displayed etc.
/// It provides useful methods and also the simplest form of race control, which can be further extended in derived classes by overriding several methods.
/// </summary>
public abstract class RaceControllerBase : MonoBehaviourLongInitialization {

    // Simple singleton
    /// <summary>Instance of <c>RaceControllerBase</c> or any derived class in the current scene (simple singleton implementation).</summary>
    public static RaceControllerBase Instance;

    [Header("Penalizations")]
    [Tooltip("How many seconds are added to the time when player misses a hoop.")]
    public int missedHoopPenalization = 10;

    /// <summary>Current state of the race (to distinguish between training and race).</summary>
    public RaceState State { get; protected set; } = RaceState.BeforeRace;

    /// <summary>Object representation of the level, to get access to track points and record racers' position within the track.</summary>
    public LevelRepresentation Level { get; private set; } = null;
    /// <summary>Number between 0 and 1 denoting how difficult the track is (computed based on stats values which were used to generate it).</summary>
    protected float levelDifficulty; // number between 0 and 1

    /// <summary>Current time elapsed in the race.</summary>
    [HideInInspector] public float raceTime = 0;

    /// <summary>A list of all racers, including the player.</summary>
    public List<RacerRepresentation> racers;
    /// <summary>References to important components of the player racer.</summary>
    public RacerRepresentation playerRacer;

    // Related objects
    /// <summary>Manages all UI elements which are part of HUD.</summary>
    protected RaceUI raceHUD;
    /// <summary>Responsible for displaying race results UI.</summary>
    protected RaceResultsUI raceResults;
    /// <summary>Generates level based on some parameters.</summary>
    protected LevelGenerationPipeline levelGenerator;
    /// <summary>Parent object of all bonuses (it is used to hide all bonuses during training and show them during race).</summary>
    protected Transform bonusParent;
    /// <summary>Parent object of all opponents (it is used to hide all opponents during training and show them during race).</summary>
    protected Transform opponentParent;

    /// <summary>
    /// Initializes everything necessary and starts playing the race start cutscene.
    /// </summary>
    public virtual void StartRace() {
        // Start animation sequence
        StartCoroutine(PlayRaceStartSequence());
    }

    /// <summary>
    /// Finalizes everything and starts playing the race end cutscene which finishes with displaying race results.
    /// </summary>
    public virtual void EndRace() {
        GamePause.DisableGamePause();
        // Start animation sequence
        StartCoroutine(PlayRaceEndSequence());
    }

    /// <summary>
    /// This method is called when the player gives the race up (from an option in Pause Menu).
    /// </summary>
    public virtual void GiveUpRace() {
        // Send message
        Messaging.SendMessage("RaceGivenUp");
        OnRaceGivenUp();
    }

    /// <summary>
    /// Disables actions of the racer who has finished the race and plays an animation.
    /// </summary>
    /// <param name="racerState"><c>CharacterRaceState</c> of a racer who has just finished the race.</param>
    public void OnRacerFinished(CharacterRaceState racerState) {
        foreach (var racer in racers) {
            if (racer.state == racerState) {
                racer.characterController.DisableActions(CharacterMovementController.StopMethod.BrakeStop);
                racer.characterController.gameObject.GetComponent<FaceAnimationsController>()?.StartSmiling();
                // TODO: Start waving animation
                racer.spellInput.DisableSpellCasting();
                break;
            }
        }
    }

    /// <summary>
    /// Determines each racer's place and updates it in their <c>CharacterRaceState</c> component.
    /// </summary>
    protected void ComputeRacerPlaces() {
        // Sort the racers according to their place
        racers.Sort((x, y) => CompareRacers(x, y));
        // Let them know what their place is
        for (int i = 0; i < racers.Count; i++) {
            racers[i].state.UpdatePlace(i + 1);
        }
    }

    /// <summary>
    /// Computes race results (finish time, penalization) for opponents which haven't finished before the race ended because the player finished it.
    /// Finish time and number of missed hoops are predicted based on the values so far,
    /// while ensuring any finish time predicted this way is higher than the player's one.
    /// </summary>
    protected void CompleteOpponentState() {
        // Handle case when the opponent did not finish
        foreach (var racer in racers) {
            if (!racer.state.HasFinished) {
                // Compute the finish time proportionaly to the number of remaining hoops
                float timeForOneHoop = raceTime;
                if (racer.state.trackPointToPassNext > 0)
                    timeForOneHoop = racer.state.lastHoopTime / racer.state.trackPointToPassNext;
                racer.state.finishTime = timeForOneHoop * Level.Track.Count;
                // Compute time penalization for likely missed hoops
                if (racer.state.trackPointToPassNext > 0) {
                    float missedHoopsPercentage = racer.state.hoopsMissed / racer.state.trackPointToPassNext;
                    float missedHoopsInFuture = Mathf.RoundToInt((Level.Track.Count - racer.state.trackPointToPassNext) * missedHoopsPercentage);
                    racer.state.timePenalization += (missedHoopsInFuture * missedHoopPenalization);
                }
                // If the final time is lower than the player's one, increase it
                float timeDifference = (playerRacer.state.finishTime + playerRacer.state.timePenalization) - (racer.state.finishTime + racer.state.timePenalization);
                if (timeDifference > 0) {
                    racer.state.finishTime += (timeDifference + UnityEngine.Random.Range(5f, 15f));
                }
            }
        }
    }

    /// <summary>
    /// Collects race results from all racers and displays them on the screen.
    /// </summary>
    /// <param name="coinRewards">An array of coins reward for each place.</param>
    protected void ShowRaceResults(int[] coinRewards) {
        // Collect results from individual racers
        RaceResultData[] results = new RaceResultData[racers.Count];
        foreach (var racer in racers) {
            float time = racer.state.finishTime + racer.state.timePenalization;
            // If the total time is too big, make them DNF instead
            if (time > (playerRacer.state.finishTime + playerRacer.state.timePenalization) * 6)
                time = -1;
            results[racer.state.place - 1] = new RaceResultData {
                name = racer.characterName,
                color = racer.state.assignedColor,
                time = time,
                timePenalization = racer.state.timePenalization,
                coinsReward = time > 0 ? coinRewards[racer.state.place - 1] : 0
            };
            // TODO: Set coinsPenalization
        }
        raceResults.SetResultsTable(results, playerRacer.state.place);
        // Display everything
        raceResults.gameObject.TweenAwareEnable();
    }

    /// <summary>
    /// Compares two racers according to who should be placed higher.
    /// Handles places after the race has ended (comparing finish times) and also during the race (comparing racer's positions within the race).
    /// </summary>
    /// <param name="x">First racer to compare.</param>
    /// <param name="y">Second racer to compare.</param>
    /// <returns>Negative number if <c>x</c> should be first, positive if <c>y</c> should be first.</returns>
    protected int CompareRacers(RacerRepresentation x, RacerRepresentation y) {
        // If both have finish time, the better one should be first
        if (x.state.HasFinished && y.state.HasFinished)
            return (x.state.finishTime + x.state.timePenalization).CompareTo(y.state.finishTime + y.state.timePenalization);
        // The one having finish time should be first
        if (x.state.HasFinished && !y.state.HasFinished) return -1; // x is first
        if (!x.state.HasFinished && y.state.HasFinished) return 1; // y is first
        // The one having next hoop with higher index should be first
        if (x.state.trackPointToPassNext != y.state.trackPointToPassNext)
            return y.state.trackPointToPassNext.CompareTo(x.state.trackPointToPassNext);
        // The one closer to the hoop or finish should be first
        float xDistance, yDistance;
        if (x.state.trackPointToPassNext >= Level.Track.Count) { // the finish line is next
            // Get distance on the shortest line to the finish
            xDistance = Mathf.Abs(Level.finish.transform.InverseTransformPoint(x.state.transform.position).z);
            yDistance = Mathf.Abs(Level.finish.transform.InverseTransformPoint(y.state.transform.position).z);
        } else {
            // Get distance to the next hoop
            xDistance = Vector3.Distance(x.state.transform.position, Level.Track[x.state.trackPointToPassNext].position);
            yDistance = Vector3.Distance(y.state.transform.position, Level.Track[y.state.trackPointToPassNext].position);
        }
        return xDistance.CompareTo(yDistance);
    }

    /// <summary>
    /// Computes track difficulty as a weighted average of stats values which were used to generate it.
    /// </summary>
    /// <returns>Number between 0 and 1 denoting how difficult the track is.</returns>
    protected float ComputeLevelDifficulty() {
        // Weighted average of current player stats which were used for level generation
        // - weight 3: dexterity // the most important
        // - weight 3: endurance // to make the difficulty value increase reasonably over time
        // - weight 2: precision
        // - weight 1: speed // combined with endurance represents length of the track, but is not so important
        float weightedAverage = (
            3 * (PlayerState.Instance.CurrentStats.dexterity + PlayerState.Instance.CurrentStats.endurance) +
            2 * (PlayerState.Instance.CurrentStats.precision) +
            1 * (PlayerState.Instance.CurrentStats.speed))
            / 9f;
        // Mapped from (0, 100) to (0, 1)
        return weightedAverage / 100;
    }

    #region Long initialization

    /// <inheritdoc/>
    protected override void PrepareForInitialization_ReplacingAwake() {
        Instance = this;
    }

    /// <inheritdoc/>
    protected override void PrepareForInitialization_ReplacingStart() {
        raceHUD = FindObjectOfType<RaceUI>();
        raceResults = UtilsMonoBehaviour.FindObject<RaceResultsUI>();
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
        levelGenerator.onLevelGenerated += OnLevelGenerated;
        InitializeAnythingRelated();
    }

    /// <inheritdoc/>
    protected override IEnumerator InitializeAfterPreparation() {
        yield return new WaitUntil(() => Level != null); // wait until level is generated
        InitializeRacers();
        // Initialize HUD
        int checkpointsTotal = 0, hoopsTotal = 0;
        foreach (var trackPoint in Level.Track) {
            if (trackPoint.isCheckpoint) checkpointsTotal++;
            else hoopsTotal++;
        }
        raceHUD.InitializeCheckpointsAndHoops(checkpointsTotal, hoopsTotal);
        // Register callbacks on player race state changes
        playerRacer.state.onHoopAdvance += HighlightNextHoop;
        playerRacer.state.onCheckpointMissed += ReactOnCheckpointMissed;
        playerRacer.state.onHoopMissed += ReactOnHoopMissed;

        AfterInitialization();
    }

    /// <summary>
    /// This method replaces <c>Update()</c> method from which it is in fact called (after ensuring object initialization has already finished).
    /// It updates everything necessary (together with HUD) based on the current race state.
    /// </summary>
    protected override void UpdateAfterInitialization() {
        // Update state
        switch (State) {
            case RaceState.BeforeRace:
                Update_BeforeRace();
                break;
            case RaceState.Training:
                Update_Training();
                break;
            case RaceState.RaceInProgress:
                Update_RaceInProgress();
                break;
            case RaceState.RaceFinished:
                Update_RaceFinished();
                break;
        }
        // Update UI
        raceHUD.UpdatePlayerState(
            playerRacer.characterController.GetCurrentSpeed(),
            playerRacer.characterController.GetCurrentAltitude());
    }

    #endregion

    // Initializes a list of all racers (with their most important components) and a reference to the player racer
    private void InitializeRacers() {
        // Get references to the characters
        List<CharacterMovementController> characters = UtilsMonoBehaviour.FindObjects<CharacterMovementController>();
        racers = new List<RacerRepresentation>();
        for (int i = 0; i < characters.Count; i++) {
            RacerRepresentation racer = new RacerRepresentation {
                characterName = characters[i].GetComponentInChildren<CharacterAppearance>().characterName,
                characterController = characters[i],
                state = characters[i].GetComponent<CharacterRaceState>(),
                spellInput = characters[i].GetComponentInChildren<SpellInput>()
            };
            racer.state.Initialize(Level.Track.Count);
            racers.Add(racer);
            if (racer.characterController.isPlayer) playerRacer = racer;
            racer.characterController.DisableActions();
            racer.spellInput.DisableSpellCasting();
        }
    }

    // Computes the level's difficulty after it has been generated (it is registered as a callback on level generated)
    private void OnLevelGenerated(LevelRepresentation level) {
        this.Level = level;
        levelGenerator.onLevelGenerated -= OnLevelGenerated;
        levelDifficulty = ComputeLevelDifficulty();
        Analytics.Instance.LogEvent(AnalyticsCategory.Race, $"Track's difficulty is {levelDifficulty}.");
    }

    // Does everything necessary before the race starts - prepares everything, starts cutscene, displays countdown, starts the race and enables racer's actions
    private IEnumerator PlayRaceStartSequence() {
        BeforeRaceStartSequence(); // point of possible functionality extension

        double remainingDuration = 0;
        // Start playing the sequence
        PlayableDirector startCutscene = Cutscenes.Instance.PlayCutscene("RaceStart");
        if (startCutscene != null) {
            remainingDuration = startCutscene.duration;
            yield return new WaitForSeconds(0.18f); // allow the screen to fade out first
            remainingDuration -= startCutscene.time;
        }
        // Place the player + disable actions
        playerRacer.characterController.ResetPosition(Level.playerStartPosition);
        playerRacer.characterController.GetComponent<PlayerCameraController>().ResetCameras();
        playerRacer.characterController.DisableActions();
        // Prepare HUD
        raceTime = 0;
        raceHUD.UpdateTime(0);
        raceHUD.StartRace(); // also resets e.g. hoop progress and mana
        // Show bonuses
        if (bonusParent != null) bonusParent.gameObject.SetActive(true);
        // Activate the hoops and finish line
        for (int i = 0; i < Level.Track.Count; i++) {
            Level.Track[i].assignedHoop.Activate(i);
        }
        Level.finish.Activate();
        // Highlight the first hoop
        Level.Track[0].assignedHoop.StartHighlighting();
        // Show the opponents
        if (opponentParent != null) opponentParent.gameObject.SetActive(true);
        // Wait until the end of the sequence
        yield return new WaitForSeconds((float)remainingDuration);
        // Reset camera (front camera, default rotation)
        playerRacer.characterController.GetComponent<PlayerCameraController>().TweenToResetView();
        // Show the race countdown
        for (int i = 3; i > 0; i--) {
            raceHUD.ShowCountdown(i.ToString());
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.CountdownRace);
            yield return new WaitForSeconds(1);
        }
        raceHUD.ShowCountdown("START!");
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.RaceStarted);
        // Send message
        Messaging.SendMessage("RaceStarted", racers.Count);
        // Enable player and enable opponents actions
        foreach (var racer in racers) {
            racer.characterController.EnableActions();
            racer.state.SetRaceStarted(true);
            racer.spellInput.TryEnableSpellCasting();
        }
        State = RaceState.RaceInProgress;

        AfterRaceStartSequence(); // point of possible functionality extension
    }

    // Does everything necessary after the race ended - disables player's, plays cutscene, computes race results and displays them
    private IEnumerator PlayRaceEndSequence() {
        BeforeRaceEndSequence(); // point of possible functionality extension

        State = RaceState.RaceFinished;
        // Disable player actions to make them brake
        playerRacer.characterController.DisableActions(CharacterMovementController.StopMethod.BrakeStop);
        playerRacer.spellInput.DisableSpellCasting();
        // Start playing the sequence
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.Game.RaceFinished);
        PlayableDirector endCutscene = Cutscenes.Instance.PlayCutscene("RaceEnd");
        double remainingDuration = 0;
        if (endCutscene != null) {
            remainingDuration = endCutscene.duration;
        }
        // Wait until the end of the sequence
        yield return new WaitForSeconds((float)remainingDuration);
        // Recompute racers' places
        CompleteOpponentState();
        ComputeRacerPlaces();
        int[] coinRewards = ComputeCoinRewards();
        // Update player's coins account - TODO: consider coins penalization (e.g. for exposing magic)
        int playerReward = coinRewards[playerRacer.state.place - 1];
        if (playerReward > 0) {
            Analytics.Instance.LogEvent(AnalyticsCategory.Race, $"Reward for {playerRacer.state.place} place is {playerReward} coins.");
            PlayerState.Instance.ChangeCoinsAmount(playerReward);
        }
        // Send message
        Messaging.SendMessage("RaceFinished", playerRacer.state.place);
        // Show the race results
        ShowRaceResults(coinRewards);

        AfterRaceEndSequence(); // point of possible functionality extension
    }

    private void OnDestroy() {
        OnDestroy_Derived();
        Instance = null;
        // Unregister callbacks on player race state changes
        playerRacer.state.onHoopAdvance -= HighlightNextHoop;
        playerRacer.state.onCheckpointMissed -= ReactOnCheckpointMissed;
        playerRacer.state.onHoopMissed -= ReactOnHoopMissed;
    }

    // Highlights the next hoop for the player
    private void HighlightNextHoop() {
        int nextHoopIndex = playerRacer.state.trackPointToPassNext;
        Level.Track[nextHoopIndex - 1].assignedHoop.StopHighlighting();
        if (nextHoopIndex < Level.Track.Count)
            Level.Track[nextHoopIndex].assignedHoop.StartHighlighting();
    }

    // Notifies the player that a checkpoint has been missed
    private void ReactOnCheckpointMissed() {
        // Make the screen red briefly
        raceHUD.FlashScreenColor(Color.red);
        // Warn the player that they must return to the checkpoint
        raceHUD.ShowMissedCheckpointWarning();
    }

    // Notifies the player that a hoop has been missed
    private void ReactOnHoopMissed() {
        // Make the screen red briefly
        raceHUD.FlashScreenColor(Color.red);
    }

    #region Virtual methods to extend functionality
    // Different virtual methods which can be overridden by derived classes to add some functionality

    /// <summary>
    /// This method is called before the race start sequence (i.e. preparation of the track, cutscene, countdown and race start) starts.
    /// </summary>
    protected virtual void BeforeRaceStartSequence() { }
    /// <summary>
    /// This method is called after the race start sequence (i.e. preparation of the track, cutscene, countdown and race start) ended.
    /// </summary>
    protected virtual void AfterRaceStartSequence() { }

    /// <summary>
    /// This method is called before the race end sequence (i.e. cutscene, computing and displaying race results) starts.
    /// </summary>
    protected virtual void BeforeRaceEndSequence() { }
    /// <summary>
    /// This method is called after the race end sequence (i.e. cutscene, computing and displaying race results) ended.
    /// </summary>
    protected virtual void AfterRaceEndSequence() { }

    /// <summary>
    /// This method is called from <c>Update()</c> method when the race is in <c>BeforeRace</c> state.
    /// </summary>
    protected virtual void Update_BeforeRace() { }
    /// <summary>
    /// This method is called from <c>Update()</c> method when the race is in <c>Training</c> state.
    /// </summary>
    protected virtual void Update_Training() { }
    /// <summary>
    /// This method is called from <c>Update()</c> method when the race is in <c>RaceInProgress</c> state.
    /// It updates time from the start of the race and also racer's places.
    /// </summary>
    protected virtual void Update_RaceInProgress() {
        // Update time from the start of the race
        raceTime += Time.deltaTime;
        raceHUD.UpdateTime(raceTime + playerRacer.state.timePenalization);
        // Update racers' place
        ComputeRacerPlaces();
    }
    /// <summary>
    /// This method is called from <c>Update()</c> method when the race is in <c>RaceFinished</c> state.
    /// </summary>
    protected virtual void Update_RaceFinished() {
        // Update time from the start of the race
        raceTime += Time.deltaTime;
    }
    #endregion

    #region Abstract methods
    // Abstract methods which need to be implemented (usually to emphasize they replace other common methods like Awake, Start, Update or OnDestroy)

    /// <summary>
    /// This method is called from <c>Start()</c> to initialize any related objects or references.
    /// </summary>
    protected abstract void InitializeAnythingRelated();
    /// <summary>
    /// This method is called after initialization finished completely (considering implementation of <c>MonobehaviourLongInitialization</c>).
    /// </summary>
    protected abstract void AfterInitialization();
    /// <summary>
    /// This method should handle anything which would otherwise be in <c>OnDestroy()</c> (from which this is called).
    /// </summary>
    protected abstract void OnDestroy_Derived();

    /// <summary>
    /// This method is called when the player decides to give up race from pause menu.
    /// </summary>
    protected abstract void OnRaceGivenUp();

    /// <summary>
    /// Prepares a list of coin rewards for each place in race. 
    /// </summary>
    /// <returns>An array of coin rewards for individual places in race.</returns>
    protected abstract int[] ComputeCoinRewards();
	// TODO: Uncomment when coin penalization is added
	//protected abstract int[] ComputeCoinPenalizations();
	#endregion

}

/// <summary>
/// References to all important components which the <c>RaceControllerBase</c> implementation needs for each racer.
/// </summary>
public class RacerRepresentation {
    /// <summary>Name of the racer.</summary>
    public string characterName = string.Empty;
    /// <summary>Component which can be used to enable/disable all movement actions.</summary>
    public CharacterMovementController characterController;
    /// <summary>Component describing current race state of the racer.</summary>
    public CharacterRaceState state;
    /// <summary>Component which can be used to enable/disable spell casting.</summary>
    public SpellInput spellInput;
}


/// <summary>
/// All possible states in which the race could be.
/// </summary>
public enum RaceState {
    BeforeRace,
    Training,
    RaceInProgress,
    RaceFinished
}