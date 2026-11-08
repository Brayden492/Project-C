using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MultiplayerUi : MonoBehaviour
{
    public Transform playerSpawn1;
    public Transform playerSpawn2;

    // Assign the same Player prefab used by NetworkManager
    public NetworkObject playerPrefab;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        // Approve the connection, but don't automatically create a player
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        // Choose the correct spawn point
        Transform spawnPoint = clientId == NetworkManager.ServerClientId
            ? playerSpawn1
            : playerSpawn2;

        if (spawnPoint == null || playerPrefab == null)
        {
            Debug.LogError("Missing spawn point or player prefab");
            return;
        }

        // Create the player directly at the spawn point
        NetworkObject player = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Spawn the player as the connected client's PlayerObject
        player.SpawnAsPlayerObject(clientId);

        Debug.Log(
            "Manually spawned player " + clientId +
            " at " + player.transform.position
        );

        MatchManager matchManager =
        FindFirstObjectByType<MatchManager>();

        if (matchManager != null)
        {
            matchManager.TryStartMatch();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
