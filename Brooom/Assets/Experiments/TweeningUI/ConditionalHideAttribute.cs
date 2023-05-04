using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property |
    System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute {
    public string conditionField = "";
    public bool inverse = false; // true = the field will be hidden when the condition is false

    public ConditionalHideAttribute(string conditionField) {
        this.conditionField = conditionField;
        this.inverse = false;
    }

    public ConditionalHideAttribute(string conditionField, bool inverse) {
        this.conditionField = conditionField;
        this.inverse = inverse;
    }
}
