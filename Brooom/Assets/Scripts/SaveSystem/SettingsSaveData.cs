using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class used when persistently storing current settings values.
/// </summary>
[System.Serializable]
public class SettingsSaveData {

    /// <summary>Selected language.</summary>
    public string currentLanguage;
    
    /// <summary>Selected mouse sensitivity.</summary>
    public float mouseSensitivity = 3;

    /// <summary>Whether tutorials are enabled.</summary>
    public bool enableTutorial = true;
    /// <summary>Whether training before race should be skipped.</summary>
    public bool skipTraining = false;

    /// <summary>Master volume (set in VCA).</summary>
    public float masterVolume = 100;
    /// <summary>Music volume (set in  VCA).</summary>
    public float musicVolume = 100;
    /// <summary>Ambience volume (set in  VCA).</summary>
    public float ambienceVolume = 100;
    /// <summary>Sound effects volume (set in  VCA).</summary>
    public float soundEffectsVolume = 100;

}
