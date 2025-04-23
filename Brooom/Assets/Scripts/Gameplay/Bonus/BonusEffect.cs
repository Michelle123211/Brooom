using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base component representing a bonus effect in general.
/// Different bonus objects should have a component derived from this one and implement their specific effect.
/// </summary>
public abstract class BonusEffect : MonoBehaviour
{
    [Tooltip("Weight determines importance of this specific type of bonus. This is taken into account e.g. when computing player stats based on missed bonuses.")]
    public int bonusWeight = 1;

    /// <summary>
    /// Performs the primary function of the bonus.
    /// </summary>
    /// <param name="character">Racer who picked this bonus up and should be affected by it.</param>
    public abstract void ApplyBonusEffect(CharacterMovementController character);

    /// <summary>
    /// Checks whether this bonus type is available and therefore could be placed in the level (e.g. bonus increasing mana is available only after acquiring first spell).
    /// </summary>
    /// <returns><c>true</c> if the bonus is available and can be placed in track, <c>false</c> otherwise.</returns>
    public abstract bool IsAvailable();
}
