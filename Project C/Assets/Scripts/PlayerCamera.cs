using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Only enable the camera for its owning player
        Camera playerCamera = GetComponentInChildren<Camera>();

        playerCamera.enabled = IsOwner;
    }
}
