using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hoop : MonoBehaviour {
    [Tooltip("Object representing an arrow above the hoop.")]
    [SerializeField] GameObject highlightArrow;
    [Tooltip("Sprite Renderer representing a minimap icon of the hoop.")]
    [SerializeField] SpriteRenderer minimapIcon;

    private int index = 0; // index of the track point represented by the hoop
    private bool isActive = false; // when active the hoop detects the player flying through it
    private Dictionary<int, int> activeTriggersCount = new Dictionary<int, int>(); // how many triggers (value) the racer (instanceID is the key) entered and have not exited

    private Animator animator;
    private Color minimapIconColor;

    // Player flying through the hoop is detected only if the hoop is active
    public void Activate(int hoopIndex) {
        index = hoopIndex;
        isActive = true;
        // Reset
        activeTriggersCount.Clear();
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

    public void OnHoopTriggerEntered(Collider otherObject) {
        if (!isActive) return;
        int instanceID = otherObject.gameObject.GetInstanceID();
        // Note that the racer with the given instance ID entered the hoop trigger
        if (activeTriggersCount.TryGetValue(instanceID, out int triggerCount)) {
            activeTriggersCount[instanceID] = triggerCount + 1;
        } else {
            activeTriggersCount[instanceID] = 1;
        }
        // If there are 2 triggers active simultaneously, the races has passed through the hoop
        if (activeTriggersCount.GetValueOrDefault(instanceID) == 2) {
            RaceController.Instance?.OnHoopPassed(index, otherObject);
        }
    }

    public void OnHoopTriggerExited(Collider otherObject) {
        if (!isActive) return;
        int instanceID = otherObject.gameObject.GetInstanceID();
        // Note that the racer with the given instance ID exited the hoop trigger
        if (activeTriggersCount.TryGetValue(instanceID, out int triggerCount)) {
            activeTriggersCount[instanceID] = triggerCount - 1;
        }
        // Prevent eventual errors (when activating the hoop while the player is inside a trigger)
        if (activeTriggersCount.GetValueOrDefault(instanceID) < 0)
            activeTriggersCount[instanceID] = 0;
    }

	private void Awake() {
        highlightArrow.SetActive(false);
        animator = GetComponent<Animator>();
        minimapIconColor = minimapIcon.color;
    }

}
