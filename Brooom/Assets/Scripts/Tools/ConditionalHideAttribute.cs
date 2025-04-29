using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Right now this attribute doesn't work properly for fields/properties which are arrays
/// <summary>
/// A custom attribute which can be used to hide the decorated field or property in the Inspector conditionally based on a <c>bool</c> value stored in another field.
/// By default, if the logic is not inversed, the decorated field/property is hidden when the condition evaluates to <c>false</c>.
/// But the logic can be inversed as well, then the decorated field/property is hidden when the condition evaluates to <c>true</c>.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute {

    /// <summary>The name of the <c>bool</c> field that will control visibility of the decorated field or property.</summary>
    public string conditionField = "";
    /// <summary>Whether the control logic should be inversed, i.e. the field/property will be hidden when the condition is <c>true</c>.</summary>
    public bool inverse = false;

    /// <summary>
    /// The decorated field/property is shown or hidden in the Inspector based on value stored in the specified <c>bool</c> field.
    /// If the value is <c>false</c>, the decorated field/property is hidden.
    /// </summary>
    /// <param name="conditionField">The name of the <c>bool</c> field which controls visibility of the decorated field or property.</param>
    public ConditionalHideAttribute(string conditionField) {
        this.conditionField = conditionField;
        this.inverse = false;
    }

    /// <summary>
    /// The decorated field/property is shown or hidden in the Inspector based on value stored in the specified <c>bool</c> field.
    /// </summary>
    /// <param name="conditionField">The name of the <c>bool</c> field which controls visibility of the decorated field or property.</param>
    /// <param name="inverse">If <c>false</c>, decorated field/property is hidden when the condition is <c>false</c>. If <c>true</c>, decorated field/property is hidden when the condition is <c>true</c>.</param>
    public ConditionalHideAttribute(string conditionField, bool inverse) {
        this.conditionField = conditionField;
        this.inverse = inverse;
    }

}
