using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class LocalizationManager : MonoBehaviourSingleton<LocalizationManager>, IInitializableSingleton {

	[Tooltip("Name of the file (in the Resources) containing localization data. Without extension (e.g. 'translations').")]
	public string fileName = "translations";

	// The name of the currently selected language
	public string CurrentLanguage { get; private set; }

	// Callback on language changed - used from LocalizedTextMeshProUI to update text content
	public Action onCurrentLanguageChanged;


	// A dictionary containing for each language all the keys and their corresponding phrases
	// ...............language.............key....phrase
	private Dictionary<string, Dictionary<string, string>> completeDictionary;
	// A dictionary containing only keys and phrases of the currently selected language
	//..................key....phrase
	private Dictionary<string, string> currentLanguageDictionary;

	// An array containing names of all the available languages
	private string[] availableLanguages;


	private const string missingPhraseError = "MISSING PHRASE";
	private const string missingKeyError = "MISSING KEY";


	// Returns true if the given key exists, false otherwise
	// Output parameter localizedString will contain phrase in the currently selected language and with the given key
	// If the key does not exist or the associated phrase is empty, it will contain a placeholder
	public bool TryGetLocalizedString(string key, out string localizedString) {
		// Check if the key exists
		if (currentLanguageDictionary.TryGetValue(key, out localizedString)) {
			if (string.IsNullOrEmpty(localizedString)) { // the key exists but the phrase does not
				Debug.LogWarning($"There is an empty phrase for the key '{key}' in the language '{CurrentLanguage}'.");
				localizedString = missingPhraseError;
				return false;
			} else { // the jey exists and the phrase too
				return true;
			}
		} else { // the key does not exist
			Debug.Log($"There is no key '{key}' for the language '{CurrentLanguage}'.");
			localizedString = missingKeyError;
			return false;
		}
	}

	// Sets the current language to the given one and invokes onCurrentLanguageChanged callback
	// If the given language does not exist (or is not supported), nothing is changed
	public void ChangeCurrentLanguage(string languageName) {
		// Check whether the given language exists
		if (completeDictionary.TryGetValue(languageName, out Dictionary<string, string> languageDictionary)) {
			CurrentLanguage = languageName;
			currentLanguageDictionary = languageDictionary;
			onCurrentLanguageChanged.Invoke(); // inform others interested that the language was changed
		} else {
			Debug.LogWarning($"There is no language '{languageName}'.");
		}
	}

	// Returns an array of names of all the available languaages (in the same order as in the input file)
	public string[] GetAvailableLanguages() {
		return availableLanguages;
	}

	public void InitializeSingleton() {
		
	}

	// Loads languages and phrases from an input file located in Resources
	private void LoadDataFromJSONFile() {
		// Initialize dictioanry
		completeDictionary = new Dictionary<string, Dictionary<string, string>>();
		// Load the file
		TextAsset translations = Resources.Load<TextAsset>(fileName);
		if (translations == null) {
			Debug.LogError($"The localization file '{fileName}' does not exist in the Resources.");
			return;
		}
		JArray lines = JArray.Parse(translations.text); // an array (= lines) of objects (= phrases in different languages)
		// Get an array of all the available languages
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

	private void Awake() {
		LoadDataFromJSONFile();
	}
}
