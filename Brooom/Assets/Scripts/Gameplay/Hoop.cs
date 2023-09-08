using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hoop : MonoBehaviour {
    [Tooltip("Object representing an arrow above the hoop.")]
    [SerializeField] GameObject highlightArrow;
    [Tooltip("Sprite Renderer representing a minimap icon of the hoop.")]
    [SerializeField] SpriteRenderer minimapIcon;

    [HideInInspector] public bool playerDetected = false;

    private bool isActive = false; // when active the hoop detects the player flying through it
    private int numOfTriggersActive = 0; // how many triggers the player entered and have not exited

    private Animator animator;
    private Color minimapIconColor;

    // Player flying through the hoop is detected only if the hoop is active
    public void Activate() {
        isActive = true;
        // Reset
        playerDetected = false;
        numOfTriggersActive = 0;
    }

    public void StartHighlighting() {
        highlightArrow.transform.localScale = Vector3.zero;
        // Show an arrow above the hoop + scale minimap icon up and down
        animator.SetBool("IsHighlighted", true);
        highlightArrow.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
        // Highlight minimap icon
        minimapIcon.color = Color.red;
    }

    public void StopHighlighting() {
        // Hide the arrow above the hoop + stop scaling the minimap icon up and down
        animator.SetBool("IsHighlighted", false);
        // Change the minimap icon back to normal
        minimapIcon.color = minimapIconColor;
    }

    public void OnPlayerTriggerEntered() {
        if (!isActive) return;
        numOfTriggersActive++;
        if (numOfTriggersActive == 2) { // there are 2 triggers, both must be active simultaneously
            playerDetected = true;
        }
    }

    public void OnPlayerTriggerExited() {
        if (!isActive) return;
        numOfTriggersActive--;
        // Prevent eventual errors (when activating the hoop while the player is inside a trigger)
        if (numOfTriggersActive < 0) numOfTriggersActive = 0;
    }

	private void Awake() {
        highlightArrow.SetActive(false);
        animator = GetComponent<Animator>();
        minimapIconColor = minimapIcon.color;
	}

}
