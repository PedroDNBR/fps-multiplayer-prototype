using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMulitiplayerManager : NetworkBehaviour
{
    [SerializeField]
    [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
    private Transform playerPrefab;

    public Transform[] spawnPoints = new Transform[10];
    public bool isLobby = true;

    public bool testSpawn = false;

    public Dictionary<ulong, PlayerInfo> playersConnected = new Dictionary<ulong, PlayerInfo>();

    public NetworkObject testObject;

    public Dictionary<string, Inventory> playersInventory = new Dictionary<string, Inventory>();

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

    [ClientRpc]
    public void updateNameClientRpc(string playerName, string lobbyId)
    {
        Debug.Log("updateNameClientRpc");
        Debug.Log("IsServer: " + NetworkManager.Singleton.IsServer);
        Debug.Log("LocalClientId: " + NetworkManager.Singleton.LocalClientId);

        AddPlayerToDictionaryServerRpc(
                NetworkManager.Singleton.LocalClientId,
                playerName,
                null,
                lobbyId
            );
    }

    [ClientRpc]
    public void updateColorClientRpc(string playerColor, string lobbyId)
    {
        Debug.Log("updateColorClientRpc");
        Debug.Log("IsServer: " + NetworkManager.Singleton.IsServer);
        Debug.Log("LocalClientId: " + NetworkManager.Singleton.LocalClientId);

        AddPlayerToDictionaryServerRpc(
                NetworkManager.Singleton.LocalClientId,
                null,
                playerColor,
                lobbyId
            );
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
            if (spawnpoints.Length == 0) return;

            for (int i = 0; i < spawnpoints.Length; i++)
            {
                spawnPoints[i] = spawnpoints[i].transform;
            }

            int spawnIndex = (int)Mathf.Round(UnityEngine.Random.Range(0, spawnPoints.Length - 1));
            Transform playerTransform = Instantiate(playerPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            PlayerInfo playerInfo = playersConnected[clientId];

            foreach (KeyValuePair<ulong, PlayerInfo> test in playersConnected)
            {
                print(test.Value.nickname);
                print(test.Value.playerColor);
            }

            if (playerInfo != null)
            {
                playerTransform.GetComponent<PlayerManager>().ChangeColor(colors[playerInfo.playerColor]);
                playerTransform.GetComponent<PlayerManager>().SetInventory(playerInfo.nickname);
            }

            else
            {
                Debug.Log("N�o estava na lista por algum motivo: " + clientId);
            }
        }
    }

    private void Update()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;

        /*if (testSpawn)
        {
            testSpawnServerRpc();
            testSpawn = false;
        }*/
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (!playersConnected.ContainsKey(clientId))
        {
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.playerColor = "White";
            playersConnected.Add(clientId, playerInfo);
        }
    }

    /*[ServerRpc]
    void testSpawnServerRpc()
    {
        foreach (KeyValuePair<ulong, PlayerInfo> playerConnected in playersConnected)
        {
            var newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerConnected.Key, true);
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
    */

    [ServerRpc(RequireOwnership = false)]
    public void SpawnNewPlayerServerRpc(ulong clientId)
    {
        int spawnIndex = (int)Mathf.Round(UnityEngine.Random.Range(0, spawnPoints.Length - 1));
        Transform playerTransform = Instantiate(playerPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        PlayerInfo playerInfo = playersConnected[clientId];
        if (playerInfo != null)
        {
            playerTransform.GetComponent<PlayerManager>().ChangeColor(colors[playerInfo.playerColor]);
            playerTransform.GetComponent<PlayerManager>().SetInventory(playerInfo.nickname);
        }
        else
        {
            Debug.Log("N�o estava na lista por algum motivo: " + clientId);
        }
    }

    /*
    public float StartTimer = 20f;
    IEnumerator test()
    {
        yield return new WaitForSeconds(StartTimer);
        StartMatchServerRpc();
    }
    */

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerToDictionaryServerRpc(ulong localId, string playerName, string character, string lobbyId)
    {
        if (playersConnected.ContainsKey(localId))
        {
            PlayerInfo playerInfo = playersConnected[localId];

            if(playerName != null) playerInfo.nickname = playerName;

            if (character != null) playerInfo.playerColor = character;

            playerInfo.lobbyId = lobbyId;

            playersConnected[localId] = playerInfo;
        } 
        else
        {
            PlayerInfo playerInfo = new PlayerInfo();
            Debug.Log("Entrou no else lá que vc sabe qual é");

            playerInfo.nickname = playerName;

            playerInfo.playerColor = character;

            playersConnected[localId] = playerInfo;
        }
        Debug.Log("ServerRpc");
        Debug.Log("localId: " + localId);
        Debug.Log("playerName: " + playerName);
        Debug.Log("character: " + character);
    }
}


public class PlayerInfo
{
    public string lobbyId;
    public string nickname;

    public int killCount = 0;

    public string playerColor;
}