using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// A class representing state throughout the whole gameplay
public class PlayerState : MonoBehaviourSingleton<PlayerState>, ISingleton {

    #region Progress
    private bool gameComplete = false;
    public bool GameComplete {
        get => gameComplete;
        set {
            gameComplete = value;
            SaveSystem.SaveGameComplete(gameComplete);
        }
    }
	#endregion

	#region Statistics
	public PlayerStats PreviousStats { get; set; } = new PlayerStats();

    private PlayerStats currentStats = new PlayerStats();
    public PlayerStats CurrentStats {
        get => currentStats;
        set { // Automatically store the previous stats when assigning new ones
            PreviousStats = currentStats;
            currentStats = value.ClampedToRange();
            // Save the values into a file
            SaveSystem.SavePlayerStatistics(PreviousStats, currentStats);
            // Notify anyone interested that the current stats are different
            Messaging.SendMessage("StatsChanged");
        }
    }
    #endregion

    #region Character Customization
    public CharacterCustomizationOptions customizationOptions; // set in the Inspector

    private CharacterCustomizationData characterCustomization = null;
    public CharacterCustomizationData CharacterCustomization {
        get {
            if (characterCustomization != null)
                return characterCustomization;
            else {
                // Load it from a file and cache the result
                characterCustomization = new CharacterCustomizationData();
                CharacterCustomizationSaveData characterSaveData = SaveSystem.LoadCharacterCustomization();
                if (characterSaveData != null)
                    characterCustomization.LoadFromSaveData(characterSaveData, customizationOptions);
                else
                    characterCustomization.InitializeToDefaultValues(customizationOptions);
                return characterCustomization;
            }
        }
        set {
            // Save the changes into a file
            characterCustomization = value;
            SaveSystem.SaveCharacterCustomization(characterCustomization.GetSaveData());
        }
    }
    #endregion

    #region Coins
    public int Coins { get; private set; } = 0;
    public Action<int, int> onCoinsAmountChanged; // callback invoked whenever the amount of coins changes, parameters are old amount and new amount

    // Returns true if the transaction could be performed (there was enough coins)
    public bool ChangeCoinsAmount(int delta) {
        int newAmount = Coins + delta;
        if (newAmount < 0) return false;
        if (newAmount > 999_999) newAmount = 999_999; // cannot go over 999 999
        int oldAmount = Coins;
        Coins = newAmount;
        // Save the value into a file
        SaveSystem.SaveCoins(Coins);
        // Notify anyone interested that the coins amount changed
        onCoinsAmountChanged?.Invoke(oldAmount, newAmount);
        Messaging.SendMessage("CoinsChanged", newAmount - oldAmount);
        return true;
    }
    #endregion

    #region Spells
    [HideInInspector] public Spell[] equippedSpells; // spells assigned to slots
    [HideInInspector] public Dictionary<string, bool> spellAvailability; // whether the spell is unlocked (purchased) or not
    [HideInInspector] public Dictionary<string, bool> spellCast; // whether the spell has been used already or never
    [HideInInspector] public int availableSpellCount = 0; // number of purchased spells

    public void EquipSpell(Spell spell, int slotIndex) {
        equippedSpells[slotIndex] = spell;
        // Save value into a file
        SaveSystem.SaveEquippedSpells(this.equippedSpells);
    }

    public bool IsSpellPurchased(string spellIdentifier) {
        return (spellAvailability.TryGetValue(spellIdentifier, out bool isPurchased) && isPurchased);
    }

    public void UnlockSpell(string spellIdentifier) {
        if (!IsSpellPurchased(spellIdentifier))
            availableSpellCount++;
        spellAvailability[spellIdentifier] = true;
        // Check if all spells are purchased
        bool allUnlocked = true;
        foreach (var spell in spellAvailability) {
            if (!spell.Value) {
                allUnlocked = false;
                break;
            }
        }
        // Save value into a file
        SaveSystem.SavePurchasedSpells(this.spellAvailability);
        // Notify anyone interested that all the spells have been purchased
        if (allUnlocked) Messaging.SendMessage("AllSpellsPurchased");
    }

    public void MarkSpellAsUsed(string spellIdentifier) {
        bool isKnown = spellCast.ContainsKey(spellIdentifier);
        if (!isKnown || (isKnown && !spellCast[spellIdentifier])) {
            spellCast[spellIdentifier] = true;
            // Save value into a file
            SaveSystem.SaveCastedSpells(spellCast);
        }
    }
    #endregion

    #region Broom Upgrades
    [HideInInspector] public float maxAltitude = 15f; // Maximum Y coordinate the player can fly up to
    private Dictionary<string, Tuple<int, int>> broomUpgradeLevels = new Dictionary<string, Tuple<int, int>>(); // current and maximum level for each upgrade
    private bool broomUpgradesLoaded = false; // if the data is loaded from a saved state

