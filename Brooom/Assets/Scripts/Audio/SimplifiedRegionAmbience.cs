using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simplified component responsible for controlling ambience audio based on the region the player is currently located in and the altitude above ground.
/// It is used in predefined levels, e.g., Testing Track and Tutorial.
/// The region is not actually detected, but instead it is changed externally from trigger zones.
/// Altitude is only approximation because in predefined levels we don't have access to terrain information.
/// </summary>
public class SimplifiedRegionAmbience : RegionAmbienceAudio {

	[SerializeField] CharacterMovementController player;

	/// <inheritdoc/>
	protected override Vector3 GetCurrentPlayerPosition() {
		return player.transform.position;
	}

	/// <inheritdoc/>
	protected override LevelRegionType GetCurrentRegion(Vector3 playerPosition) {
		return LevelRegionType.NONE; // region is changed externally from trigger zones
	}

	/// <inheritdoc/>
	protected override float GetCurrentAltitude(Vector3 playerPosition) {
		return player.transform.position.y; // approximation only because we don't have info about the terrain so we cannot get exact altitude above ground
	}

}
