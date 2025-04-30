using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// A singleton representing the core game state throughout the whole gameplay.
/// It contains data such as e.g. current stats, selected character customization, number of coins, 
/// equipped spells, broom upgrade levels, visited regions, etc.
/// These data are persistently stored in a file, and then loaded again when necessary.
/// </summary>
public class PlayerState : MonoBehaviourSingleton<PlayerState>, ISingleton {

    #region Progress
    private bool gameComplete = false;
    /// <summary>Whether the game has been completed (i.e. the player was on first place in the leaderboard).</summary>
    public bool GameComplete {
        get => gameComplete;
        set {
            gameComplete = value;
            SaveSystem.SaveGameComplete(gameComplete);
        }
    }
	#endregion

	#region Statistics
    /// <summary>Previous stats values.</summary>
	public PlayerStats PreviousStats { get; set; } = new PlayerStats();

    private PlayerStats currentStats = new PlayerStats();
    /// <summary>Current stats values.</summary>
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
    /// <summary>All different character customization options available in the game.</summary>
    public CharacterCustomizationOptions customizationOptions; // set in the Inspector

    private CharacterCustomizationData characterCustomization = null;
    /// <summary>Currently selected character customization options.</summary>
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
    /// <summary>Current amount of coins.</summary>
    public int Coins { get; private set; } = 0;
    /// <summary>Invoked whenever the amount of coins changes. Parameters are the old amount and the new amount.</summary>
    public event Action<int, int> onCoinsAmountChanged;

    // Returns true if the transaction could be performed (there was enough coins)
    /// <summary>
    /// Tries to change the coins amount, i.e. increases or decreases the current coins amount by the given delta.
    /// </summary>
    /// <param name="delta">Change in the coins amount (can be positive or negative).</param>
    /// <returns><c>true</c> if the transaction was successful, <c>false</c> otherwise (i.e. there was not enough coins and the result would be negative value).</returns>
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
    /// <summary>Spells assigned to individual slots, i.e. spells equipped to a race.</summary>
    [HideInInspector] public Spell[] equippedSpells;
    /// <summary>Invoked when a spell is assigned to a slot, or a slot is emptied out.</summary>
    public event Action<Spell, int> onEquippedSpellChanged;
    /// <summary>For each spell identifier there is a <c>bool</c> indicating whether the spell has been unlocked/purchased already, or not.</summary>
    [HideInInspector] public Dictionary<string, bool> spellAvailability;
    /// <summary>For each spell identifier there is a <c>bool</c> indicating whether the spell has been cast already, or not.</summary>
    [HideInInspector] public Dictionary<string, bool> spellCast;
    /// <summary>Total number of purchased spells.</summary>
    [HideInInspector] public int availableSpellCount = 0;

    /// <summary>
    /// Assigns the given spell to the slot on the given index.
    /// </summary>
    /// <param name="spell">A spell to be assigned to the slot, or <c>null</c> to empty the slot.</param>
    /// <param name="slotIndex">Index of the slot to which the spell should be assigned.</param>
    public void EquipSpell(Spell spell, int slotIndex) {
        equippedSpells[slotIndex] = spell;
        onEquippedSpellChanged?.Invoke(spell, slotIndex);
        // Save value into a file
        SaveSystem.SaveEquippedSpells(this.equippedSpells);
    }
    /// <summary>
    /// Empties the slot to which the spell with the given identifier is assigned. If there is no such slot, nothing happens.
    /// </summary>
    /// <param name="spellIdentifier">Identifier of the spell to be unequipped.</param>
    public void UnequipSpell(string spellIdentifier) {
        for (int i = 0; i < equippedSpells.Length; i++) {
            if (equippedSpells[i] != null && !string.IsNullOrEmpty(equippedSpells[i].Identifier) && equippedSpells[i].Identifier == spellIdentifier) {
                equippedSpells[i] = null;
            }
        }
        // Save value into a file
        SaveSystem.SaveEquippedSpells(this.equippedSpells);
    }

