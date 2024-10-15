using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class InverseImageMask : Image {

	public override Material materialForRendering {
		get {
			// Inverse stencil operation - not equal
			Material material = new Material(base.materialForRendering);
			material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
			return material;
		}
	}

}
