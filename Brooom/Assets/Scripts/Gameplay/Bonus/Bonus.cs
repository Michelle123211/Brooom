using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// Common component of all the bonus objects (handles common functionality and triggering the specific effect)
[RequireComponent(typeof(Collider))]
public class Bonus : MonoBehaviour {
    [Tooltip("Material assigned to the bonus.")]
    public Material bonusMaterial;

    [Tooltip("If the bonus should be reactivated after a certain period, or it should be destroyed after being picked up.")]
    public bool shouldReactivate = true;
    [Tooltip("After how many seconds picked-up bonus is activated again.")]
    public float reactivationTime = 8;

    [Tooltip("What should happen when the bonus is picked up other than the primary effect handled by BonusEffect component (e.g. particles, destroying self).")]
    public UnityEvent pickUpEvent;

    [Tooltip("What should happen when the bonus is activated (e.g. particles).")]
    public UnityEvent activationEvent;
    [Tooltip("What should happen when the bonus is deactivated (e.g. particles).")]
    public UnityEvent deactivationEvent;

    private Vector3 originalPosition; // position at which the bonus should reappear (no matter if it was moved previously, e.g. by a spell)

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Characters") && !other.isTrigger) {
            // Get the CharacterMovementController component on the root object
            ColliderRootObject colliderRootObject = other.GetComponent<ColliderRootObject>();
            CharacterMovementController character = colliderRootObject.GetRootObject().GetComponent<CharacterMovementController>();
            if (character == null) {
                Debug.LogWarning("OnTriggerEnter() in bonus object is invoked but no CharacterMovementController was found.");
                return;
            }
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.Events.Game.BonusPickedUp, character.gameObject);
            // Invoke the specific event
            BonusEffect effect = GetComponent<BonusEffect>();
            if (effect != null && character != null)
                effect.ApplyBonusEffect(character);
            // Invoke the common events (e.g. particles, destroying self)
            if (!Utils.IsNullEvent(pickUpEvent))
                pickUpEvent.Invoke(); // TODO: Add some pick up event
            // Let anyone interested know that a bonus was picked up by the player
            if (character.isPlayer) Messaging.SendMessage("BonusPickedUp", gameObject);
            // Deactivate and start reactivation countdown, or destroy
            if (shouldReactivate) Invoke(nameof(Activate), reactivationTime);
            Deactivate();
            if (!shouldReactivate) DestroySelf();
        }
	}
    
    [ContextMenu("Refresh Visual")]
    public void RefreshVisual() {
        GetComponentInChildren<MeshRenderer>().material = bonusMaterial;
    }

    public void Deactivate() {
        if (!Utils.IsNullEvent(deactivationEvent))
            deactivationEvent.Invoke();
        // TODO: Wait until the event is done, in the meantime e.g. disable only the collider
        // TODO: Add some tweening and deactivation event
        gameObject.SetActive(false);
    }

    public void Activate() {
        // TODO: Add some tweening and activation event
        transform.position = originalPosition; // make sure to spawn the bonus at its original location
        gameObject.SetActive(true);
        if (!Utils.IsNullEvent(activationEvent))
            activationEvent.Invoke();
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    private void Awake() {
        transform.localScale = Vector3.zero;
    }

    private void Start() {
        originalPosition = transform.position;
        RefreshVisual();
        Activate();
	}
}