    /// <summary>
    /// Checks whether the player has at least one equipped spell.
    /// </summary>
    /// <returns><c>true</c> if the player has at least one equipped spell, <c>false</c> otherwise.</returns>
    public bool HasEquippedSpells() {
        foreach (var equippedSpell in equippedSpells) {
            if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier)) return true;
        }
        return false;
    }

    /// <summary>
    /// Checks whether the spell with the given identifier has been purchased already.
    /// </summary>
    /// <param name="spellIdentifier">Identifier of the spell to be checked.</param>
    /// <returns><c>true</c> if the spell is purchased, <c>false</c> otherwise.</returns>
    public bool IsSpellPurchased(string spellIdentifier) {
        return (spellAvailability.TryGetValue(spellIdentifier, out bool isPurchased) && isPurchased);
    }

    /// <summary>
    /// Unlocks the spell with the given identifier (i.e. makes it available).
    /// </summary>
    /// <param name="spellIdentifier">Identifier of the spell to be unlocked.</param>
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

    /// <summary>
    /// Locks the spell with the given identifier (i.e. makes it unavailable).
    /// </summary>
    /// <param name="spellIdentifier">Identifier of the spell to be locked.</param>
    public void LockSpell(string spellIdentifier) {
        if (IsSpellPurchased(spellIdentifier))
            availableSpellCount--;
        spellAvailability[spellIdentifier] = false;
        UnequipSpell(spellIdentifier);
        // Save value into a file
        SaveSystem.SavePurchasedSpells(this.spellAvailability);
    }

    /// <summary>
    /// Marks the spell with the given identifier as not used (i.e. not cast yet).
    /// </summary>
    /// <param name="spellIdentifier">Identifier of the spell.</param>
    public void MarkSpellAsUsed(string spellIdentifier) {
        bool isKnown = spellCast.ContainsKey(spellIdentifier);
        if (!isKnown || (isKnown && !spellCast[spellIdentifier])) {
            spellCast[spellIdentifier] = true;
            // Save value into a file
            SaveSystem.SaveCastSpells(spellCast);
        }
    }
    #endregion

    #region Broom Upgrades
    /// <summary>Maximum Y coordinate the player can fly up to. It is affected by the Elevation broom upgrade's level.</summary>
    [HideInInspector] public float maxAltitude = 15f;
    /// <summary>For each broom upgrade there is a pair of values indicating the current upgrade level and the maximum upgrade level.</summary>
    [HideInInspector] public Dictionary<string, (int currentLevel, int maxLevel)> BroomUpgradeLevels { get; private set; } = new Dictionary<string, (int currentLevel, int maxLevel)>();
    private bool broomUpgradesLoaded = false; // if the data is loaded from a saved state

    /// <summary>
    /// Gets the current (i.e. the highest purchased) level of the given broom upgrade.
    /// </summary>
    /// <param name="upgradeName">Name of the broom upgrade.</param>
    /// <returns>Current level of the given broom upgrade, or <c>-1</c> if upgrade of the given name is not known.</returns>
    public int GetBroomUpgradeLevel(string upgradeName) {
        // Load from the saved state if necessary - enables loading it even before the whole PlayerState(e.g. in the main menu)
        if (!broomUpgradesLoaded) {
            BroomUpgradesSaveData broomUpgrades = SaveSystem.LoadBroomUpgrades();
            LoadFromSavedBroomUpgrades(broomUpgrades);
        }
        if (BroomUpgradeLevels.ContainsKey(upgradeName))
            return BroomUpgradeLevels[upgradeName].Item1;
        else {
            return -1;
        }
    }

    /// <summary>
    /// Sets the current level (i.e. the highest purchased) of the given broom upgrade to the given value.
    /// Maximum level is required so that it can be saved too in case there hasn't been any record of it previously.
    /// </summary>
    /// <param name="upgradeName">Name of the broom upgrade whose level should be set.</param>
    /// <param name="level">Level value to be set as the current value.</param>
    /// <param name="maxLevel">Maximum level of the broom upgrade.</param>
    public void SetBroomUpgradeLevel(string upgradeName, int level, int maxLevel) {
        BroomUpgradeLevels[upgradeName] = (level, maxLevel);
        // Check if all upgrades are purchased
        bool allMax = true;
        foreach (var upgrade in BroomUpgradeLevels) {
            if (upgrade.Value.currentLevel != upgrade.Value.maxLevel) { // current level is not max level
                allMax = false;
                break;
            }
        }
        // Save the value into a file
        SaveSystem.SaveBroomUpgrades(new BroomUpgradesSaveData { UpgradeLevels = this.BroomUpgradeLevels });
        // Notify anyone interested that the broom has been upgraded maximally
        if (allMax) Messaging.SendMessage("AllBroomUpgrades");
    }
    #endregion

    #region Opponents
    /// <summary>Stored names of opponents which were already visible in the leaderboard (according to their place). These names are used again next time.</summary>
    public Dictionary<int, string> knownOpponents;
	#endregion

	#region Visited regions
    /// <summary>
    /// For each region type there is a <c>bool</c> indicating whether it has been visited yet.
    /// </summary>
	public Dictionary<LevelRegionType, bool> regionsVisited = new Dictionary<LevelRegionType, bool>();

    /// <summary>
    /// Marks the given region as visited.
    /// </summary>
    /// <param name="region">Region to be marked as visited.</param>
    public void SetRegionVisited(LevelRegionType region) {
        if (!regionsVisited.ContainsKey(region) || !regionsVisited[region]) {
			regionsVisited[region] = true;
			// Notify anyone interested that a new region has been visited
			Messaging.SendMessage("NewRegionVisited", (int)region);
		}
		SaveSystem.SaveVisitedRegions(regionsVisited);
	}
    #endregion

    #region Mana + available regions - not stored persistently
    /// <summary>The maximum amount of mana the player (or any other racer) can have at once.</summary>
    [field: SerializeField]
    [field:Tooltip("The maximum amount of mana the player can have at once.")]
    public int MaxManaAmount { get; private set; } = 100;

    /// <summary>For each region there is a <c>bool</c> value indicating whether the region is available (based on its specific conditions). 
    /// These values are not stored presistently, they are recomputed whenever a level is being generated.</summary>
    public Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();

    /// <summary>
    /// Sets availability of the given region to the given value.
    /// </summary>
    /// <param name="region">Region whose availability is to be set.</param>
    /// <param name="availability"><c>true</c> if the region should be available, <c>false</c> if unavailable.</param>
    public void SetRegionAvailability(LevelRegionType region, bool availability) {
        if (availability && (!regionsAvailability.ContainsKey(region) || (regionsAvailability.ContainsKey(region) && !regionsAvailability[region]))) { // a new region became available
			regionsAvailability[region] = availability;
			// Notify anyone interested that a new region has been unlocked
			Messaging.SendMessage("NewRegionAvailable", (int)region);
        }
        regionsAvailability[region] = availability;
    }
    #endregion

    /// <summary>
    /// Persistently stores the current game state using a <c>SaveSystem</c>.
    /// </summary>
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
        SaveSystem.SaveBroomUpgrades(new BroomUpgradesSaveData { UpgradeLevels = this.BroomUpgradeLevels });
        // ...purchased, equipped and used spells
        SaveSystem.SaveSpells(new SpellsSaveData { 
            EquippedSpells = this.equippedSpells,
            SpellsAvailability = this.spellAvailability,
            SpellsUsage = this.spellCast
        });
        // ...visited regions
        SaveSystem.SaveRegions(new RegionsSaveData { RegionsVisited = this.regionsVisited });
        // Save AchievementManager data
        AchievementManager.Instance.SaveAchievementsProgress();
        // Save Tutorial data
        Tutorial.Instance.SaveCurrentProgress();
    }

    /// <summary>
    /// Loads current game state values from save files using a <c>SaveSystem</c>.
    /// </summary>
    public void LoadSavedState() {
        // Use SaveSystem to load all the player's state
        // ...character customization is loaded automatically in its getter
        characterCustomization = null;
        // ...player state
        PlayerStateSaveData playerState = SaveSystem.LoadPlayerState();
        LoadFromSavedPlayerState(playerState);
        // ...broom upgrades
        BroomUpgradesSaveData broomUpgrades = SaveSystem.LoadBroomUpgrades();
        LoadFromSavedBroomUpgrades(broomUpgrades);
        // ...purchased, equipped and used spells
        SpellsSaveData spells = SaveSystem.LoadSpells();
        LoadFromSavedSpells(spells);
        // ...visited regions
        RegionsSaveData regions = SaveSystem.LoadRegions();
        LoadFromSavedRegions(regions);
        // Load AchievementManager data
        AchievementManager.Instance.LoadAchievementsProgress();
        // Load Tutorial data
        Tutorial.Instance.LoadCurrentProgress();
    }

    /// <summary>
    /// Resets the game state to default values.
    /// </summary>
	public void ResetState() {
        // Initialize everything in player's state to default values
        InitializeSingleton();
        broomUpgradesLoaded = true;
        // Reset AchievementManager data
        AchievementManager.Instance.ResetAchievementsProgress();
        // Reset Tutorial data
        Tutorial.Instance.ResetCurrentProgress();
        // Save the default state
        SaveCurrentState();
    }

    // Sets player state values from the loaded save data
    private void LoadFromSavedPlayerState(PlayerStateSaveData savedState) {
        if (savedState != null) {
            GameComplete = savedState.gameComplete;

            CurrentStats = savedState.stats.previousStats;
            CurrentStats = savedState.stats.currentStats;

            Coins = savedState.coins;

            knownOpponents = savedState.KnownOpponents;
        }
    }

    // Sets broom upgrades values from the loaded save data
    private void LoadFromSavedBroomUpgrades(BroomUpgradesSaveData broomUpgrades) {
        if (broomUpgrades != null && broomUpgrades.UpgradeLevels != null) {
            this.BroomUpgradeLevels = broomUpgrades.UpgradeLevels;
            broomUpgradesLoaded = true;
        }
    }

    // Sets spells values from the loaded save data
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
            // Make sure any spells added later are also considered
            foreach (var spell in SpellManager.Instance.AllSpells) { // Initialize all spells to false
                if (!spellAvailability.ContainsKey(spell.Identifier))
                    spellAvailability[spell.Identifier] = false;
                if (!spellCast.ContainsKey(spell.Identifier))
                    spellCast[spell.Identifier] = false;
            }
        }
    }

    // Sets regions values from the loaded save data
    private void LoadFromSavedRegions(RegionsSaveData regions) {
        if (regions != null) { 
            if (regions.RegionsVisited != null) this.regionsVisited = regions.RegionsVisited;
        }
    }


    #region Singleton
    static PlayerState() {
        // Singleton options override
        Options = SingletonOptions.PersistentBetweenScenes | SingletonOptions.LazyInitialization | SingletonOptions.RemoveRedundantInstances;
    }

    /// <inheritdoc/>
    public void AwakeSingleton() {
    }

    /// <summary>
    /// Called when a new instance of the <c>PlayerState</c> singleton is created. It initializes everything to default values.
    /// </summary>
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
        BroomUpgradeLevels = new Dictionary<string, (int currentLevel, int maxLevel)>();
        broomUpgradesLoaded = false;
        // ...opponents
        knownOpponents = new Dictionary<int, string>();
        // ...available regions
        regionsAvailability = new Dictionary<LevelRegionType, bool>();
        regionsVisited = new Dictionary<LevelRegionType, bool>();
    }
	#endregion

}

