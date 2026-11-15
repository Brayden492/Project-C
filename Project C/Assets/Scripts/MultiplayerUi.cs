using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MultiplayerUi : MonoBehaviour
{
    public Transform playerSpawn1;
    public Transform playerSpawn2;

    public NetworkObject playerPrefab;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;
    }

    public void StartMatch()
    {
        // Only the host can start the match
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Only the host can start the match!");
            return;
        }

        // Make sure both players are connected
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2)
        {
            Debug.Log("Waiting for the second player!");
            return;
        }

        // Spawn each connected player's character
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Don't spawn a duplicate player
            if (client.PlayerObject != null)
                continue;

            Transform spawnPoint = client.ClientId == NetworkManager.ServerClientId
                ? playerSpawn1
                : playerSpawn2;

            if (spawnPoint == null || playerPrefab == null)
            {
                Debug.LogError("Missing spawn point or player prefab!");
                return;
            }

            NetworkObject player = Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            player.SpawnAsPlayerObject(client.ClientId);

            Debug.Log("Spawned player " + client.ClientId);
        }

        // Start the actual match
        MatchManager matchManager =
            FindFirstObjectByType<MatchManager>();

        if (matchManager != null)
        {
            matchManager.TryStartMatch();
        }
        else
        {
            Debug.LogError("MatchManager not found!");
        }
    }
}
