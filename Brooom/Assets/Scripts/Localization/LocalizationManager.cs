using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


/// <summary>
/// A singleton managing localization table and providing methods for getting the correct localized string 
/// based on a key and the currently selected language.
/// </summary>
public class LocalizationManager : MonoBehaviourSingleton<LocalizationManager>, ISingleton {

	[Tooltip("Name of the file (in the Resources) containing localization data. Without extension (e.g. 'translations').")]
	public string fileName = "translations";

	/// <summary>The name of the currently selected language.</summary>
	public string CurrentLanguage { get; private set; }

	/// <summary>Called whenever the current language changes.</summary>
	public event Action onCurrentLanguageChanged;

	/// <summary>A dictionary containing all the localization keys and their corresponding phrases for each language.</summary>
	// ...............language.............key....phrase
	private Dictionary<string, Dictionary<string, string>> completeDictionary;
	/// <summary>A dictionary containing only localization keys and their corresponding phrases of the currently selected language.</summary>
	//..................key....phrase
	private Dictionary<string, string> currentLanguageDictionary;

	// An array containing names of all the available languages
	private string[] availableLanguages;


	private const string missingPhraseError = "MISSING PHRASE"; // used whenever there is no phrase for a particular localization key
	private const string missingKeyError = "MISSING KEY"; // used whenever the particular localization key cannot be found


	/// <summary>
	/// Checks whether the given localization key exists, and if so, gets a phrase corresponding to it in the currently selected language.
	/// </summary>
	/// <param name="key">Localization key used for looking up a phrase.</param>
	/// <param name="localizedString">Phrase associated with the given key in the currently selected language, or a placeholder if the key doesn't exist or the associated phrase is empty.</param>
	/// <returns><c>true</c> if the given key exists, <c>false</c> otherwise.</returns>
	public bool TryGetLocalizedString(string key, out string localizedString) {
		// Check if the key exists
		if (currentLanguageDictionary.TryGetValue(key, out localizedString)) {
			if (string.IsNullOrEmpty(localizedString)) { // the key exists but the phrase does not
				Debug.LogWarning($"There is an empty phrase for the key '{key}' in the language '{CurrentLanguage}'.");
				localizedString = missingPhraseError;
				return false;
			} else { // the key exists and the phrase too
				return true;
			}
		} else { // the key does not exist
			Debug.Log($"There is no key '{key}' for the language '{CurrentLanguage}'.");
			localizedString = missingKeyError;
			return false;
		}
	}

	/// <summary>
	/// Gets a phrase corresponding to the given key in the currently selected language.
	/// </summary>
	/// <param name="key">Localization key used for looking up a phrase.</param>
	/// <returns>Phrase associated with the given key in the currently selected language, or a placeholder if the key doesn't exist or the associated phrase is empty.</returns>
	public string GetLocalizedString(string key) {
		bool keyExists = TryGetLocalizedString(key, out string phrase);
		return phrase;
	}

	/// <summary>
	/// Changes the currently selected language to the given one and invokes callbacks registered for that event.
	/// If the given language does not exist (or is not supported), the original language stays selected.
	/// </summary>
	/// <param name="languageName">Name of the language to be selected.</param>
	public void ChangeCurrentLanguage(string languageName) {
		// Check whether the given language exists
		if (completeDictionary.TryGetValue(languageName, out Dictionary<string, string> languageDictionary)) {
			CurrentLanguage = languageName;
			currentLanguageDictionary = languageDictionary;
			onCurrentLanguageChanged?.Invoke(); // inform others interested that the language was changed
			// Save the current language persistently to a file
			SaveSystem.SaveCurrentLanguage(CurrentLanguage);
		} else {
			Debug.LogWarning($"There is no language '{languageName}'.");
		}
	}

	/// <summary>
	/// Gets an array of all available languages.
	/// </summary>
	/// <returns>An array of names of available languages.</returns>
	public string[] GetAvailableLanguages() {
		return availableLanguages;
	}

	static LocalizationManager() {
		// Singleton options override
		Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances | SingletonOptions.LazyInitialization;
	}

	/// <inheritdoc/>
	public void AwakeSingleton() {
	}

	/// <summary>
	/// Initializes localization table by loading it from a file, and sets currently selected language to the one persistently saved (or default).
	/// </summary>
	public void InitializeSingleton() {
		// Initialize localization table
		LoadDataFromJSONFile();
		// Load the persistently saved selected language from earlier
		string loadedLanguage = SaveSystem.LoadCurrentLanguage();
		if (!string.IsNullOrEmpty(loadedLanguage)) {
			ChangeCurrentLanguage(loadedLanguage);
		}
	}

	/// <summary>
	/// Initializes localization table from a JSON file located in Resources.
	/// Loads all languages and phrases and stores them in internal data structures.
	/// </summary>
	private void LoadDataFromJSONFile() {
		// Initialize dictionary
		completeDictionary = new Dictionary<string, Dictionary<string, string>>();
		// Load the file
		TextAsset translations = Resources.Load<TextAsset>(fileName);
		if (translations == null) {
			Debug.LogError($"The localization file '{fileName}' does not exist in the Resources.");
			return;
		}
		JArray lines = JArray.Parse(translations.text); // an array (= lines) of objects (= phrases in different languages)
		// Get an array of all the available languages - in the same order as in the input file
		JObject firstLine = (JObject)lines[0];
		availableLanguages = new string[firstLine.Count - 1];
		int i = -1;
		foreach (var pair in firstLine) {
			if (i >= 0)
				availableLanguages[i] = pair.Key;
			i++;
		}
		// Initialize dictionaries for all the languages
		foreach (var lang in availableLanguages)
			completeDictionary.Add(lang, new Dictionary<string, string>());
		// Store all the phrases in all of the languages into the dictionary
		foreach (JToken line in lines) {
			JObject lineObject = (JObject)line;
			if (lineObject.Count == 0) continue; // empty row
			string key = (string)lineObject["Key"];
			foreach (var lang in availableLanguages) {
				string phrase = (string)lineObject[lang];
				completeDictionary[lang][key] = phrase;
			}
		}
		// Set the first language as the default one
		CurrentLanguage = availableLanguages[0];
		currentLanguageDictionary = completeDictionary[CurrentLanguage];
		// Invoke the callback on language change
		onCurrentLanguageChanged?.Invoke();
	}

	private void DebugOutput() {
		foreach (var lang in completeDictionary.Keys) {
			Debug.Log($"LANGUAGE: {lang}");
			foreach (var phrase in completeDictionary[lang]) {
				Debug.Log($"PHRASE <{phrase.Key}>: {phrase.Value}");
			}
			Debug.Log("-------------------------");
		}
	}
}
