using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;


/// <summary>
/// A component derived from <c>Image</c> which can be used to mask out a portion of an image. It should contain the base image from which a part is cutout.
/// Then the object with this component should be a child of another object with <c>Image</c> component, containing the shape which will be cutout,
/// and <c>Mask</c> component.
/// </summary>
public class InverseImageMask : Image {

	/// <inheritdoc/>
	public override Material materialForRendering {
		get {
			// Inverse stencil operation - not equal
			Material material = new Material(base.materialForRendering);
			material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
			return material;
		}
	}

}
