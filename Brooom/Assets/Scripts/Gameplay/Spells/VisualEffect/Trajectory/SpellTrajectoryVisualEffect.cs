using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTrajectoryVisualEffect : CustomVisualEffect {

	[Tooltip("Speed in which the spell is travelling to the target.")]
	[SerializeField] private float spellSpeed = 20;
    [Tooltip("A component used to compute spell trajectory between the racer casting it and the target.")]
    [SerializeField] private SpellTrajectoryComputer spellTrajectory;

	[Tooltip("A GameObject vith visual representation of the spell travelling to its target, e.g. a sphere.")]
	[SerializeField] private GameObject spellCastVisual;
	[Tooltip("A component representing trail which is left behind while the spell is travelling to its target.")]
	[SerializeField] private TrailRenderer spellCastTrail;
	[Tooltip("A component representing particles which are left behind while the spell is travelling to its target.")]
	[SerializeField] private ParticleSystem spellCastParticles;

	private SpellCastParameters castParameters;
	private bool isInitialized = false;

	private float currentTime = 0;


	public void InitializeStartAndTarget(SpellCastParameters castParameters) {
		this.castParameters = castParameters;
		isInitialized = true;
	}

	protected override void StartPlaying_Internal() {
		if (!isInitialized)
			throw new System.NotSupportedException("SpellTrajectoryVisualEffect must be initialized using the InitializeStartAndTarget method before playing.");
		// Initialization
		transform.position = castParameters.GetCastPoint();
		currentTime = 0;
		spellTrajectory.ResetTrajectory();
		// Set material color and enable everything (object with visual representation, trail, particles)
		Color color = castParameters.Spell.EmissionColor;
		if (spellCastVisual != null) { // enable visual
			spellCastVisual.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
			spellCastVisual.SetActive(true);
		}
		if (spellCastTrail != null) { // enable trail
			spellCastTrail.material.SetColor("_Color", color);
			spellCastTrail.emitting = true;
		}
		if (spellCastParticles != null) { // enable particles
			// Set start color
			ParticleSystem.MainModule mainModule = spellCastParticles.main;
			mainModule.startColor = color.WithA(0.75f); // TODO: Change the alpha value if necessary (based on sprites used)
			// Set color over lifetime
			Gradient gradient = new Gradient();
			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
				new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
			ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = spellCastParticles.colorOverLifetime;
			colorOverLifetimeModule.color = gradient;
			// Play
			spellCastParticles.Play();
		}
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
		Vector3 startPosition = castParameters.GetCastPoint();
		Vector3 targetPosition = castParameters.GetTargetPoint();
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
