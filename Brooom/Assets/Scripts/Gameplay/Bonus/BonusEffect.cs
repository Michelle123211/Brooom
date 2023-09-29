using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// All bonus objects should have a component derived from this one and implement their specific effect
// The method is then called from the common Bonus component
public abstract class BonusEffect : MonoBehaviour
{
    [Tooltip("Weight determines importance of this specific type of bonus. This is taken into account e.g. when computing player stats based on missed bonuses.")]
    public int bonusWeight = 1;

    // The primary function of the bonus object (e.g. speed up)
    public abstract void ApplyBonusEffect(CharacterMovementController character);

    // Returns true if the bonus should be available in the level (e.g. bonus increasing mana is available only after acquiring first spell)
    public abstract bool IsAvailable();
}
