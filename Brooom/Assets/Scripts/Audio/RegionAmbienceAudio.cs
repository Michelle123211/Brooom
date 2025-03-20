using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionAmbienceAudio : MonoBehaviour {

	[SerializeField] FMODUnity.StudioEventEmitter ambienceSound;
	[SerializeField] FMODUnity.StudioEventEmitter underwaterSnapshot;

	protected LevelRegionType LastRegion { get; private set; } = LevelRegionType.NONE;

	private bool underwater = false;

	protected virtual Vector3 GetCurrentPlayerPosition() {
		if (!RaceController.Instance.IsInitialized) return Vector3.zero;
		return RaceController.Instance.playerRacer.characterController.transform.position;
	}

	// Returns LevelRegionType.NONE if there is no need for change
	protected virtual LevelRegionType GetCurrentRegion(Vector3 playerPosition) {
		if (!RaceController.Instance.IsInitialized) return LevelRegionType.NONE;
		TerrainPoint nearestTerrainPoint = RaceController.Instance.Level.GetNearestTerrainPoint(playerPosition);
		LevelRegionType region = nearestTerrainPoint.region;
		if (region != LastRegion) {
			return region;
		} else {
			return LevelRegionType.NONE;
		}
	}

	protected virtual float GetCurrentAltitude(Vector3 playerPosition) {
		if (!RaceController.Instance.IsInitialized) return 0f;
		TerrainPoint nearestTerrainPoint = RaceController.Instance.Level.GetNearestTerrainPoint(playerPosition);
		float altitudeAboveGround = playerPosition.y - Mathf.Max(0, nearestTerrainPoint.position.y); // above terrain or water level, whichever is higher
		return altitudeAboveGround; // would not work for more complicated terrain (e.g. tunnel)
	}

	private float GetFMODRegionParameterValue(LevelRegionType region) {
		return region switch {
			LevelRegionType.NONE => 0,
			LevelRegionType.AboveWater => 1,
			LevelRegionType.EnchantedForest => 2,
			LevelRegionType.AridDesert => 3,
			LevelRegionType.SnowyMountain => 4,
			LevelRegionType.BloomingMeadow => 5,
			LevelRegionType.StormyArea => 6,
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
