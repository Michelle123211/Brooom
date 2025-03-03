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

	[Space]
	[Tooltip("Cheats which will be automatically processed when the game is started.")]
	[SerializeField] List<string> initializationCommands;
	[Tooltip("Cheats which will be automatically processed whenever the corresponding scene is loaded.")]
	[SerializeField] List<CheatsForSceneInitialization> sceneInitializationCommands;

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
			AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
			Utils.TweenAwareEnable(cheatsUI);
			cheatsAreVisible = true;
			// Focus on the input field
			commandField.Select();
			commandField.ActivateInputField();
		} else {
			// Hide the cheats UI
			AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
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

	#region Initialization
	private void InitializeCommandList() {
		commands = new List<CheatCommand>();

		// Basic commands
		InitializeSceneCommand(); // scene - change scene
		InitializeStatsCommand(); // stats - change player statistics values

		// Shop commands
		InitializeCoinsCommand(); // coins - change coins amount
		InitializeUpgradeCommand(); // upgrade - upgrade the broom, available in Main Menu, Tutorial, Player Overview, Race and Testing Track

		// Spell commands
		InitializeSpellCommand(); // spell - unlock all spells or only the given one
		InitializeManaCommand(); // mana - change mana amount or enable/disable unlimited mana, available only in Race
		InitializeRechargeCommand(); // recharge - change spells' charge or enable/disable cooldown, available only in Race

		// Race commands
		InitializeSpeedCommand(); // speed - change maximum speed, available only in Race and Testing Track
		InitializeStartCommand(); // start - quick race start, available only in Race
		InitializeFinishCommand(); // finish - quick race finish, available only in Race
	}

	private void InitializeSceneCommand() {
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
	}
	private void InitializeStatsCommand() {
		// stats - change player statistics values
		commands.Add(new CheatCommand("stats", "Changes values of individual player statistics or all of them at once. Statistics not specified will be left without change. Usage: 'stats all=<0-100>', e.g. 'stats all=40', or 'stats (<statLetter>=<0-100>){1,5}', e.g. 'stats m=45', 'stats e=83 p=21'.\nAvailable stats: e (endurance), s (speed), d (dexterity), p (precision), m (magic).", (commandParts) => {
			// Handle errors
			if (commandParts.Length < 2 || commandParts.Length > 6) {
				return new CommandParseResult {
					isSuccessful = false, message = "Invalid number of parameters, at least one is required and at most five."
				};
			} else {
				PlayerStats stats = PlayerState.Instance.CurrentStats;
				string message = string.Empty;
				// Parse parameters to set all stats at once
				if (commandParts.Length == 2) {
					string[] parameterParts = commandParts[1].Trim().Split('=', StringSplitOptions.RemoveEmptyEntries);
					if (parameterParts.Length != 2)
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter. All parameters must be in a form of <parameterName>=<value>."
						};
					if (parameterParts[0] == "all") {
						if (!int.TryParse(parameterParts[1], out int parameterValue))
							return new CommandParseResult {
								isSuccessful = false, message = "Invalid parameter, an integer is required for the stat value."
							};
						if (parameterValue < 0 || parameterValue > 100)
							return new CommandParseResult {
								isSuccessful = false, message = "Invalid parameter, stat value must be between 0 and 100 (inclusive)."
							};
						// Everything was all right
						message = $"Player stats have been changed from ({PlayerState.Instance.CurrentStats.endurance}, {PlayerState.Instance.CurrentStats.speed}, {PlayerState.Instance.CurrentStats.dexterity}, {PlayerState.Instance.CurrentStats.precision}, {PlayerState.Instance.CurrentStats.magic}).";
						PlayerState.Instance.CurrentStats = new PlayerStats { endurance = parameterValue, speed = parameterValue, dexterity = parameterValue, precision = parameterValue, magic = parameterValue };
						message += $" to ({PlayerState.Instance.CurrentStats.endurance}, {PlayerState.Instance.CurrentStats.speed}, {PlayerState.Instance.CurrentStats.dexterity}, {PlayerState.Instance.CurrentStats.precision}, {PlayerState.Instance.CurrentStats.magic}).";
						return new CommandParseResult {
							isSuccessful = true,
							message = message
						};
					}
				}
				// Parse individual stats and try to override the values
				for (int i = 1; i < commandParts.Length; i++) {
					string[] statParts = commandParts[i].Trim().Split('=', StringSplitOptions.RemoveEmptyEntries);
					if (statParts.Length != 2)
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter. All parameters must be in a form of <parameterName>=<value>."
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
				message = $"Player stats have been changed from ({PlayerState.Instance.CurrentStats.endurance}, {PlayerState.Instance.CurrentStats.speed}, {PlayerState.Instance.CurrentStats.dexterity}, {PlayerState.Instance.CurrentStats.precision}, {PlayerState.Instance.CurrentStats.magic}).";
				PlayerState.Instance.CurrentStats = stats;
				message += $" to ({PlayerState.Instance.CurrentStats.endurance}, {PlayerState.Instance.CurrentStats.speed}, {PlayerState.Instance.CurrentStats.dexterity}, {PlayerState.Instance.CurrentStats.precision}, {PlayerState.Instance.CurrentStats.magic}).";
				return new CommandParseResult {
					isSuccessful = true,
					message = message
				};
			}
		}));
	}

	private void InitializeCoinsCommand() {
		// coins - change coins amount
		commands.Add(new CheatCommand("coins", "Adds this amount of coins to the current amount. Usage: 'coins <amount>', e.g. 'coins 1000', 'coins -100'.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is required.";
			else if (!int.TryParse(commandParts[1], out int amount)) message = "Invalid parameter, an integer is required.";
			// Perform the command
			else {
				int oldValue = PlayerState.Instance.Coins;
				PlayerState.Instance.ChangeCoinsAmount(amount);
				success = true;
				message = $"The amount of coins has been changed from {oldValue} to {PlayerState.Instance.Coins}.";
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		}));
	}
	private void InitializeUpgradeCommand() {
		// upgrade - upgrade the broom
		commands.Add(new CheatCommand("upgrade", "Unlocks all broom upgrades or only levels up the given one. Usage: 'upgrade all' or 'upgrade <upgradeIdentifier>', e.g. 'upgrade Speed'.\nAvailable upgrades: Speed, Control, Elevation.", (commandParts) => {
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
	}

	private void InitializeSpellCommand() {
		// spell - unlock all spells or only the given one
		// Get list of available spells
		StringBuilder helpMessage = new StringBuilder($"Unlocks all spells or only the given one. Usage: 'spell all' or 'spell <spellIdentifier>', e.g. 'spell {SpellManager.Instance.AllSpells[0].Identifier}'.\nAvailable spells: ");
		for (int i = 0; i < SpellManager.Instance.AllSpells.Count; i++) {
			helpMessage.Append(SpellManager.Instance.AllSpells[i].Identifier);
			if (i < SpellManager.Instance.AllSpells.Count - 1)
				helpMessage.Append(", ");
		}
		helpMessage.Append(".");
		// Add command
		commands.Add(new CheatCommand("spell", helpMessage.ToString(), (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is needed.";
			else if (commandParts[1] == "all") {
				// Unlock all spells
				foreach (var spell in SpellManager.Instance.AllSpells) {
					PlayerState.Instance.UnlockSpell(spell.Identifier);
				}
				return new CommandParseResult { isSuccessful = true, message = "All spells have been unlocked." };
			} else {
				// Unlock only the given spell
				if (SpellManager.Instance.CheckIfSpellExists(commandParts[1])) { // if the spell exists
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
	}
	private void FillManaUp() {
		SpellController playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
		if (playerSpellController.CurrentMana < playerSpellController.MaxMana) {
			playerSpellController.ChangeManaAmount(playerSpellController.MaxMana - playerSpellController.CurrentMana);
		}
	}
	private void InitializeManaCommand() {
		// mana - change mana amount or enable/disable unlimited mana, available only in Race
		void ScheduleFillingManaUp(int _) { // the parameter must be there because this method is used as a callback on mana value change
			// Schedule filling mana up so that it is done independently of the current callbacks being invoked
			SpellController playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
			Invoke(nameof(FillManaUp), 0.1f);
		}
		commands.Add(new CheatCommand("mana", "Changes mana amount or enables/disables unlimited mana. Usage: 'mana <amount>' for a specific amount, e.g. 'mana 80', or 'mana max' for maximum amount, or 'mana off' or 'mana on' to enable/disable unlimited mana.",
			(commandParts) => {
				bool success = false;
				string message = string.Empty;
				SpellController playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
				// Handle errors
				if (commandParts.Length != 2) message = "Invalid number of parameters, one is needed.";
				else if (playerSpellController == null) message = "A suitable target has not been found.";
				else if (commandParts[1] == "on") {
					// Disable unlimited mana
					playerSpellController.onManaAmountChanged -= ScheduleFillingManaUp;
					return new CommandParseResult { isSuccessful = true, message = "Unlimited mana has been disabled." };
				} else if (commandParts[1] == "off") {
					// Enable unlimited mana
					playerSpellController.onManaAmountChanged -= ScheduleFillingManaUp; // try to unregister in case it was already registered
					playerSpellController.onManaAmountChanged += ScheduleFillingManaUp;
					FillManaUp();
					return new CommandParseResult { isSuccessful = true, message = "Unlimited mana has been enabled." };
				} else if (commandParts[1] == "max") {
					// Fill mana up
					FillManaUp();
					return new CommandParseResult { isSuccessful = true, message = "Mana has been filled up." };
				} else {
					// Change mana amount
					if (int.TryParse(commandParts[1], out int manaAmount)) {
						playerSpellController.ChangeManaAmount(manaAmount - playerSpellController.CurrentMana);
						return new CommandParseResult { isSuccessful = true, message = $"Mana amount has been changed to {manaAmount}." };
					} else {
						return new CommandParseResult { isSuccessful = false, message = $"Invalid parameter '{commandParts[1]}', amount must be an integer." };
					}
				}
				// Return the result
				return new CommandParseResult {
					isSuccessful = success,
					message = message
				};
			},
			enabledScenes: new Scene[] { Scene.Race }));
	}
	private void InitializeRechargeCommand() {
		// recharge - change spells' charge or enable/disable cooldown, available only in Race
		void RechargeAllSpells(int _) { // the parameter must be there because this method is used as a callback on spell casted
			SpellController playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
			playerSpellController.RechargeAllSpells();
		}
		commands.Add(new CheatCommand("recharge", "Changes spells' charge or enables/disables cooldown. Usage: 'recharge all' to immediately recharge all spells, or 'recharge on' or 'recharge off' to enable/disable spells' cooldown.",
			(commandParts) => {
				bool success = false;
				string message = string.Empty;
				SpellController playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
				// Handle errors
				if (commandParts.Length != 2) message = "Invalid number of parameters, one is needed.";
				else if (playerSpellController == null) message = "A suitable target has not been found.";
				else if (commandParts[1] == "all") {
					// Immediately recharge all spells
					playerSpellController.RechargeAllSpells();
					return new CommandParseResult { isSuccessful = true, message = "All spells have been recharged." };
				} else if (commandParts[1] == "on") {
					// Enable spells' cooldown
					playerSpellController.onSpellCasted -= RechargeAllSpells;
					return new CommandParseResult { isSuccessful = true, message = "Spells' cooldown has been enabled." };
				} else if (commandParts[1] == "off") {
					// Disable spells' cooldown
					playerSpellController.onSpellCasted -= RechargeAllSpells; // try to unregister in case it wal already registered
					playerSpellController.onSpellCasted += RechargeAllSpells;
					playerSpellController.RechargeAllSpells();
					return new CommandParseResult { isSuccessful = true, message = "Spells' cooldown has been disabled." };
				} else {
					return new CommandParseResult { isSuccessful = false, message = $"Invalid parameter '{commandParts[1]}', only 'all', 'on' and 'off' can be used." };
				}
				// Return the result
				return new CommandParseResult {
					isSuccessful = success,
					message = message
				};
			},
			enabledScenes: new Scene[] { Scene.Race }));
	}

	private void InitializeSpeedCommand() {
		// speed - change maximum speed, available only in Race
		commands.Add(new CheatCommand("speed", "Changes the maximum speed. Usage: 'speed <value>', e.g. 'speed 30', 'speed 10'.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 2) message = "Invalid number of parameters, one is required.";
			else if (!int.TryParse(commandParts[1], out int value)) message = "Invalid parameter, an integer is required.";
			else if (value < 0) message = "Invalid parameter, the value must be non-negative.";
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
		enabledScenes: new Scene[] { Scene.Race, Scene.TestingTrack }));
	}
	private void InitializeStartCommand() {
		// start - quick race start, available only in Race
		commands.Add(new CheatCommand("start", "Immediately goes from training to race. Usage: 'start'.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length != 1) message = "Invalid number of parameters, there should be none.";
			else {
				if (RaceController.Instance.State != RaceState.Training) {
					message = "The race has already started.";
				} else {
					// Perform the command
					RaceController.Instance.StartRace();
					Destroy(FindObjectOfType<StartingZone>().transform.parent.gameObject);
					success = true;
					message = "The race is starting";
				}
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		},
		enabledScenes: new Scene[] { Scene.Race }));
	}
	private void InitializeFinishCommand() {
		// finish - quick race finish, available only in Race
		commands.Add(new CheatCommand("finish", "Immediately finishes the race with the given parameters. Usage: 'finish time=<finishTimeInSeconds> (missed=<numberOfHoopsMissed>)?', e.g. 'finish time=65', 'finish time=137 missed=3'.", (commandParts) => {
			bool success = false;
			string message = string.Empty;
			// Handle errors
			if (commandParts.Length < 2 || commandParts.Length > 3) message = "Invalid number of parameters, there should be at least one and at most two.";
			else if (RaceController.Instance.State != RaceState.RaceInProgress)
				message = "The race has not started yet and therefore cannot be finished.";
			else {
				int time = -1;
				int missedHoops = 0;
				for (int i = 1; i < commandParts.Length; i++) {
					string[] parameterParts = commandParts[i].Trim().Split('=', StringSplitOptions.RemoveEmptyEntries);
					if (parameterParts.Length != 2)
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter. All parameters must be in a form of <parameterName>=<value>."
						};
					if (!int.TryParse(parameterParts[1], out int parameterValue))
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter, an integer is required for the parameter value."
						};
					if (parameterValue < 0)
						return new CommandParseResult {
							isSuccessful = false, message = "Invalid parameter, the value must be non-negative."
						};
					if (parameterParts[0] == "time") {
						time = parameterValue;
					} else if (parameterParts[0] == "missed") {
						missedHoops = parameterValue;
					} else {
						return new CommandParseResult {
							isSuccessful = false, message = $"Invalid parameter, the parameter '{parameterParts[0]}' is not known. Available parameters: time, missed."
						};
					}
				}
				if (time < 0)
					return new CommandParseResult {
						isSuccessful = false, message = $"Invalid parameter, the 'time' parameter must be specified."
					};
				// Perform the command
				CharacterRaceState playerState = RaceController.Instance.playerRacer.state;
				// Distribute the missed hoops but pass all checkpoints
				int actualMissedHoops = 0;
				for (int i = 0; i < playerState.hoopsPassedArray.Length; i++) {
					if (RaceController.Instance.level.track[i].isCheckpoint || missedHoops == 0) {
						playerState.hoopsPassedArray[i] = true;
					} else {
						missedHoops--;
						actualMissedHoops++;
					}
				}
				// Set time and penalization
				int penalization = Mathf.RoundToInt(RaceController.Instance.missedHoopPenalization * actualMissedHoops);
				playerState.finishTime = time;
				playerState.timePenalization = penalization;
				// End race
				RaceController.Instance.EndRace();
				success = true;
				message = $"The race is finishing with finish time {time} s and {actualMissedHoops} missed hoops (resulting in penalization {penalization} s).";
			}
			// Return the result
			return new CommandParseResult {
				isSuccessful = success,
				message = message
			};
		},
		enabledScenes: new Scene[] { Scene.Race }));
	}
	#endregion

	private void ProcessInitializationCommands() {
		foreach (var command in initializationCommands) {
			ProcessCommand(command);
		}
		Utils.TweenAwareDisable(messageArea); // hide message area
	}

	private void ProcessSceneInitializationCommands(Scene currentScene) {
		// Process commands for the current scene
		foreach (var sceneCommands in sceneInitializationCommands) {
			if (sceneCommands.scene == currentScene) {
				foreach (var command in sceneCommands.commands) {
					ProcessCommand(command);
				}
			}
		}
		Utils.TweenAwareDisable(messageArea); // hide message area
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
				AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.Click);
				OnCommandEntered();
			}
			// ... history
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
				AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.Click);
				HandleCommandHistory();
			}
		}
	}

	#region SINGLETON
	static Cheats() { 
		Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances;
	}

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
		// Register callbacks
		Messaging.RegisterForMessage("GameStarted", ProcessInitializationCommands);
		SceneLoader.Instance.onSceneLoaded += ProcessSceneInitializationCommands;
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
		// Check if it is disabled in the current scene
		if (disabledInScenes != null) {
			foreach (var scene in disabledInScenes)
				if (scene == SceneLoader.Instance.CurrentScene) return false;
		}
		// Check if the current scene is among the scenes the command is enabled in
		if (enabledInScenes != null) {
			foreach (var scene in enabledInScenes)
				if (scene == SceneLoader.Instance.CurrentScene) return true;
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

[System.Serializable]
public struct CheatsForSceneInitialization {
	public Scene scene;
	public List<string> commands;
}