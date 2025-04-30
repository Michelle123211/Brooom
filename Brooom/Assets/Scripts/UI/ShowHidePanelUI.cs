using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a UI panel which can be shown or hidden with a tween (if it has <c>GenericTween</c> component attached as well).
/// </summary>
public class ShowHidePanelUI : MonoBehaviour {

    /// <summary>
    /// Shows the panel using a tween (if it has <c>GenericTween</c> component attached), or by simply activating it (if it doesn't).
    /// </summary>
    public void ShowPanel() {
        gameObject.TweenAwareEnable();
    }

    /// <summary>
    /// Hides the panel using a tween (if it has <c>GenericTween</c> component attached), or by simply deactivating it (if it doesn't).
    /// </summary>
    public void HidePanel() {
        gameObject.TweenAwareDisable();
    }

}
