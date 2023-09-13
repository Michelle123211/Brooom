using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Displays a line/curve going through the next 2 significant points (hoops/checkpoints)
public class NavigationBonusEffect : BonusEffect {

	[Tooltip("To how many track points (hoops/checkpoints) ahead the trajectory is highlighted.")]
	public int numberOfPoints = 2;

	public HighlightTrajectory trajectoryHighlighterPrefab;

	public override void ApplyBonusEffect(CharacterMovementController character) {
		// Instantiate an object travelling between the track points and leaving trail behind
		HighlightTrajectory highlighter = Instantiate<HighlightTrajectory>(trajectoryHighlighterPrefab, transform.position, Quaternion.identity);
		int nextHoopIndex = PlayerState.Instance.raceState.trackPointToPassNext;
		for (int i = 0; i < numberOfPoints; i++) {
			int hoopIndex = nextHoopIndex + i;
			if (hoopIndex >= PlayerState.Instance.raceState.level.track.Count) break;
			highlighter.AddTrajectoryPoint(PlayerState.Instance.raceState.level.track[hoopIndex].position);
		}
		highlighter.Play();
	}

	public override bool IsAvailable() {
		return true; // always available
	}
}
