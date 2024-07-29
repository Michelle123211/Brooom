using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// A common interface for any visual effect related to casting a spell
public abstract class CustomVisualEffect : MonoBehaviour {

    public bool IsPlaying { get; private set; } = false;

    public void StartPlaying() {
        IsPlaying = true;
        StartPlaying_Internal();
    }
    public void StopPlaying() {
        IsPlaying = false;
        StopPlaying_Internal();
    }

    protected abstract void StartPlaying_Internal();
    protected abstract void StopPlaying_Internal();
    // Returns bool indicating whether it is still playing
    protected abstract bool UpdatePlaying_Internal(float deltaTime);

	private void Update() {
        if (IsPlaying) {
            IsPlaying = UpdatePlaying_Internal(Time.deltaTime);
            if (!IsPlaying) StopPlaying_Internal();
        }
	}

}

public abstract class SelfDestructiveVisualEffect : CustomVisualEffect {

	protected override void StopPlaying_Internal() {
        StopPlayingBeforeDestruction_Internal();
        Destroy(gameObject);
	}

    protected abstract void StopPlayingBeforeDestruction_Internal();

}
