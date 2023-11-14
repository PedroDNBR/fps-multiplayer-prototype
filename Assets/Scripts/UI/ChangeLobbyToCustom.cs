using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeLobbyToCustom : MonoBehaviour
{
    public GameObject lobbyScreen;
    public GameObject customizationScreen;
    public Button button;

    private void Start()
    {
        lobbyScreen.SetActive(true);
        customizationScreen.SetActive(false);

        button.onClick.AddListener(() =>
        {
            lobbyScreen.SetActive(!lobbyScreen.activeSelf);
            customizationScreen.SetActive(!customizationScreen.activeSelf);
        });
    }
}
