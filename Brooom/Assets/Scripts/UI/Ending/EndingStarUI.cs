using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingStarUI : MonoBehaviour {

	[Tooltip("Whether the star is on the left side or the right side of the screen. That affects a trail animation, from which side it is filled.")]
	[SerializeField] bool isOnLeft = true;

	[SerializeField] Image starImage;
	[SerializeField] Image trailImage;

	private Animator animator;

	public void SetColor(Color color) {
		starImage.color = color.WithA(1);
		trailImage.color = color.WithA(0.6f);
	}

	public bool IsIdle() {
		return this.animator.GetCurrentAnimatorStateInfo(0).IsName("StarIdle");
	}

	public void PlayFireworkWhistleSound() {
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.FireworkWhistle);
	}

	public void PlayFireworkExplosionSound() {
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.FireworkHit);
	}

	public void PlaySpawnAnimation() {
		this.animator.SetTrigger("Spawn");
	}

	private void Awake() {
		this.animator = GetComponent<Animator>();
		this.animator.SetBool("ToTheLeft", isOnLeft);
	}

}
