using System.Globalization;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController controller;

    // Stores keyboard input received by the server
    private Vector2 movementInput;

    private float sendRate = 1f / 30f;
    private float sendTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Only read input for the local player
        if (!IsOwner)
        {
            return;
        }

        // Read keyboard input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(horizontal, vertical);

        // Prevent faster diagonal movement
        input = Vector2.ClampMagnitude(input, 1f);

        // Send input to the server 30 times per second
        sendTimer += Time.deltaTime;

        if (sendTimer >= sendRate)
        {
            SendMovementServerRpc(input);
            sendTimer = 0f;
        }
    }

    // Receive keyboard input on the server
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

        // Get directions based on where the player is facing
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Calculate movement relative to player rotation
        Vector3 movement =
            forward * movementInput.y +
            right * movementInput.x;

        // Only move when there is input
        if (movement.sqrMagnitude > 0f)
        {
            controller.Move(
                movement * moveSpeed * Time.fixedDeltaTime
            );
        }
    }
}
