using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

/// <summary>
/// A class containing references to useful FMOD audio events, so they can be used easily from code (access without having to use strings with path or any other direct dependency).
/// Events are divided into two groups - GUI and Game.
/// It is a non-exhaustive list with only simple one-shots which are invoked from scripts.
/// </summary>
[CreateAssetMenu(fileName = "FMODEvents", menuName = "Audio/FMOD Events")]
public class FMODEvents : ScriptableObject {
	[Tooltip("GUI events references.")]
	[field: SerializeField] public GUIEvents GUI { get; private set; }

	[Tooltip("Game events references.")]
	[field: SerializeField] public GameEvents Game { get; private set; }
}

/// <summary>
/// A class containing references to useful FMOD GUI events (e.g., click, toggle, purchase).
/// It is a non-exhaustive list with only simple one-shots which are invoked from scripts.
/// </summary>
[System.Serializable]
public class GUIEvents {

	[field: Header("General UI")]

	[field: SerializeField] public EventReference Click { get; private set; }

	[field: SerializeField] public EventReference Release { get; private set; }

	[field: SerializeField] public EventReference Toggle { get; private set; }

	[field: SerializeField] public EventReference SliderValue { get; private set; }

	[field: SerializeField] public EventReference ScrollbarValue { get; private set; }

	[field: SerializeField] public EventReference PanelOpen { get; private set; }

	[field: SerializeField] public EventReference PanelClose { get; private set; }

	[field: SerializeField] public EventReference KeyDown { get; private set; }


	[field: Header("Shop UI")]

	[field: SerializeField] public EventReference Purchase { get; private set; }

	[field: SerializeField] public EventReference PurchaseDenied { get; private set; }


	[field: Header("Race results UI")]

	[field: SerializeField] public EventReference RaceResult { get; private set; }

	[field: SerializeField] public EventReference RaceResultPlayer { get; private set; }


	[field: Header("Game end UI")]

	[field: SerializeField] public EventReference Applause { get; private set; }

	[field: SerializeField] public EventReference FireworkWhistle { get; private set; }

	[field: SerializeField] public EventReference FireworkHit { get; private set; }

}

/// <summary>
/// A class containing references to useful FMOD Game events (e.g., spell cast, race started, hoop missed).
/// It is a non-exhaustive list with only simple one-shots which are invoked from scripts.
/// </summary>
[System.Serializable]
public class GameEvents {

	[field: Header("Spells")]

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
