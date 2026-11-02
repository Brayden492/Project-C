using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MultiplayerUi : MonoBehaviour
{
    private void Start()
    {
        // Listen for players connecting
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

    private void OnClientConnected(ulong clientId)
    {
        // Only the server assigns spawn positions
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(
            clientId, out var client))
        {
            Vector3 spawnPosition;

            if (clientId == NetworkManager.ServerClientId)
            {
                spawnPosition = new Vector3(-24, 1, 0);
            }
            else
            {
                spawnPosition = new Vector3(-22, 1, 0);
            }

            NetworkTransform networkTransform =
                client.PlayerObject.GetComponent<NetworkTransform>();

            networkTransform.Teleport(
                spawnPosition,
                Quaternion.identity,
                Vector3.one
            );
            Debug.Log("Assigned spawn position: " + spawnPosition);
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
