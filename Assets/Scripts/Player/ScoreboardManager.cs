using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreboardManager : NetworkBehaviour
{
    InputManager inputManager;

    public GameObject UiItem;
    public Transform grid;

    public void Init(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    public void HandleScoreboardData()
    {
        if (inputManager.tab)
            CheckUIUpdatesServerRpc();
    }

    [ServerRpc]
    public void CheckUIUpdatesServerRpc()
    {

        Dictionary<ulong, PlayerInfo> playersConnected = GameMulitiplayerManager.Instance.playersConnected;
        CleanUIClientRpc();
        foreach (KeyValuePair<ulong, PlayerInfo> playerConnected in playersConnected)
        {
            RenderUIClientRpc(playerConnected.Value.nickname, playerConnected.Value.killCount);
        }
    }

    [ClientRpc]
    void CleanUIClientRpc()
    {
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }
    }

    [ClientRpc]
    public void RenderUIClientRpc(string nickname, int killCount)
    {
        GameObject newUIItem = UiItem;
        newUIItem.GetComponentsInChildren<TMP_Text>()[0].text = nickname;
        newUIItem.GetComponentsInChildren<TMP_Text>()[1].text = "Kills: " + killCount;
        newUIItem.SetActive(true);
        Instantiate(newUIItem, grid);
        UiItem.SetActive(false);
    }

}
