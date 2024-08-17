using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for creating a wall obstacle
public class MateriaMuriSpellEffect : OneShotSpellEffect {

	[SerializeField] Transform brickWallPrefab;

	protected override void ApplySpellEffect_Internal() {
		// Create an instance of a wall prefab and set its position and rotation
		Transform wall = Instantiate<Transform>(brickWallPrefab, spellTarget.GetTargetPoint(), Quaternion.identity);
		wall.eulerAngles = Vector3.up * spellTarget.SourceObject.transform.eulerAngles.y; // perpendicular to the racer's direction of movement
	}

}
