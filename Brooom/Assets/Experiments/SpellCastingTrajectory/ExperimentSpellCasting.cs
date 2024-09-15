using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentSpellCasting : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    [SerializeField] ExperimentSpellCastEffect spellCastObjectPrefab;

    ExperimentSpellCastEffect spellObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Cast spell
            spellObject = Instantiate<ExperimentSpellCastEffect>(spellCastObjectPrefab);
            spellObject.transform.position = transform.position;
            spellObject.InitializeStartAndTarget(new SpellCastParameters { SourceObject = gameObject, Target = new SpellTarget { TargetObject = targetObject } });
            spellObject.StartPlaying();
        }
    }
}