    // Returns the highest purchased level of the given broom upgrade
    // Or -1 if the given broom upgrade is not known
    public int GetBroomUpgradeLevel(string upgradeName) {
        // Load from the saved state if necessary - enables loading it even before the whole PlayerState(e.g. in the main menu)
        if (!broomUpgradesLoaded) {
            BroomUpgradesSaveData broomUpgrades = SaveSystem.LoadBroomUpgrades();
            LoadFromSavedBroomUpgrades(broomUpgrades);
        }
        if (broomUpgradeLevels.ContainsKey(upgradeName))
            return broomUpgradeLevels[upgradeName].Item1;
        else {
            return -1;
        }
    }

    // Saves the given level as the highest purchased one for the given broom upgrade
    public void SetBroomUpgradeLevel(string upgradeName, int level, int maxLevel) {
        broomUpgradeLevels[upgradeName] = new Tuple<int, int>(level, maxLevel);
        // Check if all upgrades are purchased
        bool allMax = true;
        foreach (var upgrade in broomUpgradeLevels) {
            if (upgrade.Value.Item1 != upgrade.Value.Item2) { // current level is not max level
                allMax = false;
                break;
            }
        }
        // Save the value into a file
        SaveSystem.SaveBroomUpgrades(new BroomUpgradesSaveData { UpgradeLevels = this.broomUpgradeLevels });
        // Notify anyone interested that the broom has been upgraded maximally
        if (allMax) Messaging.SendMessage("AllBroomUpgrades");
    }
    #endregion

    #region Opponents
    public Dictionary<int, string> knownOpponents; // stored names of opponents already visible in the leaderboard (according to their place)
	#endregion

	#region Mana + available regions
    [field: SerializeField]
    [Tooltip("The maximum amount of mana the player can have at once.")]
    public int MaxManaAmount { get; private set; } = 100;

    public Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>(); // not persistently stored, recomputed whenever a level is generated

    public void SetRegionAvailability(LevelRegionType region, bool availability) {
        if (regionsAvailability.ContainsKey(region) && availability && !regionsAvailability[region]) { // a new region became available
            // Notify anyone interested that a new region has been unlocked
            Messaging.SendMessage("NewRegionAvailable");
        }
        regionsAvailability[region] = availability;
    }
    #endregion


    public void SaveCurrentState() {
        // Use SaveSystem to save all the player's state
        // ...character customization is saved automatically in its setter
        // ...player state
        SaveSystem.SavePlayerState(new PlayerStateSaveData {
            gameComplete = this.GameComplete,
            stats = new StatisticsSaveData { 
                previousStats = this.PreviousStats,
                currentStats = this.currentStats
            },
            coins = this.Coins,
            KnownOpponents = this.knownOpponents
        });
        // ...broom upgrades
        SaveSystem.SaveBroomUpgrades(new BroomUpgradesSaveData { UpgradeLevels = this.broomUpgradeLevels });
        // ...purchased, equipped and used spells
        SaveSystem.SaveSpells(new SpellsSaveData { 
            EquippedSpells = this.equippedSpells,
            SpellsAvailability = this.spellAvailability,
            SpellsUsage = this.spellCast
        });
        // Save AchievementManager data
        AchievementManager.Instance.SaveAchievementsProgress();
    }

    public void LoadSavedState() {
        // Use SaveSystem to load all the player's state
        // ...character customization is loaded automatically in its getter
        // ...player state
        PlayerStateSaveData playerState = SaveSystem.LoadPlayerState();
        LoadFromSavedPlayerState(playerState);
        // ...broom upgrades
        BroomUpgradesSaveData broomUpgrades = SaveSystem.LoadBroomUpgrades();
        LoadFromSavedBroomUpgrades(broomUpgrades);
        // ...purchased, equipped and used spells
        SpellsSaveData spells = SaveSystem.LoadSpells();
        LoadFromSavedSpells(spells);
        // Load AchievementManager data
        AchievementManager.Instance.LoadAchievementsProgress();
    }

	public void ResetState() {
        // Initialize everything in player's state to default values
        InitializeSingleton();
        broomUpgradesLoaded = true;
        // Reset AchievementManager data
        AchievementManager.Instance.ResetAchievementsProgress();
        // Save the default state
        SaveCurrentState();
    }

    private void LoadFromSavedPlayerState(PlayerStateSaveData savedState) {
        if (savedState != null) {
            GameComplete = savedState.gameComplete;

            CurrentStats = savedState.stats.previousStats;
            CurrentStats = savedState.stats.currentStats;

            Coins = savedState.coins;

            knownOpponents = savedState.KnownOpponents;
        }
    }

    private void LoadFromSavedBroomUpgrades(BroomUpgradesSaveData broomUpgrades) {
        if (broomUpgrades != null && broomUpgrades.UpgradeLevels != null) {
            this.broomUpgradeLevels = broomUpgrades.UpgradeLevels;
            broomUpgradesLoaded = true;
        }
    }

    private void LoadFromSavedSpells(SpellsSaveData spells) {
        if (spells != null) { 
            if (spells.EquippedSpells != null) this.equippedSpells = spells.EquippedSpells;
            availableSpellCount = 0;
            if (spells.SpellsAvailability != null) { 
                this.spellAvailability = spells.SpellsAvailability;
                foreach (var isSpellAvailable in this.spellAvailability.Values) {
                    if (isSpellAvailable) availableSpellCount++;
                }
            }
            if (spells.SpellsUsage != null) this.spellCast = spells.SpellsUsage;
        }
    }


