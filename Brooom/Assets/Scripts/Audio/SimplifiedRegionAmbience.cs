using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifiedRegionAmbience : RegionAmbienceAudio {

	[SerializeField] CharacterMovementController player;

	protected override Vector3 GetCurrentPlayerPosition() {
		return player.transform.position;
	}

	protected override LevelRegionType GetCurrentRegion(Vector3 playerPosition) {
		return LevelRegionType.NONE; // region is changed externally from trigger zones
	}

	protected override float GetCurrentAltitude(Vector3 playerPosition) {
		return player.transform.position.y; // approximation only because we don't have info about the terrain so we cannot get exact altitude above ground
	}

}
