using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTooltip : TooltipBase {

	[Tooltip("A single string which should be shown in the tooltip.")]
	[TextArea(3, 8)]
	public string text;

	protected override void PassTextToTooltipController() {
		TooltipController.Instance.SetTooltipContent(text);
	}
}
