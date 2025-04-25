using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component handling behaviour of a brick wall created by Materia Muri spell.
/// It uses tweens to make the wall appear and disappear and also to make the wall pulse while it is visible.
/// Three seconds after a racer collides with the wall or at the end of its duration, the wall is destroyed.
/// </summary>
public class BrickWallBehaviour : MonoBehaviour {

	[Tooltip("After that amount of seconds the wall will be destroyed.")]
	[SerializeField] float duration = 30;

	[Tooltip("A GenericTween component which will be used to let the wall appear/disappear.")]
	[SerializeField] GenericTween appearTween;
	[Tooltip("A GenericTween component which will be used to let the wall pulse while being in the air.")]
	[SerializeField] GenericTween pulseTween;

	private float currentTime = 0f;
	private bool isBeingDestroyed = false; // prevents from playing the disappearing tween multiple times

	/// <summary>
	/// Destroys the brick wall object.
	/// </summary>
	public void DestroySelf() {
		Destroy(gameObject);
	}

	private void Awake() {
		transform.localScale = Vector3.zero;
	}

	// Plays tween to make the wall appear
	private void Start() {
		appearTween.DoTween();
	}

	// Detects collisions with racers and schedules destruction of the wall after 3 seconds
	private void OnCollisionEnter(Collision collision) {
		if (!isBeingDestroyed && collision.gameObject.layer == LayerMask.NameToLayer("Characters")) {
			isBeingDestroyed = true;
			Invoke(nameof(Disappear), 3f);
		}
	}

	// Plays tween to make the wall disappear
	private void Disappear() {
		pulseTween.enabled = false;
		appearTween.UndoTween();
	}

	// Checks if it is already time to destroy the wall (the duration is up)
	private void Update() {
		// Destroy self after the given duration
		if (!isBeingDestroyed) {
			currentTime += Time.deltaTime;
			if (currentTime > duration) {
				isBeingDestroyed = true;
				Disappear();
			}
		}
	}

}
