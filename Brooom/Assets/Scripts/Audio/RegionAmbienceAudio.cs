using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionAmbienceAudio : MonoBehaviour {

	[SerializeField] FMODUnity.StudioEventEmitter ambienceSound;
	[SerializeField] FMODUnity.StudioEventEmitter underwaterSnapshot;

	private LevelRegionType lastRegion = LevelRegionType.NONE;

	private bool underwater = false;

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
		// Set Region parameter - only if it changed
		Vector3 playerPosition = RaceController.Instance.playerRacer.characterController.transform.position;
		TerrainPoint nearestTerrainPoint = RaceController.Instance.level.GetNearestGridPoint(playerPosition);
		LevelRegionType region = nearestTerrainPoint.region;
		if (region != lastRegion) {
			lastRegion = region;
			ambienceSound.SetParameter("Region", GetFMODRegionParameterValue(region));
		}

		// Set Altitude parameter
		float altitudeAboveGround = playerPosition.y - Mathf.Max(0, nearestTerrainPoint.position.y); // above terrain or water level, whichever is higher
		ambienceSound.SetParameter("Altitude", altitudeAboveGround); // would not work for more complicated terrain (e.g. tunnel)

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