    #region Singleton
    static PlayerState() { 
        Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.LazyInitialization | SingletonOptions.RemoveRedundantInstances;
    }
    public void AwakeSingleton() {
    }

    public void InitializeSingleton() {
        // Initialize everything to default values
        // ...progress
        gameComplete = false;
        // ...statistics
        PreviousStats = new PlayerStats();
        currentStats = new PlayerStats();
        // ...character customization
        characterCustomization = null;
        // ...coins
        Coins = 0;
        // ...spells
        equippedSpells = new Spell[4];
        spellAvailability = new Dictionary<string, bool>();
        spellCast = new Dictionary<string, bool>();
        foreach (var spell in SpellManager.Instance.AllSpells) { // Initialize all spells to false
            spellAvailability[spell.Identifier] = false;
            spellCast[spell.Identifier] = false;
        }
        availableSpellCount = 0;
        // ...broom upgrades
        maxAltitude = 15f;
        broomUpgradeLevels = new Dictionary<string, Tuple<int, int>>();
        broomUpgradesLoaded = false;
        // ...opponents
        knownOpponents = new Dictionary<int, string>();
        // ...available regions
        regionsAvailability = new Dictionary<LevelRegionType, bool>();
    }
	#endregion

}

[System.Serializable]
public struct PlayerStats {
    [Range(0, 100)]
    public int endurance;
    [Range(0, 100)]
    public int speed;
    [Range(0, 100)]
    public int dexterity;
    [Range(0, 100)]
    public int precision;
    [Range(0, 100)]
    public int magic;

    // Computes weighted average of the stats
    // weight 3: precision, speed (more important and objective)
    // weight 2: endurance, magic (artificially lowering the score at the beginning and not that important)
    // weight 1: dexterity (not precise)
    public float GetWeightedAverage() {
        return (3 * (precision + speed) + 2 * (endurance + magic) + 1 * (dexterity)) / 11f;
    }

    // Returns the values in a specific order
    public List<float> GetListOfValues() {
        return new List<float> { endurance, speed, dexterity, precision, magic };
    }

    public PlayerStats ClampedToRange() {
        return new PlayerStats {
            endurance = Mathf.Clamp(endurance, 0, 100),
            speed = Mathf.Clamp(speed, 0, 100),
            dexterity = Mathf.Clamp(dexterity, 0, 100),
            precision = Mathf.Clamp(precision, 0, 100),
            magic = Mathf.Clamp(magic, 0, 100)
        };
    }

    public PlayerStats GetComplement() {
        return new PlayerStats {
            endurance = 100 - endurance,
            speed = 100 - speed,
            dexterity = 100 - dexterity,
            precision = 100 - precision,
            magic = 100 - magic
        };
    }

	public override string ToString() {
        return $"(E: {endurance}, S: {speed}, D: {dexterity}, P: {precision}, M: {magic})";
	}

	// Returns stats names in a specific order
	public static List<string> GetListOfStatNames() {
        return new List<string> { "Endurance", "Speed", "Dexterity", "Precision", "Magic" };
    }

	#region Operator overloads
	public static PlayerStats operator +(PlayerStats a, PlayerStats b) {
        return new PlayerStats {
            endurance = a.endurance + b.endurance,
            speed = a.speed + b.speed,
            dexterity = a.dexterity + b.dexterity,
            precision = a.precision + b.precision,
            magic = a.magic + b.magic
        }.ClampedToRange();
    }

    public static PlayerStats operator -(PlayerStats a, PlayerStats b) {
        return new PlayerStats {
            endurance = a.endurance - b.endurance,
            speed = a.speed - b.speed,
            dexterity = a.dexterity - b.dexterity,
            precision = a.precision - b.precision,
            magic = a.magic - b.magic
        }.ClampedToRange();
    }

    public static PlayerStats operator *(PlayerStats a, float b) {
        return new PlayerStats {
            endurance = Mathf.RoundToInt(a.endurance * b),
            speed = Mathf.RoundToInt(a.speed * b),
            dexterity = Mathf.RoundToInt(a.dexterity * b),
            precision = Mathf.RoundToInt(a.precision * b),
            magic = Mathf.RoundToInt(a.magic * b)
        }.ClampedToRange();
    }

    public static PlayerStats operator *(float a, PlayerStats b) {
        return b * a;
    }

    public static PlayerStats operator /(PlayerStats a, float b) {
        if (b == 0) throw new DivideByZeroException();
        return new PlayerStats {
            endurance = Mathf.RoundToInt(a.endurance / b),
            speed = Mathf.RoundToInt(a.speed / b),
            dexterity = Mathf.RoundToInt(a.dexterity / b),
            precision = Mathf.RoundToInt(a.precision / b),
            magic = Mathf.RoundToInt(a.magic / b)
        }.ClampedToRange();
    }
    #endregion
}