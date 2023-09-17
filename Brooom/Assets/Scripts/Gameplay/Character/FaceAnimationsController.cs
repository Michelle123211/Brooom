using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// An intermediate layer between MainMenuAnimator with its animations (with AnimationEvent) and TextureSwapAnimationPlayer
public class FaceAnimationsController : MonoBehaviour {
    [Tooltip("Component handling playing face animations.")]
    [SerializeField] TextureSwapAnimationPlayer faceAnimationsPlayer;

    public void StartSmiling() {
        faceAnimationsPlayer.StartAnimation("StartSmiling");
    }

    public void StopSmiling() {
        faceAnimationsPlayer.StopAnimation("StartSmiling");
        faceAnimationsPlayer.StartAnimation("StopSmiling");
    }
}
