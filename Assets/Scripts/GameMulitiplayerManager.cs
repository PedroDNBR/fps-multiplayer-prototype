using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameMulitiplayerManager : NetworkBehaviour
{
    [SerializeField]
    [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
    private NetworkObject playerPrefab;

    public Transform[] spawnPoints = new Transform[10];
    public bool isLobby = true;

    public Dictionary<ulong, PlayerInfo> playersConnected = new Dictionary<ulong, PlayerInfo>();

    Dictionary<string, Color> colors = new Dictionary<string, Color>()
    {
        { "White", UnityEngine.Color.white },
        { "Brown", new UnityEngine.Color32(91,46,33, 255) },
        { "Red", UnityEngine.Color.red },
        { "Purple", new UnityEngine.Color32(231, 0, 195, 255) },
        { "Blue", UnityEngine.Color.blue },
        { "Cyan", new UnityEngine.Color32(0, 231, 227, 255) },
        { "Green", UnityEngine.Color.green },
    };

    public static GameMulitiplayerManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (!playersConnected.ContainsKey(clientId))
        {
            Debug.Log("Client connected with id:" + clientId);
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.playerColor = "White";
            playersConnected.Add(clientId, playerInfo);
        }
    }

    [ServerRpc]
    private void StartMatchServerRpc()
    {
        GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        if (spawnpoints.Length == 0) return;

        for (int i = 0; i < spawnpoints.Length; i++)
        {
            spawnPoints[i] = spawnpoints[i].transform;
        }

        /*for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            SpawnNewPlayer(NetworkManager.Singleton.ConnectedClients[(ulong)i].ClientId);
            Debug.Log("Spawnou 1");
        }*/

        foreach (KeyValuePair<ulong, PlayerInfo> playerConnected in playersConnected)
        {
            int spawnIndex = (int)Mathf.Round(UnityEngine.Random.Range(0, spawnPoints.Length - 1));

            var newPlayer = Instantiate(playerPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerConnected.Key, true);
            newPlayer.GetComponent<PlayerManager>().ChangeColor(colors[playerConnected.Value.playerColor]);
        }
    }

    public void StartGameMatch()
    {
        if(NetworkManager.Singleton.IsHost)
            StartMatchServerRpc();
    }

    private void OnLevelWasLoaded()
    {
        if (NetworkManager.Singleton.IsHost)
            StartCoroutine(test());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnNewPlayerServerRpc(ulong clientId)
    {
        int spawnIndex = (int)Mathf.Round(UnityEngine.Random.Range(0, spawnPoints.Length - 1));

        var newPlayer = Instantiate(playerPrefab, spawnPoints[spawnIndex]);
        newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        newPlayer.GetComponent<PlayerManager>().ChangeColor(colors[playersConnected[clientId].playerColor]);
    }

    public float StartTimer = 1;
    IEnumerator test()
    {
        yield return new WaitForSeconds(StartTimer);
        StartMatchServerRpc();
    }
}


public class PlayerInfo {
    public string playerId;
    public string lobbyId;
    public string nickname;

    public int killCount = 0;

    public string playerColor;
}