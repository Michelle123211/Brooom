using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ColliderRootObject : MonoBehaviour {

	[Tooltip("Root object of the entity to which this component and corresponding collider belong.")]
	[SerializeField] GameObject rootObject;

	public GameObject GetRootObject() {
		return this.rootObject;
	}

}
