using System.Globalization;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController controller;

    // Stores the movement input received by the server
    private Vector2 movementInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        // Only the server assigns spawn positions
        if (!IsServer)
        {
            return;
        }

        Vector3 spawnPosition;

        // Host spawns on the left
        if (OwnerClientId == NetworkManager.ServerClientId)
        {
            spawnPosition = new Vector3(-24, 1, 0);
        }
        // Client spawns on the right
        else
        {
            spawnPosition = new Vector3(-22, 1, 0);
        }

        // Get the NetworkTransform component
        NetworkTransform networkTransform = GetComponent<NetworkTransform>();

        // Assign the networked spawn position
        networkTransform.Teleport(
            spawnPosition,
            Quaternion.identity,
            Vector3.one
        );
    }

    private float sendRate = 1f / 30f;
    private float sendTimer;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        sendTimer += Time.deltaTime;

        if (sendTimer >= sendRate)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 input = new Vector2(horizontal, vertical);

            SendMovementServerRpc(input);

            sendTimer = 0f;
        }
    }

    [ServerRpc]
    private void SendMovementServerRpc(Vector2 input)
    {
        movementInput = Vector2.ClampMagnitude(input, 1f);
    }

    private void FixedUpdate()
    {
        // Only the server moves the actual player objects
        if (!IsServer)
        {
            return;
        }

        // Convert input into a 3D movement direction
        Vector3 movement = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        // Only move if the player is actually providing input
        if (movement.sqrMagnitude > 0f)
        {
            controller.Move(movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
