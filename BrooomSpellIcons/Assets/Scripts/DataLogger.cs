using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour {

	// Simple singleton
	public static DataLogger Instance { get; private set; }

	private CultureInfo culture = new CultureInfo("cs-CZ");
	private StreamWriter file;

	public void Log(string message) {
		// Write the message into a file - prefix it with current date and time
		file.WriteLine($"{DateTime.Now.ToString(culture)} | {message}");
	}

	private void Awake() {
		Instance = this;
		// Open a file for writing
		file = new StreamWriter(Application.persistentDataPath + "/icons_experiment.data", true); // append set to true just to make sure we wouldn't lose data if the player opened the build again by accident
		file.AutoFlush = true;
		Log("Spell icons experiment started.");
	}

	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		// Close file
		Log("Spell icons experiment ended.");
		file.WriteLine("===========================================");
		file.Close();
	}

}
