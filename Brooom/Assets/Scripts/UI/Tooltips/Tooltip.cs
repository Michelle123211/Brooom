using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : TooltipBase {

	[Tooltip("Texts which should be shown in the individual tooltip sections.")]
	public TooltipSectionsText texts;

	protected override void PassTextToTooltipController() {
		TooltipController.Instance.SetTooltipContent(texts);
	}
}
