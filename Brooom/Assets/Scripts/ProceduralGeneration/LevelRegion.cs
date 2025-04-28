using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scriptable Object containing all information related to a single region (terrain or track) available in the game.
/// </summary>
[CreateAssetMenu(fileName = "LevelRegion", menuName = "Level/Level Region")]
public class LevelRegion : ScriptableObject {

	[Tooltip("A name of the region which is displayed whenever this region's Scriptable Object is added to a list in an Inspector.")]
	public string displayName;
	[Tooltip("Type of the region this instance represents.")]
	public LevelRegionType regionType;
	[Tooltip("Color assigned to this region, which is then used as a terrain color.")]
	public Color color;
	[Tooltip("An image of what the region looks like in the game. It is used when displaying information about a new available region to the player.")]
	public Sprite regionImage;

	// TODO: Add environment features etc. (with some description how to place them - e.g. random rotation in which axis, lower density at the edges of the region, etc.)
}

/// <summary>
/// All regions (terrain or track) available in the game, with values assigned according to how they are unlocked.
/// </summary>
public enum LevelRegionType {
	NONE = 0,
	// Terrain - default
	AboveWater = 101,
	// Terrain - unlocked by tutorial
	EnchantedForest = 111, // after shop tutorial (to make sure the player has finished a few races)
	// Terrain - unlocked by level length
	AridDesert = 121,
	BloomingMeadow = 122,
	StormyArea = 123,
	// Terrain - unlocked by broom upgrade
	SnowyMountain = 131,
	// Track - default
	MysteriousTunnel = 201,
	// Track - unlocked by broom upgrade
	AboveClouds = 211
}