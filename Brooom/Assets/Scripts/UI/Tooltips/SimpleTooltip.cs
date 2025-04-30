using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A very simple tooltip with only a single text section displayed on the screen on mouse hover.
/// It uses <c>TooltipController</c> to set content of the tooltip and to display/hide it 
/// (there is only one tooltip object in the scene, controlled by <c>TooltipController</c>, which is reused).
/// </summary>
public class SimpleTooltip : TooltipBase {

	[Tooltip("A single string which should be shown in the tooltip.")]
	[TextArea(3, 8)]
	public string text;

	/// <inheritdoc/>
	protected override void PassTextToTooltipController() {
		TooltipController.Instance.SetTooltipContent(text);
	}
}
