using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// Common component of all the bonus objects (handles common functionality and triggering the specific effect)
[RequireComponent(typeof(Collider))]
public class Bonus : MonoBehaviour {
    [Tooltip("Material assigned to the bonus.")]
    public Material bonusMaterial;

    [Tooltip("After how many seconds picked-up bonus is activated again.")]
    public float reactivationTime = 8;

    [Tooltip("What should happen when the bonus is picked up other than the primary effect handled by BonusEffect component (e.g. particles, destroying self).")]
    public UnityEvent pickUpEvent;

    [Tooltip("What should happen when the bonus is activated (e.g. particles).")]
    public UnityEvent activationEvent;
    [Tooltip("What should happen when the bonus is deactivated (e.g. particles).")]
    public UnityEvent deactivationEvent;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Characters")) {
            // Get the CharacterMovementController component
            CharacterMovementController character = other.GetComponent<CharacterMovementController>(); // try the colliding object
            if (character == null)
                character = other.transform.parent.GetComponent<CharacterMovementController>(); // otherwise try the parent object
            if (character == null)
                Debug.LogWarning("OnTriggerEnter() in bonus object is invoked but no CharacterMovementController was found.");
            // Invoke the specific event
            BonusEffect effect = GetComponent<BonusEffect>();
            if (effect != null && character != null)
                effect.ApplyBonusEffect(character);
            // Invoke the common events (e.g. particles, destroying self)
            if (!Utils.IsNullEvent(pickUpEvent))
                pickUpEvent.Invoke();
            // Let anyone interested know that a bonus was picked up by the player
            if (character.isPlayer) Messaging.SendMessage("BonusPickedUp", gameObject);
            // Deactivate and start reactivation countdown
            Invoke(nameof(Activate), reactivationTime);
            Deactivate();
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
        // TODO: Add some tweening
        gameObject.SetActive(false);
    }

    public void Activate() {
        // TODO: Add some tweening
        gameObject.SetActive(true);
        if (!Utils.IsNullEvent(activationEvent))
            activationEvent.Invoke();
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

	private void Start() {
        RefreshVisual();
	}
}
