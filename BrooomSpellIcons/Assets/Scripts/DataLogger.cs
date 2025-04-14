using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour {

	// Simple singleton
	public static DataLogger Instance { get; private set; }

	private const string fileName = "ExperimentResults.data";
	private CultureInfo culture = new CultureInfo("cs-CZ");
	private StreamWriter file;

	public void Log(string message, bool includeDateTime = true) {
		// Write the message into a file - prefix it with current date and time
		if (includeDateTime)
			file.WriteLine($"{DateTime.Now.ToString(culture)} | {message}");
		else
			file.WriteLine(message);
	}

	private void Awake() {
		Instance = this;
		// Open a file for writing
		file = new StreamWriter(Path.Combine(Application.persistentDataPath, fileName), true); // append set to true just to make sure we wouldn't lose data if the player opened the build again by accident
		file.AutoFlush = true;
		Log("Spell icons experiment started.");
	}

	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		// Close file
		Log("Spell icons experiment ended.");
		Log("========================================", false);
		file.Close();
		// Copy the file to Desktop for easier access
		// Make sure the folder exists
		string desktopFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Brooom");
		if (!Directory.Exists(desktopFolder))
			Directory.CreateDirectory(desktopFolder);
		// Then copy the analytics file there
		string originalFilePath = Path.Combine(Application.persistentDataPath, fileName);
		string newFilePath = Path.Combine(desktopFolder, fileName);
		if (File.Exists(originalFilePath))
			File.Copy(originalFilePath, newFilePath, true); // will overwrite an existing file
	}

}
