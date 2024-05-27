using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using static GameMulitiplayerManager;
using static LobbyManager;

public class LobbyUI : MonoBehaviour
{


    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private Button changeWhiteButton;
    [SerializeField] private Button changeBrownButton;
    [SerializeField] private Button changeRedButton;
    [SerializeField] private Button changePurpleButton;
    [SerializeField] private Button changeBlueButton;
    [SerializeField] private Button changeCyanButton;
    [SerializeField] private Button changeGreenButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button changeGameModeButton;
    [SerializeField] private Button starGameButton;

    private void Awake()
    {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        changeWhiteButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.White);
        });
        changeBrownButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Brown);
        });
        changeRedButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Red);
        });
        changePurpleButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Purple);
        });
        changeBlueButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Blue);
        });
        changeCyanButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Cyan);
        });
        changeGreenButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Green);
        });

        leaveLobbyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
        });

        changeGameModeButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.ChangeGameMode();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();


    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    public void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private async void UpdateLobby(Lobby lobby)
    {
        ClearLobby();
        int i = 0;
        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            GameMulitiplayerManager.Instance.AddPlayerToDictionaryServerRpc(
                (ulong)i,
                player.Data[LobbyManager.KEY_PLAYER_NAME].Value,
                player.Data[KEY_PLAYER_CHARACTER].Value,
                lobby.Id
            );
            i++;
            /*
            GameMulitiplayerManager.Instance.updateColorClientRpc(
                playerCharacter.ToString(),
                lobby.Id
            );

            GameMulitiplayerManager.Instance.updateNameClientRpc(
                playerName,
                lobby.Id
            );
            */
            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        starGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameModeText.text = lobby.Data[LobbyManager.KEY_GAME_MODE].Value;

        Show();
    }



    private void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

}