using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A common base for any visual effect related to casting a spell.
/// </summary>
public abstract class CustomVisualEffect : MonoBehaviour {

    /// <summary>Whether the visual effect is playing, or not.</summary>
    public bool IsPlaying { get; private set; } = false;

    /// <summary>
    /// Starts playing the visual effect.
    /// </summary>
    public void StartPlaying() {
        IsPlaying = true;
        StartPlaying_Internal();
    }
    /// <summary>
    /// Stops playing the visual effect.
    /// </summary>
    public void StopPlaying() {
        IsPlaying = false;
        StopPlaying_Internal();
    }

    /// <summary>
    /// Initializes everything necessary and starts playing the visual effect.
    /// </summary>
    protected abstract void StartPlaying_Internal();
    /// <summary>
    /// Stops playing the visual effect and finalizes everything necessary.
    /// </summary>
    protected abstract void StopPlaying_Internal();
    /// <summary>
    /// Updates the visual effect to progress further. It is called from <c>Update()</c> method.
    /// </summary>
    /// <param name="deltaTime">Elapsed time (in seconds) from the last call.</param>
    /// <returns><c>true</c> if the visual effect is still playing, <c>false</c> otherwise.</returns>
    protected abstract bool UpdatePlaying_Internal(float deltaTime);

	private void Update() {
        // Update the visual effect and check if it has finished
        if (IsPlaying) {
            IsPlaying = UpdatePlaying_Internal(Time.deltaTime);
            if (!IsPlaying) StopPlaying_Internal();
        }
	}

}

/// <summary>
/// A base class for any visual effect related to casting a spell which destroys itself once it has finished.
/// </summary>
public abstract class SelfDestructiveVisualEffect : CustomVisualEffect {

    /// <summary>
    /// Stops playing the visual effect, finalizes everything necessary and at the end destroys the visual effect.
    /// </summary>
	protected override void StopPlaying_Internal() {
        StopPlayingBeforeDestruction_Internal();
        Destroy(gameObject);
	}

	/// <summary>
	/// Stops playing the visual effect and finalizes everything necessary before the visual effect is destroyed.
	/// </summary>
	protected abstract void StopPlayingBeforeDestruction_Internal();

}
