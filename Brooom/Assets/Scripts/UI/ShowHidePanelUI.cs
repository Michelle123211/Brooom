using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHidePanelUI : MonoBehaviour {
    public void ShowPanel() {
        gameObject.TweenAwareEnable();
    }

    public void HidePanel() {
        gameObject.TweenAwareDisable();
    }
}
