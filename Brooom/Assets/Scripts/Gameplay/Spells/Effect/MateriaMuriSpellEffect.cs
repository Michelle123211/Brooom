using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell creating a wall obstacle in the direction it was cast in.
/// </summary>
public class MateriaMuriSpellEffect : OneShotSpellEffect {

	[Tooltip("Prefab of a brick wall to be instantiated as an effect of this spell.")]
	[SerializeField] Transform brickWallPrefab;

	/// <summary>
	/// Creates a brick wall in the spell's target position.
	/// </summary>
	protected override void ApplySpellEffect_Internal() {
		// Create an instance of a wall prefab and set its position and rotation
		Transform wall = Instantiate<Transform>(brickWallPrefab, castParameters.GetTargetPosition(), Quaternion.identity);
		wall.eulerAngles = Vector3.up * castParameters.SourceObject.transform.eulerAngles.y; // perpendicular to the racer's direction of movement
	}

}
