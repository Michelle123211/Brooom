using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Cheats : MonoBehaviourSingleton<Cheats>, ISingleton {

	[Header("Parameters")]
	[Tooltip("Whether the cheats are enabled in the game (so they can be easily disabled in the final product).")]
	[SerializeField] bool cheatsEnabled = true;
	[Tooltip("How many previous commands are remembered.")]
	[SerializeField] int historyLength;

	[Header("UI elements")]
	[SerializeField] TMP_InputField commandField;
	[SerializeField] TextMeshProUGUI messageField;
	[SerializeField] GameObject messageArea;
	[SerializeField] GameObject cheatsUI;


	// Cheat commands
	private Dictionary<string, CheatCommand> commandsDictionary;
	private List<CheatCommand> commands;

	private bool cheatsAreVisible = false;

	private string[] commandHistory; // cycles over edges
	private int lastCommandIndex; // last command entered
	private int historyOffset; // offset of the last command displayed from the history

	private void OnCommandEntered() {
		string command = commandField.text;
		if (!string.IsNullOrEmpty(command)) {
			// Process the text from the command line
			ProcessCommand(command);
			// Clear the command line
			commandField.text = string.Empty;
		}
		// Don't lose focus
		commandField.Select();
		commandField.ActivateInputField();
	}

	private void ProcessCommand(string commandLine) {
		// Parse the command
		string[] commandParts = commandLine.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (commandParts.Length < 1) return;
		// Handle help
		if (commandParts[0] == "help") {
			ProvideHelp(commandParts);
		}
		// Handle command
		else {
			string commandName = commandParts[0];
			if (commandsDictionary.ContainsKey(commandName)) {
				if (commandsDictionary[commandName].IsAvailable()) {
					CommandParseResult result = commandsDictionary[commandName].parseCommand(commandParts);
					if (result.isSuccessful) {
						DisplayMessage(result.message);
					} else {
						DisplayError(result.message);
					}
				} else {
					DisplayError($"Command '{commandName}' is not available in the current scene.");
				}
			} else {
				DisplayError($"Unknown command '{commandName}'");
			}
		}
		// Store command into history
		lastCommandIndex = (lastCommandIndex + 1) % commandHistory.Length;
		commandHistory[lastCommandIndex] = commandLine;
		historyOffset = -1;
	}

	private void ProvideHelp(string[] commandParts) {
		if (commandParts.Length == 1) {
			StringBuilder message = new StringBuilder();
			message.Append("Use a command or type 'help <command>' for more info. \nAvailable commands:");
			foreach (var command in commands) {
				if (command.IsAvailable()) {
					message.Append(" ");
					message.Append(command.commandName);
				}
			}
			message.Append(".");
			DisplayMessage(message.ToString());
		} else if (commandParts.Length == 2) {
			string commandName = commandParts[1];
			if (commandsDictionary.ContainsKey(commandName)) {
				if (commandsDictionary[commandName].IsAvailable()) {
					DisplayMessage(commandsDictionary[commandName].helpMessage);
				} else {
					DisplayMessage("(This command is not available in the current scene.) " + commandsDictionary[commandName].helpMessage);
				}
			} else {
				DisplayError($"Unknown command '{commandName}'");
			}
		} else {
			DisplayError("Incorrect form of the 'help'. Must be 'help <command>', e.g. 'help coins', 'help speed'.");
		}
	}

	private void DisplayError(string errorMessage) {
		// Write ERROR: in red using rich text, then the rest of the message
		string result = "<color=red>ERROR:</color> " + errorMessage;
		DisplayMessage(result);
	}

	private void DisplayMessage(string message) {
		// Set the message to the text field and show the message area
		messageField.text = message;
		Utils.TweenAwareEnable(messageArea);
	}

	private void ToggleVisibility() {
		if (!cheatsAreVisible) {
			// Show the cheats UI
			Utils.TweenAwareEnable(cheatsUI);
			cheatsAreVisible = true;
			// Focus on the input field
			commandField.Select();
			commandField.ActivateInputField();
		} else {
			// Hide the cheats UI
			Utils.TweenAwareDisable(cheatsUI);
			cheatsAreVisible = false;
		}
	}

	private void HandleCommandHistory() {
		// Reset if the field was erased
		if (string.IsNullOrEmpty(commandField.text)) {
			historyOffset = -1;
		}
		// Show previous command in the command line
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			historyOffset++;
			if (historyOffset > commandHistory.Length - 1) historyOffset = historyLength - 1;
			int newIndex = (lastCommandIndex - historyOffset + historyLength) % historyLength;
			if (string.IsNullOrEmpty(commandHistory[newIndex])) historyOffset--;
			else {
				commandField.text = commandHistory[newIndex];
			}
		}
		// Show following command in the command line
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (historyOffset >= 0) {
				historyOffset--;
				if (historyOffset == -1) {
					commandField.text = string.Empty;
				} else {
					int newIndex = (lastCommandIndex - historyOffset + historyLength) % historyLength;
					if (!string.IsNullOrEmpty(commandHistory[newIndex])) {
						commandField.text = commandHistory[newIndex];
					}
				}
			}
		}

		commandField.caretPosition = commandField.text.Length;
	}

	private void InitializeCommandList() {
		commands = new List<CheatCommand>();
		// TODO
		// coins - change coins amount
		commands.Add(new CheatCommand("coins", "Adds this amount of coins to the current amount. Usage: 'coins <amount>', e.g. 'coins 1000', 'coins -100'.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is required.";
			else if (!int.TryParse(commandParts[1], out int amount)) message = "Invalid parameter, an integer is required.";
			// Perform the command
			else {
				int oldValue = PlayerState.Instance.coins;
				PlayerState.Instance.ChangeCoinsAmount(amount);
				success = true;
				message = $"The amount of coins has been changed from {oldValue} to {PlayerState.Instance.coins}.";
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		}));
		// scene - change scene
		commands.Add(new CheatCommand("scene", "Changes the current scene to the given one. Usage: 'scene <sceneName>', e.g. 'scene PlayerOverview'.\nAvailable scenes: MainMenu, CharacterCreation, Tutorial, Race, PlayerOverview, TestingTrack, Ending, Start, Exit.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is required.";
			else if (SceneManager.GetSceneByName(commandParts[1]) == null) message = $"Scene named '{commandParts[1]} does not exist. Try using one of the following instead: MainMenu, CharacterCreation, Tutorial, Race, PlayerOverview, TestingTrack, Ending, Start, Exit.'";
			// Perform the command
			else {
				SceneLoader.Instance.LoadScene(commandParts[1]);
				success = true;
				message = $"The scene has been changed to {commandParts[1]}.";
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		}));
		// stats - change player statistics values
		commands.Add(new CheatCommand("stats", "Changes values of individual player statistics. Usage: 'stats (<statLetter>=<0-100>){1,5}', e.g. 'stats m=45', 'stats e=83 p=21'.\nAvailable stats: e (endurance), s (speed), d (dexterity), p (precision), m (magic).", (commandParts) => {
			// Handle errors
			if (commandParts.Length < 2 || commandParts.Length > 6) {
				return new CommandParseResult {
					isSuccessful = false, message = "Invalid number of parameters, at least one is required and at most five."
				};
			} else {
				PlayerStats stats = PlayerState.Instance.CurrentStats;
				// Parse individual stats and try to override the values
				for (int i = 1; i < commandParts.Length; i++) {
					string[] statParts = commandParts[i].Trim().Split('=', StringSplitOptions.RemoveEmptyEntries);
					if (statParts.Length != 2)
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter. All parameters must be in a form of <statLetter>=<value>."
						};
					if (!int.TryParse(statParts[1], out int statValue))
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter, an integer is required for the stat value."
						};
					if (statValue < 0 || statValue > 100)
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter, stat value must be between 0 and 100 (inclusive)."
						};
					string statLetter = statParts[0];
					if (statLetter == "e") stats.endurance = statValue;
					else if (statLetter == "s") stats.speed = statValue;
					else if (statLetter == "d") stats.dexterity = statValue;
					else if (statLetter == "p") stats.precision = statValue;
					else if (statLetter == "m") stats.magic = statValue;
					else // unknown
						return new CommandParseResult {
							isSuccessful = false, message = $"Invalid parameter, stat {statLetter} is not known. Use only e (endurance), s (speed), d (dexterity), p (precision), m (magic)."
						};
				}
				// Everything was all right
				string message = $"Player stats have been changed from ({PlayerState.Instance.CurrentStats.endurance}, {PlayerState.Instance.CurrentStats.speed}, {PlayerState.Instance.CurrentStats.dexterity}, {PlayerState.Instance.CurrentStats.precision}, {PlayerState.Instance.CurrentStats.magic}).";
				PlayerState.Instance.CurrentStats = stats;
				message += $" to ({PlayerState.Instance.CurrentStats.endurance}, {PlayerState.Instance.CurrentStats.speed}, {PlayerState.Instance.CurrentStats.dexterity}, {PlayerState.Instance.CurrentStats.precision}, {PlayerState.Instance.CurrentStats.magic}).";
				return new CommandParseResult {
					isSuccessful = true,
					message = message
				};
			}
		}));
		// upgrade - upgrade the broom
		commands.Add(new CheatCommand("upgrade", "Unlocks all broom upgrades or only levels up the given one. Usage: 'unlock all' or 'unlock <upgradeIdentifier>', e.g. 'unlock Speed'.\nAvailable upgrades: Speed, Control, Elevation..", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is required.";
			else if (commandParts[1] == "all") {
				// Unlock all broom upgrades
				Broom broom = UtilsMonoBehaviour.FindObjectOfTypeAndTag<Broom>("Player");
				if (broom == null)
					message = "It is not possible to use this command in a scene without a broom.";
				else {
					foreach (var upgrade in broom.GetAvailableUpgrades()) {
						while (upgrade.CurrentLevel != upgrade.MaxLevel) {
							upgrade.LevelUp();
							PlayerState.Instance.SetBroomUpgradeLevel(upgrade.UpgradeName, upgrade.CurrentLevel, upgrade.MaxLevel);
						}
					}
					return new CommandParseResult { isSuccessful = true, message = "All broom upgrades have been unlocked and equipped." };
				}
			} else {
				// Unlock only one level of the given upgrade
				Broom broom = UtilsMonoBehaviour.FindObjectOfTypeAndTag<Broom>("Player");
				if (broom == null)
					message = "It is not possible to use this command in a scene without a broom.";
				else {
					foreach (var upgrade in broom.GetAvailableUpgrades()) {
						if (upgrade.UpgradeName == commandParts[1]) {
							upgrade.LevelUp();
							PlayerState.Instance.SetBroomUpgradeLevel(upgrade.UpgradeName, upgrade.CurrentLevel, upgrade.MaxLevel);
							return new CommandParseResult { isSuccessful = true, message = $"Broom upgrade '{commandParts[1]}' has been leveled up." };
						}
					}
					return new CommandParseResult { isSuccessful = false, message = $"Invalid parameter, upgrade '{commandParts[1]}' is not known." };
				}
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		}, enabledScenes: new Scene[] { Scene.Race, Scene.PlayerOverview, Scene.TestingTrack, Scene.MainMenu, Scene.Tutorial })); // broom must be available
		// PARTIAL: spell - unlock all spells or only the given one
		commands.Add(new CheatCommand("spell", "Unlocks all spells or only the given one. Usage 'spell all' or 'spell <spellIdentifier>'.\nAvailable spells: .", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is needed.";
			else if (commandParts[1] == "all") {
				// Unlock all spells
				foreach (var spell in PlayerState.Instance.spellAvailability) {
					if (!spell.Value) PlayerState.Instance.UnlockSpell(spell.Key);
				}
				return new CommandParseResult { isSuccessful = true, message = "All spells have been unlocked." };
			} else {
				// Unlock only the given spell
				if (PlayerState.Instance.spellAvailability.ContainsKey(commandParts[1])) { // if the spell exists
					PlayerState.Instance.UnlockSpell(commandParts[1]);
					return new CommandParseResult { isSuccessful = true, message = $"Spell '{commandParts[1]}' has been unlocked." };
				} else {
					return new CommandParseResult { isSuccessful = false, message = $"Invalid parameter, spell '{commandParts[1]}' is not known." };
				}
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		}));
		// speed - change maximum speed, available only in Race
		commands.Add(new CheatCommand("speed", "Changes the maximum speed. Usage: 'speed <value>', e.g. 'speed 30', 'speed 10'.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is required.";
			else if (!int.TryParse(commandParts[1], out int value)) message = "Invalid parameter, an integer is required.";
			// Perform the command
			else {
				CharacterMovementController controller = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterMovementController>("Player");
				float oldValue = controller.GetMaxSpeed();
				controller.SetMaxSpeed(value / CharacterMovementController.MAX_SPEED);
				success = true;
				message = $"The maximum speed has been changed from {oldValue} to {controller.GetMaxSpeed()}.";
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		},
		enabledScenes: new Scene[] { Scene.Race }));
		// TODO: start - quick race start, available only in Race
		// TODO: finish - quick race finish, available only in Race
	}

	private void Update() {
		if (cheatsEnabled) {
			// Handle input
			// ... show/hide cheats
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.C) && // all keys are pressed
				(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.C))) { // and one of them was pressed just now
				ToggleVisibility();
			}
			// ... enter cheat
			if (Input.GetKeyDown(KeyCode.Return)) { // Enter
				OnCommandEntered();
			}
			// ... history
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
				HandleCommandHistory();
			}
		}
	}

	#region SINGLETON
	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		commandHistory = new string[historyLength];
		lastCommandIndex = historyLength - 1;
		historyOffset = -1;
		// Initialize commands
		InitializeCommandList();
		commandsDictionary = new Dictionary<string, CheatCommand>();
		foreach (var command in commands) {
			commandsDictionary.Add(command.commandName, command);
		}
	}

	protected override void SetSingletonOptions() {
		Options = (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances;
	}
	#endregion

}