/// <summary>
/// A struct containing values of all statistics used in the game to evaluate performance of the player (or any other racer).
/// </summary>
[System.Serializable]
public struct PlayerStats {
    /// <summary>Value of the Endurance stat (between 0 and 100).</summary>
    [Range(0, 100)]
    public int endurance;
    /// <summary>Value of the Speed stat (between 0 and 100).</summary>
    [Range(0, 100)]
    public int speed;
    /// <summary>Value of the Dexterity stat (between 0 and 100).</summary>
    [Range(0, 100)]
    public int dexterity;
    /// <summary>Value of the Precision stat (between 0 and 100).</summary>
    [Range(0, 100)]
    public int precision;
    /// <summary>Value of the Magic stat (between 0 and 100).</summary>
    [Range(0, 100)]
    public int magic;

    /// <summary>
    /// Computes weighted average of the stats. Precision and Speed are taken with weight 3 (more important and objective),
    /// Endurace and Magic with weight 2 (artificially lowering the score at the beginning and not that important),
    /// Dexterity with weight 1 (not very precise).
    /// </summary>
    /// <returns>A weighted average of all stats values.</returns>
    public float GetWeightedAverage() {
        return (3 * (precision + speed) + 2 * (endurance + magic) + 1 * (dexterity)) / 11f;
    }

