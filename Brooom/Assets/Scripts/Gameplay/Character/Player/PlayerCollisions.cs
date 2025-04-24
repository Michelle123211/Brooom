using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component detecting collisions between the player and other objects.
/// It notifies anyone interested that a collision occurred.
/// </summary>
public class PlayerCollisions : MonoBehaviour {

	[Tooltip("Only collisions with objects from these layers will be detected.")]
	[SerializeField] LayerMask layersToDetect;

	[Tooltip("Another collision which is following in a shorter time then the specified timeout is not registered (to prevent registering multiple small collisions with the same object).")]
	[SerializeField] float collisionTimeout = 5f;

	private GameObject lastCollisionTarget;
	private float lastCollisionTime;

	private void OnCollisionEnter(Collision collision) {
		// Only collisions with a different target than the last one and after a certain timeout are registered
		if (lastCollisionTarget != collision.gameObject && Time.time > lastCollisionTime + collisionTimeout) {
			// Only collisions with objects from specific layers are registered
			if ((layersToDetect & (1 << collision.gameObject.layer)) != 0) {
				// Update state
				lastCollisionTarget = collision.gameObject;
				lastCollisionTime = Time.time;
				// Notify anyone interested
				Messaging.SendMessage("ObstacleCollision");
			}
		}
	}
}