public class CheatCommand {
	public string commandName;
	public Func<string[], CommandParseResult> parseCommand;
	public string helpMessage;
	public Scene[] enabledInScenes; // scenes in which the command is available, all of them by default (if null)
	public Scene[] disabledInScenes; // scenes in which the command is not available, none by default (if null)

	public CheatCommand(string name, string helpMessage, Func<string[], CommandParseResult> parsing, Scene[] enabledScenes = null, Scene[] disabledScenes = null) {
		this.commandName = name;
		this.parseCommand = parsing;
		this.helpMessage = helpMessage;

		this.enabledInScenes = enabledScenes;
		this.disabledInScenes = disabledScenes;
	}

	public bool IsAvailable() {
		string currentScene = SceneLoader.Instance.currentScene;
		// Check if it is disabled in the current scene
		if (disabledInScenes != null) {
			foreach (var scene in disabledInScenes)
				if (scene.ToString() == currentScene) return false;
		}
		// Check if the current scene is among the scenes the command is enabled in
		if (enabledInScenes != null) {
			foreach (var scene in enabledInScenes)
				if (scene.ToString() == currentScene) return true;
			return false;
		}
		// Otherwise it is not disabled and is enabled in all the scenes
		return true;
	}
}

public class CommandParseResult {
	public bool isSuccessful;
	public string message;
}