    /// <summary>
    /// Creates a list of stats values in a specific order (i.e. Endurance, Speed, Dexterity, Precision, Magic).
    /// </summary>
    /// <returns>A list of stats values.</returns>
    public List<float> GetListOfValues() {
        return new List<float> { endurance, speed, dexterity, precision, magic };
    }

    /// <summary>
    /// Creates a new <c>PlayerStats</c> instance with all values from this instance clamped between 0 and 100.
    /// </summary>
    /// <returns>A new <c>PlayerStats</c> instance with stats values within range.</returns>
    public PlayerStats ClampedToRange() {
        return new PlayerStats {
            endurance = Mathf.Clamp(endurance, 0, 100),
            speed = Mathf.Clamp(speed, 0, 100),
            dexterity = Mathf.Clamp(dexterity, 0, 100),
            precision = Mathf.Clamp(precision, 0, 100),
            magic = Mathf.Clamp(magic, 0, 100)
        };
    }

    /// <summary>
    /// Gets the complement values of the stats (i.e. for each stat it is a hundred minus the value).
    /// </summary>
    /// <returns>A new <c>PlayerStats</c> instance with complement stats values.</returns>
    public PlayerStats GetComplement() {
        return new PlayerStats {
            endurance = 100 - endurance,
            speed = 100 - speed,
            dexterity = 100 - dexterity,
            precision = 100 - precision,
            magic = 100 - magic
        };
    }

