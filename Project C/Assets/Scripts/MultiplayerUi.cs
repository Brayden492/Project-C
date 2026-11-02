using Unity.Netcode;
using UnityEngine;

public class MultiplayerUi : MonoBehaviour
{
    private void Start()
    {
        // Subscribe after the scene objects have initialized
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // Stop listening when this object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
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
        // Find the player that just connected
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // Give the host and client different starting positions
            if (clientId == NetworkManager.ServerClientId)
            {
                client.PlayerObject.transform.position = new Vector3(-24, 1, 0);
            }
            else
            {
                client.PlayerObject.transform.position = new Vector3(-22, 1, 0);
            }
        }
    }
}
