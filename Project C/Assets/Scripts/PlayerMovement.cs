using System.Globalization;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float kickDistance = 2.5f;

    public float runSpeed = 10f;

    public float jumpHeight = 2f;
    public float gravity = -20f;

    private float verticalVelocity;
    private bool jumpRequested;

    private bool runInput;
    private float currentMoveSpeed;

    private PlayerStamina playerStamina;

    private CharacterController controller;

    // Stores keyboard input received by the server
    private Vector2 movementInput;

    private float sendRate = 1f / 30f;
    private float sendTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        playerStamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        // Only read input for the local player
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            KickBallServerRpc();
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
            SendMovementServerRpc(input, Input.GetKey(KeyCode.LeftShift));
            sendTimer = 0f;
        }
    }

    // Receive keyboard input on the server
    [ServerRpc]
    private void SendMovementServerRpc(Vector2 input, bool running)
    {
        movementInput = Vector2.ClampMagnitude(input, 1f);
        runInput = running;
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        jumpRequested = true;
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

        // Check whether the player can run
        bool canRun =
            runInput &&
            movement.sqrMagnitude > 0f &&
            playerStamina.currentStamina.Value > 0f;

        // Tell the stamina system whether the player is running
        playerStamina.SetRunning(canRun);

        // Choose movement speed
        currentMoveSpeed = canRun ? runSpeed : moveSpeed;

        // Drain stamina while running
        if (canRun)
        {
            playerStamina.DrainStamina(
                playerStamina.runCostPerSecond * Time.fixedDeltaTime
            );
        }

        // Check whether the player is touching the ground
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Allow jumping if the player is grounded
        if (jumpRequested && controller.isGrounded)
        {
            // Calculate the upward speed needed to reach jumpHeight
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Reset the jump request
        jumpRequested = false;

        // Apply gravity over time
        verticalVelocity += gravity * Time.fixedDeltaTime;

        // Add vertical movement to horizontal movement
        movement *= currentMoveSpeed;
        movement.y = verticalVelocity; 

        // Move the player
        controller.Move(movement * Time.fixedDeltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Only the server handles ball collisions
        if (!IsServer)
        {
            return;
        }

        // Check whether the player touched the soccer ball
        SoccerBall ball = hit.gameObject.GetComponent<SoccerBall>();

        if (ball == null)
        {
            return;
        }

        // Get the player's movement direction
        Vector3 pushDirection = transform.forward * movementInput.y +
                                transform.right * movementInput.x;

        // Push the ball
        ball.PushBall(pushDirection, currentMoveSpeed);
    }

    [ServerRpc]
    private void KickBallServerRpc()
    {

        // Find the soccer ball
        SoccerBall ball = FindFirstObjectByType<SoccerBall>();

        if (ball == null)
        {
            return;
        }

        // Check the distance between the player and ball
        float distance = Vector3.Distance(
            transform.position,
            ball.transform.position
        );

        // Only kick if the ball is close enough
        if (distance > kickDistance)
        {
            return;
        }

        // Check whether the player has enough stamina
        if (!playerStamina.TryUseStamina(playerStamina.kickCost))
        {
            return;
        }

        // Kick the ball in the direction the player is facing
        ball.KickBall(transform.forward);
    }
}
