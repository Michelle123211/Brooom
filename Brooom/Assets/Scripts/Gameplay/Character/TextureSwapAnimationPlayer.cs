using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSwapAnimationPlayer : MonoBehaviour {
	[Tooltip("A list of all available animations.")]
	[SerializeField] List<TextureSwapAnimation> animations;

	// For each animation stores if it is currently playing or not
	private Dictionary<string, bool> animationsPlaying = new Dictionary<string, bool>();


	public void StartAnimation(string identifier) {
		foreach (var animation in animations) {
			if (animation.identifier == identifier) {
				StartAnimation(animation);
				break;
			}
		}
	}

	public void StopAnimation(string identifier) {
		// If the animation is currently playing
		if (animationsPlaying.TryGetValue(identifier, out bool isPlaying) && isPlaying) {
			// Set the animation as not playing (the coroutine playing the animation will then stop automatically)
			animationsPlaying[identifier] = false;
			// Complete the animation
			foreach (var animation in animations) {
				if (animation.identifier == identifier) {
					CompleteAnimation(animation);
					break;
				}
			}
		}
	}

	private void StartAnimation(TextureSwapAnimation animation) {
		// If the animation is not playing already
		if (animationsPlaying.TryGetValue(animation.identifier, out bool isPlaying) && !isPlaying) {
			// Set the animation as playing and start playing it
			animationsPlaying[animation.identifier] = true;
			StartCoroutine(PlayAnimation(animation));
		}
	}

	private IEnumerator PlayAnimation(TextureSwapAnimation animation) {
		while (true) {
			foreach (var keyframe in animation.keyframes) {
				// Check if still playing
				if (animationsPlaying.TryGetValue(animation.identifier, out bool isPlaying) && !isPlaying)
					yield break;
				// Set texture
				animation.targetMeshRenderer.material.mainTexture = keyframe.keyframeTexture;
				// Wait for the duration
				yield return new WaitForSeconds(Random.Range(keyframe.duration.x, keyframe.duration.y));
			}
			// If not looping, stop playing
			if (!animation.loop) {
				animationsPlaying[animation.identifier] = false;
				yield break;
			}
		}
	}

	private void CompleteAnimation(TextureSwapAnimation animation) {
		// Set the last keyframe
		animation.targetMeshRenderer.material.mainTexture = animation.keyframes[animation.keyframes.Count - 1].keyframeTexture;
	}

	private void Start() {
		foreach (var animation in animations) {
			// Initialize animationsPlaying
			animationsPlaying[animation.identifier] = false;
			// Start those which should be played on awake
			if (animation.playOnAwake) {
				StartAnimation(animation);
			}
		}
	}
}

[System.Serializable]
public class TextureSwapAnimation {
	[Tooltip("Identifier of the animation used to start/stop the animation on request.")]
	public string identifier;
	[Tooltip("Skinned Mesh Renderer whose material is used to swap textures on.")]
	public SkinnedMeshRenderer targetMeshRenderer;
	[Tooltip("Whether the animation should start playing automatically at the start.")]
	public bool playOnAwake;
	[Tooltip("Whether the animation should be played in loop.")]
	public bool loop;

	[Tooltip("Keyframes of the animation and their duration.")]
	public List<TextureSwapAnimationKeyframe> keyframes;
}

[System.Serializable]
public class TextureSwapAnimationKeyframe {
	[Tooltip("What texture should be used in the given keyframe.")]
	public Texture keyframeTexture;
	[Tooltip("How long this keyframe should last. If the x and y values are different a random value in that interval is chosen each time.")]
	public Vector2 duration;
}