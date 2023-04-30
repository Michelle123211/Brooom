using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Common component of all the bonus objects (handles common functionality and triggering the specific effect)
[RequireComponent(typeof(Collider))]
public class Bonus : MonoBehaviour {
    [Tooltip("What should happen when the bonus is picked up other than the primary effect handled by BonusEffect component (e.g. particles, destroying self).")]
    public UnityEvent pickUpEvent;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerController player = other.GetComponent<PlayerController>();
            // Invoke the specific event
            BonusEffect effect = GetComponent<BonusEffect>();
            if (effect != null)
                effect.ApplyBonusEffect(player);
            // Invoke the common events (e.g. particles, destroying self)
            pickUpEvent.Invoke();
        }
	}

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
