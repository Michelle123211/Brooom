using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlacement : LevelGeneratorModule {

	[Tooltip("Plane already placed in the scene which will be scaled according to the level dimensions.")]
	[SerializeField] Transform waterPlane;
	public override void Generate(LevelRepresentation level) {
		waterPlane.position = Vector3.zero;
		waterPlane.localScale = new Vector3(level.dimensions.x / 10f, 1f, level.dimensions.y / 10f);
	}
}
