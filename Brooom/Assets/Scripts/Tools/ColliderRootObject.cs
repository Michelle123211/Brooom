using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component allowing to quickly get a root object from an object containing a collider (e.g. on collision with another object).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ColliderRootObject : MonoBehaviour {

	[Tooltip("Root object of the entity to which this component and corresponding collider belong.")]
	[SerializeField] GameObject rootObject;

	/// <summary>
	/// Gets root object of the entity to which this component and corresponding collider belong.
	/// </summary>
	/// <returns>Root object of this entity.</returns>
	public GameObject GetRootObject() {
		return this.rootObject;
	}

}
