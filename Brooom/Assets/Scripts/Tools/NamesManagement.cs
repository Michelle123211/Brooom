using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// This class is used to provide names for the opponents in leaderboard and the randomization in character creation
public class NamesManagement : MonoBehaviour
{
    public static List<string> LoadNames(string namesFilename) {
		List<string> names = null;
		// Load list of names from a file
		if (string.IsNullOrEmpty(namesFilename)) { // empty filename
			Debug.LogError($"An empty filename was given for the file containing a list of names.");
		} else {
			string path = Path.Combine(Application.streamingAssetsPath, namesFilename);
			try {
				if (!File.Exists(path)) { // file does not exist
					Debug.LogError($"The file '{namesFilename}' does not exist in the StreamingAssets.");
				} else using (StreamReader reader = new StreamReader(path)) { // reading the file
						names = ParseNamesFromReader(reader);
					}
			} catch (IOException ex) { // exception while reading
				Debug.LogError($"An exception occurred while reading the file '{namesFilename}': {ex.Message}");
			}
		}
		// Make sure there is at least one name
		if (names == null || names.Count == 0)
			names = UseDefaultNames();
		return names;
	}

	private static List<string> ParseNamesFromReader(StreamReader reader) {
		List<string> names = new List<string>();
		string line;
		while ((line = reader.ReadLine()) != null) {
			if (string.IsNullOrEmpty(line)) continue; // skip empty rows
			if (line[0] == '#') continue; // skip lines beginning with # (denotes names sections)
			string name = line.Trim();
			// Take only first 20 characters if the name is longer than that
			if (name.Length > 20) name = name.Substring(0, 20);
			names.Add(name);
		}
		return names;
	}

	private static List<string> UseDefaultNames() {
		List<string> names = new List<string>();
		names.Add("Emil");
		return names;
	}
}
