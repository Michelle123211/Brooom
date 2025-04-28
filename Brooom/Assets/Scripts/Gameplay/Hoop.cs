using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// A component representing a hoop or a checkpoint object on a track.
/// It detects any racers flying through and updates their <c>CharacterRaceState</c> accordingly.
/// </summary>
public class Hoop : MonoBehaviour {

    /// <summary>An option to register callback on hoop passed (it is used e.g. in a tutorial). The parameter is index of the hoop.</summary>
    public event Action<int> onHoopPassed;

    [Tooltip("Object representing an arrow above the hoop highlighting the next hoop the player has to fly through.")]
    [SerializeField] GameObject highlightArrow;
    [Tooltip("Sprite Renderer representing a minimap icon of the hoop.")]
    [SerializeField] SpriteRenderer minimapIcon;

    private int index = 0; // index of the track point represented by the hoop
    private bool isActive = false; // when active, the hoop detects racer flying through it
    private Dictionary<int, int> activeTriggersCount = new Dictionary<int, int>(); // how many triggers (value) the racer (instanceID is the key) entered and have not exited

    private Animator animator; // for arrow highlighting the hoop
    private Color minimapIconColor;

    /// <summary>
    /// Activates the hoop, only then it detects racers flying through.
    /// </summary>
    /// <param name="hoopIndex">Index of the track point this hoop belongs to.</param>
    public void Activate(int hoopIndex) {
        index = hoopIndex;
        isActive = true;
        // Reset
        activeTriggersCount.Clear();
    }

    /// <summary>
    /// Starts highlighting the hoop by showing a big arrow above it and making its minimap icon more distinctive.
    /// </summary>
    public void StartHighlighting() {
        highlightArrow.transform.localScale = Vector3.zero;
        // Show an arrow above the hoop + scale minimap icon up and down
        animator.SetBool("IsHighlighted", true);
        highlightArrow.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
        // Highlight minimap icon
        minimapIcon.color = Color.red;
    }

    /// <summary>
    /// Stops highlighting the hoop by hiding a big arrow above it and restoring its minimap icon to its original state.
    /// </summary>
    public void StopHighlighting() {
        // Hide the arrow above the hoop + stop scaling the minimap icon up and down
        animator.SetBool("IsHighlighted", false);
        // Change the minimap icon back to normal
        minimapIcon.color = minimapIconColor;
    }

    /// <summary>
    /// Detects any racer flying through and calls a callback <c>onHoopPassed</c> on their <c>CharacterRaceState</c> component to update it.
    /// It is called whenever an object enters the trigger zone.
    /// </summary>
    /// <param name="otherObject">Object entering the trigger zone.</param>
    public void OnHoopTriggerEntered(Collider otherObject) {
        if (!isActive) return;
        if (otherObject.isTrigger) return;
        ColliderRootObject colliderRootObject = otherObject.GetComponent<ColliderRootObject>();
        GameObject rootObject = colliderRootObject.GetRootObject();
        int instanceID = rootObject.GetInstanceID();
        // Note that the racer with the given instance ID entered the hoop trigger
        if (activeTriggersCount.TryGetValue(instanceID, out int triggerCount)) {
            activeTriggersCount[instanceID] = triggerCount + 1;
        } else {
            activeTriggersCount[instanceID] = 1;
        }
        // If there are 2 triggers active simultaneously, the racer has passed through the hoop
        if (activeTriggersCount.GetValueOrDefault(instanceID) == 2) {
            rootObject.GetComponent<CharacterRaceState>()?.OnHoopPassed(index); // root object has CharacterRaceState component
            onHoopPassed?.Invoke(index);
        }
    }

    /// <summary>
    /// Detects any object exiting the hoop's trigger zone and updates its internal state accordingly. 
    /// </summary>
    /// <param name="otherObject">Object exiting the trigger zone.</param>
    public void OnHoopTriggerExited(Collider otherObject) {
        if (!isActive) return;
        if (otherObject.isTrigger) return;
        ColliderRootObject colliderRootObject = otherObject.GetComponent<ColliderRootObject>();
        GameObject rootObject = colliderRootObject.GetRootObject();
        int instanceID = rootObject.GetInstanceID();
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
