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
	public Color hoopMinimap;
	public Color checkpointMinimap;
	public Color borderMinimap;

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
			ColorFromPalette.MainUI_AlertColor => alertColor,
			// HUD

			// Spells
			ColorFromPalette.Spells_SelfCastSpellBackground => selfCastSpellBackground,
			ColorFromPalette.Spells_OpponentCurseSpellBackground => opponentCurseSpellBackground,
			ColorFromPalette.Spells_EnvironmentManipulationSpellBackground => environmentManipulationSpellBackground,
			ColorFromPalette.Spells_ObjectApparitionSpellBackground => objectApparitionSpellBackground,
			_ => Color.black,
		};
	}

}

public enum ColorFromPalette {
	None = 0,
	// TODO: Add more colors
	// Main UI
	// 100 - Gold
	// 101 - Silver
	// 102 - Bronze
	// 103 - Potato
	MainUI_AlertColor = 104,
	// Spells
	Spells_SelfCastSpellBackground = 300,
	Spells_OpponentCurseSpellBackground = 301,
	Spells_EnvironmentManipulationSpellBackground = 302,
	Spells_ObjectApparitionSpellBackground = 303
}
