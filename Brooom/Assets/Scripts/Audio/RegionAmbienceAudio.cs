using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionAmbienceAudio : MonoBehaviour {

	[SerializeField] FMODUnity.StudioEventEmitter ambienceSound;
	[SerializeField] FMODUnity.StudioEventEmitter underwaterSnapshot;

	protected LevelRegionType LastRegion { get; private set; }  = LevelRegionType.NONE;

	private bool underwater = false;

	protected virtual Vector3 GetCurrentPlayerPosition() {
		return RaceController.Instance.playerRacer.characterController.transform.position;
	}

	// Returns LevelRegionType.NONE if there is no need for change
	protected virtual LevelRegionType GetCurrentRegion(Vector3 playerPosition) {
		TerrainPoint nearestTerrainPoint = RaceController.Instance.level.GetNearestGridPoint(playerPosition);
		LevelRegionType region = nearestTerrainPoint.region;
		if (region != LastRegion) {
			return region;
		} else {
			return LevelRegionType.NONE;
		}
	}

	protected virtual float GetCurrentAltitude(Vector3 playerPosition) {
		TerrainPoint nearestTerrainPoint = RaceController.Instance.level.GetNearestGridPoint(playerPosition);
		float altitudeAboveGround = playerPosition.y - Mathf.Max(0, nearestTerrainPoint.position.y); // above terrain or water level, whichever is higher
		return altitudeAboveGround; // would not work for more complicated terrain (e.g. tunnel)
	}

	private float GetFMODRegionParameterValue(LevelRegionType region) {
		return region switch {
			LevelRegionType.AboveWater => 0,
			LevelRegionType.EnchantedForest => 1,
			LevelRegionType.AridDesert => 2,
			LevelRegionType.SnowyMountain => 3,
			LevelRegionType.BloomingMeadow => 4,
			LevelRegionType.StormyArea => 5,
			_ => throw new System.NotImplementedException(),
		};
	}

	private void Update() {
		Vector3 playerPosition = GetCurrentPlayerPosition();

		// Set Region parameter - only if it changed
		LevelRegionType region = GetCurrentRegion(playerPosition);
		if (region != LevelRegionType.NONE) {
			LastRegion = region;
			ambienceSound.SetParameter("Region", GetFMODRegionParameterValue(region));
		}

		// Set Altitude parameter
		ambienceSound.SetParameter("Altitude", GetCurrentAltitude(playerPosition));

		// Handle being underwater
		if (playerPosition.y < 0 && !underwater) { // Play underwater effect
			underwaterSnapshot.Play();
			underwater = true;
		} else if (playerPosition.y > 0 && underwater) { // Stop playing underwater effect
			underwaterSnapshot.Stop();
			underwater = false;
		}
	}
}
