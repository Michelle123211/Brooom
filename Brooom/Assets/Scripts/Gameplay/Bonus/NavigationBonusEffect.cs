using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays a line/curve going through the next 2 significant points (hoops/checkpoints)
public class NavigationBonusEffect : BonusEffect {

	[Tooltip("To how many track points (hoops/checkpoints) ahead the trajectory is highlighted.")]
	public int numberOfPoints = 2;

	public HighlightTrajectory trajectoryHighlighterPrefab;

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
			if (hoopIndex >= RaceController.Instance.Level.track.Count) break;
			highlighter.AddTrajectoryPoint(RaceController.Instance.Level.track[hoopIndex].position);
		}
		highlighter.Play();
	}

	public override bool IsAvailable() {
		if (SceneLoader.Instance.CurrentScene == Scene.TestingTrack) return false; // not available in Testing Track
		else return true; // otherwise, always available
	}
}
