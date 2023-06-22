using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// All bonus objects should have a component derived from this one and implement their specific effect
// The method is then called from the common Bonus component
public abstract class BonusEffect : MonoBehaviour
{
    // The primary function of the bonus object (e.g. speed up)
    public abstract void ApplyBonusEffect(PlayerController player);

    // Returns true if the bonus should be available in the level (e.g. bonus increasing mana is available only after acquiring first spell)
    public abstract bool IsAvailable();
}
