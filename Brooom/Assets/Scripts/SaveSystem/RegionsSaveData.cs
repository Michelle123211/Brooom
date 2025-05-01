using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class used when persistently storing data related to regions (i.e. visited regions).
/// </summary>
[System.Serializable]
public class RegionsSaveData {

	#region Visited regions
	/// <summary>
	/// Whether a region has been visited, stored in array (which is serializable by JsonUtility as opposed to Dictionary).
	/// Each string contains region identifier and <c>bool</c> whether it was visited already separated by |.
	/// </summary>
	public string[] regionsVisitedArray = null;

	/// <summary>
	/// Whether a region has been visited already.
	/// Getter and setter convert <c>string[]</c> array from <c>regionsVisitedArray</c> to <c>Dictionary&lt;LevelRegionType, bool&gt;</c> and vice versa.
	/// </summary>
	public Dictionary<LevelRegionType, bool> RegionsVisited {
		get {
			return GetDictionaryOfRegions(regionsVisitedArray);
		}
		set {
			regionsVisitedArray = GetArrayOfRegions(value);
		}
	}

	// Creates a dictionary with information about whether regions were visited or not out of anarray
	private Dictionary<LevelRegionType, bool> GetDictionaryOfRegions(string[] regionsArray) {
		Dictionary<LevelRegionType, bool> regionsDictionary = new();
		foreach (var region in regionsArray) {
			string[] parts = region.Split('|');
			if (parts.Length == 2 && Enum.TryParse<LevelRegionType>(parts[0], out LevelRegionType regionType)) {
				if (parts[1] == "false")
					regionsDictionary[regionType] = false;
				else if (parts[1] == "true")
					regionsDictionary[regionType] = true;
			}
		}
		return regionsDictionary;
	}

	// Creates an array with information about whether regions were visited or not out of a dictionary
	private string[] GetArrayOfRegions(Dictionary<LevelRegionType, bool> regionsDictionary) {
		string[] regionsArray = new string[regionsDictionary.Count];
		int i = 0;
		foreach (var region in regionsDictionary) {
			regionsArray[i] = $"{region.Key}|{(region.Value ? "true" : "false")}";
			i++;
		}
		return regionsArray;
	}
	#endregion

}
