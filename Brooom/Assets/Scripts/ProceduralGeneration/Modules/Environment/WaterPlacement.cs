using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for changing a scale of a water plane already placed in the scene so that it covers exactly the whole level.
/// </summary>
public class WaterPlacement : LevelGeneratorModule {

	[Tooltip("Plane already placed in the scene which will be scaled according to the level dimensions.")]
	[SerializeField] Transform waterPlane;

	/// <summary>
	/// Changes position and scale of a water plane already placed in the scene so that it covers exactly the whole level.
	/// </summary>
	/// <param name="level"><inheritdoc/></param>
	public override void Generate(LevelRepresentation level) {
		waterPlane.position = Vector3.zero;
		waterPlane.localScale = new Vector3(level.dimensions.x / 10f, 1f, level.dimensions.y / 10f);
	}

}
