using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A bonus which displayes a line going through the next few significant track points (hoops/checkpoints) for easier navigation.
/// </summary>
public class NavigationBonusEffect : BonusEffect {

	[Tooltip("To how many track points (hoops/checkpoints) ahead the trajectory is highlighted.")]
	public int numberOfPoints = 2;
	[Tooltip("A prefab of an object leaving trail behind which is used as a visual effect for highlighting a trajectory.")]
	public HighlightTrajectory trajectoryHighlighterPrefab;

	/// <inheritdoc/>
	public override void ApplyBonusEffect(CharacterMovementController character) {
		// This bonus is visual, therefore it works for the player only
		if (!character.CompareTag("Player"))
			return;
		
		// Instantiate an object travelling between the track points and leaving trail behind
		HighlightTrajectory highlighter = Instantiate<HighlightTrajectory>(trajectoryHighlighterPrefab, character.transform.position, Quaternion.identity);
		CharacterRaceState raceState = character.GetComponent<CharacterRaceState>();
		int nextHoopIndex = raceState.trackPointToPassNext;
		for (int i = 0; i < numberOfPoints; i++) {
			int hoopIndex = nextHoopIndex + i;
			if (hoopIndex >= RaceControllerBase.Instance.Level.Track.Count) break;
			highlighter.AddTrajectoryPoint(RaceControllerBase.Instance.Level.Track[hoopIndex].position);
		}
		highlighter.Play();
	}

	/// <inheritdoc/>
	public override bool IsAvailable() {
		// Not available in Testing Track or Tutorial - there is no actual track to highlight
		if (SceneLoader.Instance.CurrentScene == Scene.TestingTrack || SceneLoader.Instance.CurrentScene == Scene.Tutorial) return false;
		else return true; // otherwise, always available
	}
}
