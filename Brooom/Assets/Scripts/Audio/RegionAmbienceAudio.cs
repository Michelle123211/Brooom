using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionAmbienceAudio : MonoBehaviour {

	private FMODUnity.StudioEventEmitter ambienceSound;

	private LevelRegionType lastRegion = LevelRegionType.NONE;

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
		float altitudeAboveGround = playerPosition.y - nearestTerrainPoint.position.y;
		ambienceSound.SetParameter("Altitude", altitudeAboveGround); // would not work for more complicated terrain (e.g. tunnel)
	}

	private void Start() {
		ambienceSound = GetComponent<FMODUnity.StudioEventEmitter>();
	}
}
