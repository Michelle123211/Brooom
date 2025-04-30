using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A component controlling a single animated star in the game ending screen.
/// First, the star's trail appears by gradually filling its image from the centre of the screen, then the star itself appears.
/// </summary>
public class EndingStarUI : MonoBehaviour {

	[Tooltip("Whether the star is on the left side or the right side of the screen. That affects a trail animation, from which side it is filled.")]
	[SerializeField] bool isOnLeft = true;

	[Tooltip("Image displaying the star itself.")]
	[SerializeField] Image starImage;
	[Tooltip("Image displaying the star's trail.")]
	[SerializeField] Image trailImage;

	private Animator animator;

	/// <summary>
	/// Sets the star's color.
	/// </summary>
	/// <param name="color">Color to be used for the star.</param>
	public void SetColor(Color color) {
		starImage.color = color.WithA(1);
		trailImage.color = color.WithA(0.6f);
	}

	/// <summary>
	/// Checks whether the star's idle animation is playing, indicating that the start is not visible and the main animation has finished and can be started again.
	/// </summary>
	/// <returns><c>true</c> if the star is in an idle state (not visible), <c>false</c> otherwise.</returns>
	public bool IsIdle() {
		return this.animator.GetCurrentAnimatorStateInfo(0).IsName("StarIdle");
	}

	/// <summary>
	/// Plays a sound effect for when the star's trail is gradually appearing. Can be invoked from animation.
	/// </summary>
	public void PlayFireworkWhistleSound() {
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.FireworkWhistle);
	}
	/// <summary>
	/// Plays a sound effect for when the star itself is appearing. Can be invoked from animation.
	/// </summary>
	public void PlayFireworkExplosionSound() {
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.FireworkHit);
	}
	/// <summary>
	/// Starts playing an animation of the star appearing on the screen.
	/// </summary>
	public void PlaySpawnAnimation() {
		this.animator.SetTrigger("Spawn");
	}

	private void Awake() {
		this.animator = GetComponent<Animator>();
		this.animator.SetBool("ToTheLeft", isOnLeft);
	}

}
