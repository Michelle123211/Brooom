using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


[CreateAssetMenu(fileName = "FMODEvents", menuName = "Audio/FMOD Events")]
public class FMODEvents : ScriptableObject
{
    [field: SerializeField] public GUIEvents GUI { get; private set; }

    [field: SerializeField]  public GameEvents Game { get; private set; }
}

[System.Serializable]
public class GUIEvents {

    [field:Header("General UI")]

    [field: SerializeField] public EventReference Click { get; private set; }

    [field: SerializeField] public EventReference Release { get; private set; }

    [field: SerializeField] public EventReference Toggle { get; private set; }

    [field: SerializeField] public EventReference SliderValue { get; private set; }

    [field: SerializeField] public EventReference ScrollbarValue { get; private set; }

    [field: SerializeField] public EventReference PanelOpen { get; private set; }

    [field: SerializeField] public EventReference PanelClose { get; private set; }

    [field: SerializeField] public EventReference KeyDown { get; private set; }


    [field:Header("Shop UI")]

    [field: SerializeField] public EventReference Purchase { get; private set; }

    [field: SerializeField] public EventReference PurchaseDenied { get; private set; }


    [field:Header("Game end UI")]

    [field: SerializeField] public EventReference Applause { get; private set; }

    [field: SerializeField] public EventReference FireworkWhistle { get; private set; }

    [field: SerializeField] public EventReference FireworkHit { get; private set; }

}

[System.Serializable]
public class GameEvents {

    [field:Header("Spells")]

    [field: SerializeField] public EventReference SpellCast { get; private set; }
    [field: SerializeField] public EventReference SpellCastFailed { get; private set; }
    [field: SerializeField] public EventReference SpellHit { get; private set; }
    [field: SerializeField] public EventReference SpellBlocked { get; private set; }

    [field: SerializeField] public EventReference SpellSwapped { get; private set; }


    [field: Header("Race")]

    [field: SerializeField] public EventReference CountdownRace { get; private set; }

    [field: SerializeField] public EventReference CountdownStartingZone { get; private set; }

    [field: SerializeField] public EventReference RaceStarted { get; private set; }
    [field: SerializeField] public EventReference RaceFinished { get; private set; }


    [field: Header("Track")]

    [field: SerializeField] public EventReference HoopPassed { get; private set; }
    [field: SerializeField] public EventReference HoopMissed { get; private set; }

    [field: SerializeField] public EventReference CheckpointPassed { get; private set; }
    [field: SerializeField] public EventReference CheckpointMissed { get; private set; }

    [field: SerializeField] public EventReference BonusPickedUp { get; private set; }

}
