using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTrajectoryVisualEffect : CustomVisualEffect {

	[Tooltip("Speed in which the spell is travelling to the target.")]
	[SerializeField] private float spellSpeed = 7;
    [Tooltip("A component used to compute spell trajectory between the racer casting it and the target.")]
    [SerializeField] private SpellTrajectoryComputer spellTrajectory;

	[Tooltip("A GameObject vith visual representation of the spell travelling to its target, e.g. a sphere.")]
	[SerializeField] private GameObject spellCastVisual;
	[Tooltip("A component representing trail which is left behind while the spell is travelling to its target.")]
	[SerializeField] private TrailRenderer spellCastTrail;
	[Tooltip("A component representing particles which are left behind while the spell is travelling to its target.")]
	[SerializeField] private ParticleSystem spellCastParticles;
	[Tooltip("A color used for the visual effect")]
	[SerializeField] private Color spellCastColor = Color.white;

	private SpellTarget spellTarget;
	private bool isInitialized = false;

	private float currentTime = 0;


	public void InitializeStartAndTarget(SpellTarget spellTarget) {
		this.spellTarget = spellTarget;
		isInitialized = true;
	}

	protected override void StartPlaying_Internal() {
		if (!isInitialized)
			throw new System.NotSupportedException("SpellTrajectoryVisualEffect must be initialized using the InitializeStartAndTarget method before playing.");
		transform.position = spellTarget.GetCastPoint();
		currentTime = 0;
		spellTrajectory.ResetTrajectory();
		// TODO: Set color to every material (object with visual representation, trail, particles)
		if (spellCastVisual != null) // enable visual
			spellCastVisual.SetActive(true);
		if (spellCastTrail != null) // enable trail
			spellCastTrail.emitting = true;
		if (spellCastParticles != null) // enable particles
			spellCastParticles.Play();
	}

	protected override void StopPlaying_Internal() {
		if (spellCastVisual != null) // disable visual
			spellCastVisual.SetActive(false);
		if (spellCastTrail != null) // disable trail
			spellCastTrail.emitting = false;
		if (spellCastParticles != null) // disable particles
			spellCastParticles.Stop();
		isInitialized = false;
	}

	protected override bool UpdatePlaying_Internal(float deltaTime) {
		// Update variables
		Vector3 startPosition = spellTarget.GetCastPoint();
		Vector3 targetPosition = spellTarget.GetTargetPoint();
		currentTime += deltaTime;
		float totalDistance = Vector3.Distance(startPosition, targetPosition);
		float currentDistance = currentTime * spellSpeed;
		// Check if complete
		bool isComplete = false;
		if (currentDistance > totalDistance) {
			currentDistance = totalDistance;
			isComplete = true;
		}
		// Compute a world position out of the next trajectory point
		SpellTrajectoryPoint trajectoryPoint = spellTrajectory.GetNextTrajectoryPoint(currentDistance);
		Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, (targetPosition - startPosition));
		Vector3 currentPosition = new Vector3(trajectoryPoint.offset.x, trajectoryPoint.offset.y, trajectoryPoint.distanceFromStart);
		currentPosition = startPosition + (rotation * currentPosition);
		// Move the object to the given point
		transform.position = currentPosition;
		return !isComplete;
	}
}
