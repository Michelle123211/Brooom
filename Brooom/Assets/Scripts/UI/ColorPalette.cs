using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Similar to UIPalette from Magical Pet Shop (https://github.com/maoap1/magical-pet-shop/blob/main/MagicalPetShop/Assets/Scripts/UI/UIPalette.cs)
[CreateAssetMenu(fileName = "ColorPalette", menuName = "Color Palette")]
public class ColorPalette : ScriptableObject {

	#region Simple singleton
	private static ColorPalette instance;
	public static ColorPalette Instance {
		get {
			if (instance == null) {
				instance = Resources.Load<ColorPalette>("ColorPalette"); // load palette
			}
			return instance;
		}
	}
	public static void ResetInstance() {
		instance = null;
	}
	#endregion

	#region Color fields
	// TODO: Add more colors

	[Header("Main UI elements")]
	[SerializeField] Color primaryButton;
	[SerializeField] Color secondaryButton;
	[SerializeField] Color primaryBackground;
	[SerializeField] Color secondaryBackground;
	[SerializeField] Color highlightColor;
	[SerializeField] Color screenTitleText;
	[SerializeField] Color sectionTitleText;
	[SerializeField] Color darkText;
	[SerializeField] Color lightText;
	[SerializeField] Color positiveColor;
	[SerializeField] Color negativeColor;
	[SerializeField] Color notificationColor;
	[SerializeField] Color tooltipBackground;
	[SerializeField] Color tooltipText;
	// TODO: sliders, scrollbars, ...

	[Header("Leaderboard")]
	[SerializeField] Color gold;
	[SerializeField] Color silver;
	[SerializeField] Color bronze;
	[SerializeField] Color potato;
	[SerializeField] Color leaderboardRowBackground;
	[SerializeField] Color leaderboardSecondaryRowBackground;
	[SerializeField] Color leaderboardRewardBackground;

	[Header("HUD")]
	[SerializeField] Color overlayBackground;
	[SerializeField] Color characterEffectBackground;
	[SerializeField] Color manaCostBackground;

	[Header("Minimap")]
	[SerializeField] Color minimapHoop;
	[SerializeField] Color minimapCheckpoint;
	[SerializeField] Color minimapBorder;
	[SerializeField] Color minimapStartingZone;
	[SerializeField] Color minimapFinishLine;

	[Header("Spells")]
	[SerializeField] Color manaColor;
	[SerializeField] Color selfCastSpellBackground;
	[SerializeField] Color opponentCurseSpellBackground;
	[SerializeField] Color environmentManipulationSpellBackground;
	[SerializeField] Color objectApparitionSpellBackground;
	[SerializeField] Color emptySlotColor;

	// Shop - from 700
	[Header("Shop")]
	[SerializeField] Color activeLightBroomUpgradeLevel;
	[SerializeField] Color activeDarkBroomUpgradeLevel;
	[SerializeField] Color inactiveBroomUpgradeLevel;
	[SerializeField] Color coinColor;

	// Statistics graph - from 800
	[Header("Statistics")]
	[SerializeField] Color statsGraphPrimaryAxis;
	[SerializeField] Color statsGraphSecondaryAxis;
	[SerializeField] Color statsGraphNewValues;
	[SerializeField] Color statsGraphOldValues;
	[SerializeField] Color statsGraphLabelBackground;
	[SerializeField] Color statsGraphValueChangeBackground;
	#endregion

	#region Enum to color mapping
	public Color GetColor(ColorFromPalette color) {
		return color switch {
			// TODO: Add more cases
			// Main UI elements
			ColorFromPalette.MainUI_ButtonPrimary => primaryButton,
			ColorFromPalette.MainUI_ButtonSecondary => secondaryButton,
			ColorFromPalette.MainUI_BackgroundPrimary => primaryBackground,
			ColorFromPalette.MainUI_BackgroundSecondary => secondaryBackground,
			ColorFromPalette.MainUI_HighlightColor => highlightColor,
			ColorFromPalette.MainUI_TextTitleScreen => screenTitleText,
			ColorFromPalette.MainUI_TextTitleSection => sectionTitleText,
			ColorFromPalette.MainUI_TextDark => darkText,
			ColorFromPalette.MainUI_TextLight => lightText,
			ColorFromPalette.MainUI_PositiveColor => positiveColor,
			ColorFromPalette.MainUI_NegativeColor => negativeColor,
			ColorFromPalette.MainUI_Notification => notificationColor,
			ColorFromPalette.MainUI_TooltipBackground => tooltipBackground,
			ColorFromPalette.MainUI_TooltipText => tooltipText,
			// Leaderboard
			ColorFromPalette.Leaderboard_PlaceGold => gold,
			ColorFromPalette.Leaderboard_PlaceSilver => silver,
			ColorFromPalette.Leaderboard_PlaceBronze => bronze,
			ColorFromPalette.Leaderboard_PlacePotato => potato,
			ColorFromPalette.Leaderboard_Row => leaderboardRowBackground,
			ColorFromPalette.Leaderboard_RowSecondary => leaderboardSecondaryRowBackground,
			ColorFromPalette.Leaderboard_RewardBackground => leaderboardRewardBackground,
			// HUD
			ColorFromPalette.HUD_BackgroundOverlay => overlayBackground,
			ColorFromPalette.HUD_BackgroundCharacterEffect => characterEffectBackground,
			ColorFromPalette.HUD_BackgroundManaCost => manaCostBackground,
			// Minimap
			ColorFromPalette.Minimap_Hoop => minimapHoop,
			ColorFromPalette.Minimap_Checkpoint => minimapCheckpoint,
			ColorFromPalette.Minimap_Border => minimapBorder,
			ColorFromPalette.Minimap_StartingZone => minimapStartingZone,
			ColorFromPalette.Minimap_FinishLine => minimapFinishLine,
			// Spells
			ColorFromPalette.Spells_ManaColor => manaColor,
			ColorFromPalette.Spells_BackgroundSelfCastSpell => selfCastSpellBackground,
			ColorFromPalette.Spells_BackgroundOpponentCurse => opponentCurseSpellBackground,
			ColorFromPalette.Spells_BackgroundEnvironmentManipulation => environmentManipulationSpellBackground,
			ColorFromPalette.Spells_BackgroundObjectApparition => objectApparitionSpellBackground,
			ColorFromPalette.Spells_EmptySlotColor => emptySlotColor,
			// Shop
			ColorFromPalette.Shop_BroomUpgradeLevelActiveLight => activeLightBroomUpgradeLevel,
			ColorFromPalette.Shop_BroomUpgradeLevelActiveDark => activeDarkBroomUpgradeLevel,
			ColorFromPalette.Shop_BroomUpgradeLevelInactive => inactiveBroomUpgradeLevel,
			ColorFromPalette.Shop_Coin => coinColor,
			// Statistics graph
			ColorFromPalette.StatsGraph_AxisPrimary => statsGraphPrimaryAxis,
			ColorFromPalette.StatsGraph_AxisSecondary => statsGraphSecondaryAxis,
			ColorFromPalette.StatsGraph_ValuesNew => statsGraphNewValues,
			ColorFromPalette.StatsGraph_ValuesOld => statsGraphOldValues,
			ColorFromPalette.StatsGraph_AxisLabelBackground => statsGraphLabelBackground,
			ColorFromPalette.StatsGraph_ValueChangeBackground => statsGraphValueChangeBackground,
			// Default
			_ => Color.black,
		};
	}

	// Returns color based on the given place (i.e. gold, silver, bronze)
	public Color GetLeaderboardPlaceColor(int place) { 
		return place switch {
			1 => ColorPalette.Instance.GetColor(ColorFromPalette.Leaderboard_PlaceGold),
			2 => ColorPalette.Instance.GetColor(ColorFromPalette.Leaderboard_PlaceSilver),
			3 => ColorPalette.Instance.GetColor(ColorFromPalette.Leaderboard_PlaceBronze),
			_ => ColorPalette.Instance.GetColor(ColorFromPalette.Leaderboard_PlacePotato)
		};
	}
	#endregion

}

public enum ColorFromPalette {
	None = 0,
	// TODO: Add more colors

	// Main UI elements - from 100
	MainUI_ButtonPrimary = 100,					// buttons in Main Menu, ...
	MainUI_ButtonSecondary = 101,				// buttons in settings, character creation, ...
	MainUI_BackgroundPrimary = 110,				// pause menu, temporarily in settings, about, player overview, shop, race results
	MainUI_BackgroundSecondary = 111,			// temporarily for sections in settings, about
	MainUI_HighlightColor = 120,				// tooltip, buttons
	MainUI_TextTitleScreen = 130,				// all screen titles, pause menu
	MainUI_TextTitleSection = 131,				// shop (spells, broom upgrades), settings (audio, controls, ...), about
	MainUI_TextDark = 132,						// almost everywhere
	MainUI_TextLight = 133,						// loading screen
	MainUI_PositiveColor = 140,					// price, mana cost, stats/leaderboard change
	MainUI_NegativeColor = 141,					// price, mana cost, HUD penalization (time, missed hoops), missed hoops in results, stats/leaderboard change
	MainUI_Notification = 150,					// new achievement
	MainUI_TooltipBackground = 160,
	MainUI_TooltipText = 161,
	// TODO: sliders, scrollbars, ... = 170		// settings, ...

	// Leaderboard - from 300
	Leaderboard_PlaceGold = 300,				// HUD, race results
	Leaderboard_PlaceSilver = 301,				// HUD, race results
	Leaderboard_PlaceBronze = 302,				// HUD, race results
	Leaderboard_PlacePotato = 303,				// HUD, race results
	Leaderboard_Row = 310,						// race results, global leaderboard
	Leaderboard_RowSecondary = 311,				// race results, global leaderboard
	Leaderboard_RewardBackground = 320,			// race results

	// HUD - from 400
	HUD_BackgroundOverlay = 400,				// different sections of HUD
	HUD_BackgroundCharacterEffect = 410,
	HUD_BackgroundManaCost = 420,

	// Minimap - from 500
	Minimap_Hoop = 500,
	Minimap_Checkpoint = 501,
	Minimap_Border = 502,
	Minimap_StartingZone = 503,
	Minimap_FinishLine = 504,

	// Spells - from 600
	Spells_ManaColor = 600,						// mana bar in HUD, tooltip, mana bonus
	Spells_BackgroundSelfCastSpell = 610,
	Spells_BackgroundOpponentCurse = 611,
	Spells_BackgroundEnvironmentManipulation = 612,
	Spells_BackgroundObjectApparition = 613,
	Spells_EmptySlotColor = 620,

	// Shop - from 700
	Shop_BroomUpgradeLevelActiveLight = 700,	// lowest broom upgrade level
	Shop_BroomUpgradeLevelActiveDark = 701,		// highest broom upgrade level
	Shop_BroomUpgradeLevelInactive = 702,
	Shop_Coin = 710,

	// Statistics graph - from 800
	StatsGraph_AxisPrimary = 800,
	StatsGraph_AxisSecondary = 801,
	StatsGraph_ValuesNew = 810,					// polygon for new values
	StatsGraph_ValuesOld = 811,					// polygon for old values
	StatsGraph_AxisLabelBackground = 820,
	StatsGraph_ValueChangeBackground = 821
}