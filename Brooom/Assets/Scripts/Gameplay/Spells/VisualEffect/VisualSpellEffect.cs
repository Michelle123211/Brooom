using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// A common interface for any visual effect related to casting a spell
public abstract class VisualSpellEffect : MonoBehaviour {

    public bool isPlaying = false;

    public void StartPlaying() {
        isPlaying = true;
        StartPlaying_Internal();
    }
    public void StopPlaying() {
        isPlaying = false;
        StopPlaying_Internal();
    }

    protected abstract void StartPlaying_Internal();
    protected abstract void StopPlaying_Internal();
    // Returns bool indicating whether it is still playing
    protected abstract bool UpdatePlaying_Internal(float deltaTime);

	private void Update() {
        if (isPlaying) {
            isPlaying = UpdatePlaying_Internal(Time.deltaTime);
            if (!isPlaying) StopPlaying_Internal();
        }
	}

}
