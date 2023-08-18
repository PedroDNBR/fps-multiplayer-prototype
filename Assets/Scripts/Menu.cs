using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Networking;

public class Menu : MonoBehaviour
{
    public TMP_InputField input;
    public GameObject[] toDestroy;

    Lobby hostLobby;
    float heartBeatTimer;

    private async void Start()
    {
        GameMulitiplayerManager gameMulitiplayerManager = FindAnyObjectByType<GameMulitiplayerManager>();
        if (gameMulitiplayerManager == null) return;
        if (gameMulitiplayerManager.isLobby) return;
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Singed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    { 
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);
            GUIUtility.systemCopyBuffer = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            DestroyAllObjects();
        } catch(RelayServiceException e) {
            Debug.LogError(e);
        }
    }

    public async void JoinRelay()
    {
        string joinCode = input.text;
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            DestroyAllObjects();
        }
        catch (RelayServiceException e) {
            Debug.LogError(e);
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "myLobby";
            int maxPlayers = 8;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;



            Debug.Log("Created Lobby " + lobby.Name + " with max of " + lobby.MaxPlayers + " Players");
        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);

            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby " + lobby.Name + " with max of " + lobby.MaxPlayers + " Players");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
    }

    async void HandleLobbyHeartBeat()
    {
        if (hostLobby == null) return;


        heartBeatTimer -= Time.deltaTime;
        if(heartBeatTimer < 0)
        {
            float heartBeatTimerMax = 10;
            heartBeatTimer = heartBeatTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    void DestroyAllObjects()
    {
        for (int i = 0; i < toDestroy.Length; i++)
        {
            Destroy(toDestroy[i]);
        }

        this.GetComponent<Camera>().enabled = false;
    }
}
