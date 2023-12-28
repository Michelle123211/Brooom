using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentSpellManager : MonoBehaviour
{
    public List<ExperimentSpellData> availableSpells;

    // Start is called before the first frame update
    void Start()
    {
        if (availableSpells != null) {
            foreach (var spell in availableSpells) {
                spell.effect.ApplyEffect(gameObject);
            }
        }
    }
}
