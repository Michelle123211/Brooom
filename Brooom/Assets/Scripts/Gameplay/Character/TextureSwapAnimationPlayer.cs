using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class responsible for playing texture swap animations when requested.
/// </summary>
public class TextureSwapAnimationPlayer : MonoBehaviour {

	[Tooltip("A list of all available animations (as a list of keyframes with additional data, e.g. identifier).")]
	[SerializeField] List<TextureSwapAnimation> animations;

	// For each animation stores if it is currently playing or not
	private Dictionary<string, bool> animationsPlaying = new();

	/// <summary>
	/// Starts playing texture swap animation with the given identifier.
	/// But nothing happens, if animation with such identifier is not found.
	/// </summary>
	/// <param name="identifier">Identifier of the animation to be played.</param>
	public void StartAnimation(string identifier) {
		foreach (var animation in animations) {
			if (animation.identifier == identifier) {
				StartAnimation(animation);
				break;
			}
		}
	}

	/// <summary>
	/// Stops playing texture swap animation with the given identifier, if it is currently playing, but makes dure it is completed immediately (set to the last keyframe).
	/// </summary>
	/// <param name="identifier">Identifier of the animation to be stopped.</param>
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

	// Starts playing the given animation
	private void StartAnimation(TextureSwapAnimation animation) {
		// If the animation is not playing already
		if (animationsPlaying.TryGetValue(animation.identifier, out bool isPlaying) && !isPlaying) {
			// Set the animation as playing and start playing it
			animationsPlaying[animation.identifier] = true;
			StartCoroutine(PlayAnimation(animation));
		}
	}

	// Coroutine for playing the given animation, yielding between keyframes
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
		animation.targetMeshRenderer.material.mainTexture = animation.keyframes[^1].keyframeTexture;
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

/// <summary>
/// A class representing a single texture swap animation composed of several keyframes (each with its own texture and duration).
/// </summary>
[System.Serializable]
public class TextureSwapAnimation {
	[Tooltip("Identifier of the animation, used to start/stop the animation on request.")]
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

/// <summary>
/// A class representing a single keyframe in a texture swap animation. It contains a texture to be used and its duration.
/// </summary>
[System.Serializable]
public class TextureSwapAnimationKeyframe {
	[Tooltip("What texture should be used in the given keyframe.")]
	public Texture keyframeTexture;
	[Tooltip("How long this keyframe should last. If the x and y values are different a random value in that interval is chosen each time.")]
	public Vector2 duration;
}