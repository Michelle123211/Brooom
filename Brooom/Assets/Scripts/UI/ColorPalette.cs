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
	[Header("Main UI")]
	public Color gold;
	public Color silver;
	public Color bronze;
	public Color potato;
	public Color alertColor;

	[Header("HUD")]
	public Color characterEffectBackground;
	public Color minimapHoop;
	public Color minimapCheckpoint;
	public Color minimapBorder;
	public Color minimapStartingZone;
	public Color minimapFinishLine;

	[Header("Spells")]
	public Color selfCastSpellBackground;
	public Color opponentCurseSpellBackground;
	public Color environmentManipulationSpellBackground;
	public Color objectApparitionSpellBackground;
	#endregion

	public Color GetColor(ColorFromPalette color) {
		return color switch {
			// TODO: Add more cases
			// Main UI
			ColorFromPalette.MainUI_Gold => gold,
			ColorFromPalette.MainUI_Silver => silver,
			ColorFromPalette.MainUI_Bronze => bronze,
			ColorFromPalette.MainUI_Potato => potato,
			ColorFromPalette.MainUI_AlertColor => alertColor,
			// HUD
			ColorFromPalette.HUD_CharacterEffectBackground => characterEffectBackground,
			ColorFromPalette.HUD_MinimapHoop => minimapHoop,
			ColorFromPalette.HUD_MinimapCheckpoint => minimapCheckpoint,
			ColorFromPalette.HUD_MinimapBorder => minimapBorder,
			ColorFromPalette.HUD_MinimapStartingZone => minimapStartingZone,
			ColorFromPalette.HUD_MinimapFinishLine => minimapFinishLine,
			// Spells
			ColorFromPalette.Spells_SelfCastSpellBackground => selfCastSpellBackground,
			ColorFromPalette.Spells_OpponentCurseSpellBackground => opponentCurseSpellBackground,
			ColorFromPalette.Spells_EnvironmentManipulationSpellBackground => environmentManipulationSpellBackground,
			ColorFromPalette.Spells_ObjectApparitionSpellBackground => objectApparitionSpellBackground,
			// Default
			_ => Color.black,
		};
	}

}

public enum ColorFromPalette {
	None = 0,
	// TODO: Add more colors
	// Main UI
	MainUI_Gold = 100,
	MainUI_Silver = 101,
	MainUI_Bronze = 102,
	MainUI_Potato = 103,
	MainUI_AlertColor = 104,
	// HUD
	HUD_CharacterEffectBackground = 200,
	HUD_MinimapHoop = 201,
	HUD_MinimapCheckpoint = 202,
	HUD_MinimapBorder = 203,
	HUD_MinimapStartingZone = 204,
	HUD_MinimapFinishLine = 205,
	// Spells
	Spells_SelfCastSpellBackground = 300,
	Spells_OpponentCurseSpellBackground = 301,
	Spells_EnvironmentManipulationSpellBackground = 302,
	Spells_ObjectApparitionSpellBackground = 303
}
