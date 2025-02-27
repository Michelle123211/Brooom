using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


[CreateAssetMenu(fileName = "FMODEvents", menuName = "Audio/FMOD Events")]
public class FMODEvents : ScriptableObject
{
    [field: SerializeField]
    public GUIEvents GUI { get; private set; }

    [field: SerializeField]
    public GameEvents Game { get; private set; }
}

[System.Serializable]
public class GUIEvents {
    [field: SerializeField]
    public EventReference Click { get; private set; }
}

[System.Serializable]
public class GameEvents {
    [field: SerializeField]
    public EventReference SpellCast { get; private set; }
}
