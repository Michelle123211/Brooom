using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component responsible for controlling ambience audio based on the region the player is currently located in (affecting which sounds are played) 
/// and the altitude above ground (affecting volume of certain sounds).
/// It also handles effect applied to audio when being underwater.
/// </summary>
public class RegionAmbienceAudio : MonoBehaviour {

	[Tooltip("StudioEventEmitter playing ambience audio.")]
	[SerializeField] FMODUnity.StudioEventEmitter ambienceSound;
	[Tooltip("StudioEventEmitter playing Underwater snapshot to alter audio when being underwater.")]
	[SerializeField] FMODUnity.StudioEventEmitter underwaterSnapshot;

	/// <summary>Last region the player has been located in.</summary>
	protected LevelRegionType LastRegion { get; private set; } = LevelRegionType.NONE;

	private bool underwater = false; // if the player is underwater

	/// <summary>
	/// Gets current player's position.
	/// </summary>
	/// <returns>Player's position.</returns>
	protected virtual Vector3 GetCurrentPlayerPosition() {
		if (!RaceControllerBase.Instance.IsInitialized) return Vector3.zero;
		return RaceControllerBase.Instance.playerRacer.characterController.transform.position;
	}

	/// <summary>
	/// Determines if current region the player is located in changed (based on player's position) and returns it.
	/// </summary>
	/// <param name="playerPosition">Current player's position.</param>
	/// <returns>Region the player is located in, if it changed, or <c>LevelRegionType.NONE</c>, if there is no need for change.</returns>
	protected virtual LevelRegionType GetCurrentRegion(Vector3 playerPosition) {
		if (!RaceControllerBase.Instance.IsInitialized) return LevelRegionType.NONE;
		// Get region of the nearest terrain point
		TerrainPoint nearestTerrainPoint = RaceControllerBase.Instance.Level.GetNearestTerrainPoint(playerPosition);
		LevelRegionType region = nearestTerrainPoint.region;
		// Determine if the region has changed
		if (region != LastRegion) {
			return region;
		} else {
			return LevelRegionType.NONE;
		}
	}

	/// <summary>
	/// Computes player's current height above ground (or water level).
	/// </summary>
	/// <param name="playerPosition">Current player's position.</param>
	/// <returns>Player's height above ground.</returns>
	protected virtual float GetCurrentAltitude(Vector3 playerPosition) {
		if (!RaceControllerBase.Instance.IsInitialized) return 0f;
		TerrainPoint nearestTerrainPoint = RaceControllerBase.Instance.Level.GetNearestTerrainPoint(playerPosition);
		float altitudeAboveGround = playerPosition.y - Mathf.Max(0, nearestTerrainPoint.position.y); // above terrain or water level, whichever is higher
		return altitudeAboveGround; // would not work for more complicated terrain (e.g. tunnel)
	}

	// Maps level region type to a number which is used in FMOD as a Region parameter value for that region type
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

	// Sets Region parameter (if it changed) and Altitude parameter
	// Also handles altering audio when being underwater
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
