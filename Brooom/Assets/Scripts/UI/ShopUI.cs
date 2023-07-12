using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopUI : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI moneyText;

    public void StartTestingTrack() {
        SceneLoader.Instance.LoadScene(Scene.TestingTrack);
    }

	private void OnEnable() {
		moneyText.text = PlayerState.Instance.money.ToString();	
	}
}
