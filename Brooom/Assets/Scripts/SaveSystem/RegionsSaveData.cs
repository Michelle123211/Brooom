using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RegionsSaveData {

	#region Visited regions
	// Regions visited stored in array (which is serializable by JsonUtility as opposed to Dictionary)
	public string[] regionsVisitedArray = null; // each string contains region identifier and whether it was visited already separated by |

	// Property with getter/setter converting Dictionary<LevelRegionType, bool> to array of strings and vice versa
	public Dictionary<LevelRegionType, bool> RegionsVisited {
		get {
			return GetDictionaryOfRegions(regionsVisitedArray);
		}
		set {
			regionsVisitedArray = GetArrayOfRegions(value);
		}
	}

	private Dictionary<LevelRegionType, bool> GetDictionaryOfRegions(string[] regionsArray) {
		Dictionary<LevelRegionType, bool> regionsDictionary = new Dictionary<LevelRegionType, bool>();
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
