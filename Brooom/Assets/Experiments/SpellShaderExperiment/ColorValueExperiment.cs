using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorValueExperiment : MonoBehaviour
{

    [SerializeField] Color color;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(color.a);
    }
}
