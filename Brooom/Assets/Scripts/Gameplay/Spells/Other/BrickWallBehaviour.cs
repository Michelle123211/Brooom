using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickWallBehaviour : MonoBehaviour {

	[Tooltip("After that amount of seconds the wall will be destroyed.")]
	[SerializeField] float duration = 30;

	[Tooltip("A GenericTween component which will be used to let the wall appear/disappear.")]
	[SerializeField] GenericTween appearTween;
	[Tooltip("A GenericTween component which will be used to let the wall pulse while being in the air.")]
	[SerializeField] GenericTween pulseTween;

	private float currentTime = 0f;
	private bool isBeingDestroyed = false;

	public void DestroySelf() {
		Destroy(gameObject);
	}

	private void Awake() {
		transform.localScale = Vector3.zero;
	}

	private void Start() {
		appearTween.DoTween();
	}

	private void OnCollisionEnter(Collision collision) {
		if (!isBeingDestroyed && collision.gameObject.layer == LayerMask.NameToLayer("Characters")) {
			isBeingDestroyed = true;
			Invoke(nameof(Disappear), 3f);
		}
	}

	private void Disappear() {
		pulseTween.enabled = false;
		appearTween.UndoTween();
	}

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
