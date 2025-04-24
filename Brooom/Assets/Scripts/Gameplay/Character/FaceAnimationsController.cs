using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An intermediate layer between <c>MainMenuAnimator</c> with its animations and <c>TextureSwapAnimationPlayer</c>.
/// It provides methods for playing/stopping specific animations using <c>TextureSwapAnimationPlayer</c>, 
/// which are then invoked in <c>MainMenuAnimator</c>'s animations as <c>AnimationEvent</c>s.
/// </summary>
public class FaceAnimationsController : MonoBehaviour {

    [Tooltip("Component handling playing face animations.")]
    [SerializeField] TextureSwapAnimationPlayer faceAnimationsPlayer;

    /// <summary>
    /// Starts playing face animation for smiling.
    /// </summary>
    public void StartSmiling() {
        faceAnimationsPlayer.StartAnimation("StartSmiling");
    }

    /// <summary>
    /// Stops playing smiling animation and starts playing animation for stopping smiling.
    /// </summary>
    public void StopSmiling() {
        faceAnimationsPlayer.StopAnimation("StartSmiling");
        faceAnimationsPlayer.StartAnimation("StopSmiling");
    }
}
