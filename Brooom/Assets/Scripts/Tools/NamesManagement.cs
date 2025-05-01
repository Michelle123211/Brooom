using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// This class is used to provide random names for the opponents (in race and in leaderboard) as well as random names used in character creation.
/// </summary>
public class NamesManagement : MonoBehaviour {

	// Name of the file in the StreamingAssets folder containing possible names for the randomization
	private const string namesFilename = "names.txt";
	// All possible names to choose from (loaded from a file)
	private static List<string> possibleNames = null;

	/// <summary>
	/// Returns a random name from a list of names loaded from a file.
	/// </summary>
	/// <returns>A random name.</returns>
	public static string GetRandomName() {
		if (possibleNames == null || possibleNames.Count == 0)
			LoadNamesFromFile();
		return possibleNames[Random.Range(0, possibleNames.Count)];
	}

	// Loads names from a file into the possibleNames list
	private static void LoadNamesFromFile() {
		// Load list of names from a file
		if (string.IsNullOrEmpty(namesFilename)) { // empty filename
			Debug.LogError($"An empty filename was given for the file containing a list of names.");
		} else {
			string path = Path.Combine(Application.streamingAssetsPath, namesFilename);
			try {
				if (!File.Exists(path)) { // file does not exist
					Debug.LogError($"The file '{namesFilename}' does not exist in the StreamingAssets.");
				} else using (StreamReader reader = new StreamReader(path)) { // reading the file
						possibleNames = ParseNamesFromReader(reader);
					}
			} catch (IOException ex) { // exception while reading
				Debug.LogError($"An exception occurred while reading the file '{namesFilename}': {ex.Message}");
			}
		}
		// Make sure there is at least one name
		if (possibleNames == null || possibleNames.Count == 0)
			possibleNames = UseDefaultNames();
	}

	// Parses names from a StreamReader of the names file
	private static List<string> ParseNamesFromReader(StreamReader reader) {
		List<string> names = new();
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

	// Returns a list of default names (used when the names file is not found)
	private static List<string> UseDefaultNames() {
		List<string> names = new List<string> {
			"Emil",
			"Michelle",
			"Igor",
			"Airin",
			"Lucie",
			"Karel",
			"Iveta",
			"Cassie",
			"Bedøich",
			"Lída",
			"Villie",
			"Pavel"
		};
		return names;
	}
}
