using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class GameMulitiplayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
    private NetworkObject playerPrefab;

    public Transform[] spawnPoints;

    public void SpawnNewPlayer(ulong clientId)
    {
        int spawnIndex = (int)Mathf.Round(Random.Range(0, spawnPoints.Length - 1));
        var newPlayer = Instantiate(playerPrefab, spawnPoints[spawnIndex]);

        newPlayer.SpawnWithOwnership(clientId, true);
    }
}
