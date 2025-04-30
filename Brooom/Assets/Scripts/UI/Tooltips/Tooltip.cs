using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A more complex version of tooltip with several different sections of text displayed on the screen on mouse hover.
/// It uses <c>TooltipController</c> to set content of the tooltip and to display/hide it 
/// (there is only one tooltip object in the scene, controlled by <c>TooltipController</c>, which is reused).
/// </summary>
public class Tooltip : TooltipBase {

	[Tooltip("Texts which should be shown in the individual tooltip sections.")]
	public TooltipSectionsText texts;

	/// <inheritdoc/>
	protected override void PassTextToTooltipController() {
		TooltipController.Instance.SetTooltipContent(texts);
	}
}
