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

    public bool testSpawn = false;

    public Dictionary<ulong, PlayerInfo> playersConnected = new Dictionary<ulong, PlayerInfo>();

    public NetworkObject testObject;

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

        if (testSpawn)
        {
            testSpawnServerRpc();
            testSpawn = false;
        }
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
    void testSpawnServerRpc()
    {
        foreach (KeyValuePair<ulong, PlayerInfo> playerConnected in playersConnected)
        {
            var aa = Instantiate(testObject, Vector3.zero, Quaternion.identity);
            aa.Spawn();
            var newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log(newPlayer.GetComponent<NetworkObject>());
            Debug.Log(playerConnected.Key);
            newPlayer.SpawnAsPlayerObject(playerConnected.Key, true);
            newPlayer.GetComponent<PlayerManager>().ChangeColor(colors[playerConnected.Value.playerColor]);
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

        foreach (KeyValuePair<ulong, PlayerInfo> playerConnected in playersConnected)
        {
            int spawnIndex = (int)Mathf.Round(UnityEngine.Random.Range(0, spawnPoints.Length - 1));

            var newPlayer = Instantiate(playerPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerConnected.Key, true);
            newPlayer.GetComponent<PlayerManager>().ChangeColor(colors[playerConnected.Value.playerColor]);
            Debug.Log(newPlayer);
            Debug.Log(playerConnected.Key);
        }
    }

    public void StartGameMatch()
    {
        if (NetworkManager.Singleton.IsHost)
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

    public float StartTimer = 5f;
    IEnumerator test()
    {
        yield return new WaitForSeconds(StartTimer);
        StartMatchServerRpc();
    }
}


public class PlayerInfo
{
    public string playerId;
    public string lobbyId;
    public string nickname;

    public int killCount = 0;

    public string playerColor;
}