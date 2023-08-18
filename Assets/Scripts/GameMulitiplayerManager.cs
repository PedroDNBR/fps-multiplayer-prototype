using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class GameMulitiplayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
    private NetworkObject playerPrefab;

    public Transform[] spawnPoints = new Transform[10];

    public bool isLobby = true;

    [ServerRpc]
    private void StartMatchServerRpc()
    {
        Debug.Log("Spawnando");
        GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        if (spawnpoints.Length == 0) return;

        for (int i = 0; i < spawnpoints.Length; i++)
        {
            spawnPoints[i] = spawnpoints[i].transform;
        }

        for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            SpawnNewPlayer(NetworkManager.Singleton.ConnectedClients[(ulong)i].ClientId);
            Debug.Log("Spawnou 1");
        }
    }

    public void StartGameMatch()
    {
        StartMatchServerRpc();
    }

    private void Start()
    {
        if(NetworkManager.Singleton.IsHost) StartMatchServerRpc();
    }

    public void SpawnNewPlayer(ulong clientId)
    {
        int spawnIndex = (int)Mathf.Round(Random.Range(0, spawnPoints.Length - 1));
        var newPlayer = Instantiate(playerPrefab, spawnPoints[spawnIndex]);

        newPlayer.SpawnWithOwnership(clientId, true);
    }
}
