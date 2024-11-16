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
	public Color bronze;
	public Color silver;
	public Color gold;
	public Color platinum;

	[Header("HUD")]
	public Color hoopMinimap;
	public Color checkpointMinimap;
	public Color borderMinimap;

	[Header("Spells")]
	public Color selfCastSpellBackground;
	public Color opponentCurseSpellBackground;
	public Color environmentManipulationSpellBackground;
	public Color objectApparitionSpellBackground;

	[Header("Debug")]
	public Color testColor1;
	public Color testColor2;
	public Color testColor3;
	public Color testColor4;
	#endregion

	public Color GetColor(ColorFromPalette color) {
		return color switch {
			// TODO: Add more cases
			_ => Color.black,
		};
	}

}

public enum ColorFromPalette {
	None = 0,
	// TODO: Add more colors
}