    /// <summary>
    /// Converts the stats values to a string containing first letter of each stat name followed by the value.
    /// </summary>
    /// <returns>A string containing the stats values.</returns>
	public override string ToString() {
        return $"(E: {endurance}, S: {speed}, D: {dexterity}, P: {precision}, M: {magic})";
	}

    /// <summary>
    /// Creates a list of stats' names (not localized) in a specific order (i.e. Endurance, Speed, Dexterity, Precision, Magic).
    /// </summary>
    /// <returns>A list of stats' names.</returns>
    public static List<string> GetListOfStatNames() {
        return new List<string> { "Endurance", "Speed", "Dexterity", "Precision", "Magic" };
    }

    /// <summary>
    /// Creates a new <c>PlayerStats</c> instance from a list of stats' values in a specific order (i.e. Endurance, Speed, Dexterity, Precision, Magic).
    /// </summary>
    /// <param name="values">A list of stats' values.</param>
    /// <returns></returns>
    public static PlayerStats FromListOfValues(List<int> values) {
        if (values.Count < 5) throw new ArgumentException("Invalid number of values provided for PlayerStats, should be 5.");
        PlayerStats stats = new PlayerStats();
        stats.endurance = values[0];
        stats.speed = values[1];
        stats.dexterity = values[2];
        stats.precision = values[3];
        stats.magic = values[4];
        return stats;
    }

    #region Operator overloads
    /// <summary>
    /// Sums up corresponding stats values from both instances and creates a new <c>PlayerStats</c> instance with these new values clamped into correct range.
    /// </summary>
    /// <param name="a">First stats values.</param>
    /// <param name="b">Second stats values.</param>
    /// <returns>A new <c>PlayerStats</c> containing sums of corresponding stats values clamped into correct range.</returns>
    public static PlayerStats operator +(PlayerStats a, PlayerStats b) {
        return new PlayerStats {
            endurance = a.endurance + b.endurance,
            speed = a.speed + b.speed,
            dexterity = a.dexterity + b.dexterity,
            precision = a.precision + b.precision,
            magic = a.magic + b.magic
        }.ClampedToRange();
    }

    /// <summary>
    /// Subtracts the second stats values from the first ones and creates a new <c>PlayerStats</c> instance with these new values clamped into correct range.
    /// </summary>
    /// <param name="a">First stats values.</param>
    /// <param name="b">Second stats values.</param>
    /// <returns>A new <c>PlayerStats</c> containing difference of corresponding stats values clamped into correct range.</returns>
    public static PlayerStats operator -(PlayerStats a, PlayerStats b) {
        return new PlayerStats {
            endurance = a.endurance - b.endurance,
            speed = a.speed - b.speed,
            dexterity = a.dexterity - b.dexterity,
            precision = a.precision - b.precision,
            magic = a.magic - b.magic
        }.ClampedToRange();
    }

    /// <summary>
    /// Multiplies all stats values by the given number and creates a new <c>PlayerStats</c> instance with these new values clamped into correct range.
    /// </summary>
    /// <param name="a">Stats values.</param>
    /// <param name="b">Multiplier.</param>
    /// <returns>A new <c>PlayerStats</c> containing multiples of original stats values clamped into correct range.</returns>
    public static PlayerStats operator *(PlayerStats a, float b) {
        return new PlayerStats {
            endurance = Mathf.RoundToInt(a.endurance * b),
            speed = Mathf.RoundToInt(a.speed * b),
            dexterity = Mathf.RoundToInt(a.dexterity * b),
            precision = Mathf.RoundToInt(a.precision * b),
            magic = Mathf.RoundToInt(a.magic * b)
        }.ClampedToRange();
    }

    /// <summary>
    /// Multiplies all stats values by the given number and creates a new <c>PlayerStats</c> instance with these new values clamped into correct range.
    /// </summary>
    /// <param name="a">Multiplier.</param>
    /// <param name="b">Stats values.</param>
    /// <returns>A new <c>PlayerStats</c> containing multiples of original stats values clamped into correct range.</returns>
    public static PlayerStats operator *(float a, PlayerStats b) {
        return b * a;
    }

    /// <summary>
    /// Divides all stats values by the given number and creates a new <c>PlayerStats</c> instance with these new values clamped into correct range.
    /// </summary>
    /// <param name="a">Stats values.</param>
    /// <param name="b">Divisor.</param>
    /// <returns>A new <c>PlayerStats</c> containing the original stats values divided and clamped into correct range.</returns>
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