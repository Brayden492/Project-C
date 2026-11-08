using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections;

public class MatchManager : NetworkBehaviour
{
    public NetworkVariable<int> player1Score = new NetworkVariable<int>(
          0,
          NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server
      );

    public NetworkVariable<int> player2Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> canMove = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> matchOver = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // The countdown number displayed to both players
    public NetworkVariable<int> countdownSeconds =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public int goalsToWin = 5;
    // Player spawn locations
    public Transform playerSpawn1;
    public Transform playerSpawn2;

    // Ball's starting location
    public Transform ballSpawnPoint;

    // The ball prefab used to create the match ball
    public NetworkObject ballPrefab;

    // Reference to the currently spawned ball
    private SoccerBall soccerBall;

    // Prevents the same goal from awarding multiple points
    private bool goalBeingProcessed = false;

    public void ScoreGoal(int playerWhoScores)
    {
        // Only the server can award points
        if (!IsServer || goalBeingProcessed || matchOver.Value)
        {
            return;
        }

        goalBeingProcessed = true;

        // Award the point to the correct player
        if (playerWhoScores == 1)
        {
            player1Score.Value++;
        }
        else if (playerWhoScores == 2)
        {
            player2Score.Value++;
        }

        Debug.Log(
            "Score: Player 1 = " + player1Score.Value +
            " | Player 2 = " + player2Score.Value
        );

        // Check whether someone has won
        if (player1Score.Value >= goalsToWin ||
            player2Score.Value >= goalsToWin)
        {
            matchOver.Value = true;
            canMove.Value = false;

            Debug.Log(
                player1Score.Value >= goalsToWin
                    ? "PLAYER 1 WINS"
                    : "PLAYER 2 WINS"
            );

            return;
        }

        // Reset everyone after a goal
        ResetPositions();

        // Start another kickoff countdown
        StartCoroutine(StartKickoffCountdown());
    }

    private void ResetPositions()
    {
        // Find both connected players
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject player = client.PlayerObject;

            if (player == null)
            {
                continue;
            }

            // Get the player's NetworkTransform
            NetworkTransform networkTransform =
                player.GetComponent<NetworkTransform>();

            if (networkTransform == null)
            {
                Debug.LogError("Player is missing NetworkTransform");
                continue;
            }

            // Choose the correct spawn point
            Transform spawnPoint = client.ClientId == NetworkManager.ServerClientId
                ? playerSpawn1
                : playerSpawn2;

            // Get the CharacterController
            CharacterController controller =
                player.GetComponent<CharacterController>();

            // Temporarily disable it
            if (controller != null)
            {
                controller.enabled = false;
            }

            // Teleport the player
            networkTransform.Teleport(
                spawnPoint.position,
                spawnPoint.rotation,
                player.transform.localScale
            );

            // Turn the CharacterController back on
            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        // Reset the ball
        Rigidbody ballRigidbody = soccerBall.GetComponent<Rigidbody>();

        soccerBall.transform.SetPositionAndRotation(
            ballSpawnPoint.position,
            ballSpawnPoint.rotation
        );

        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
    }

    public void TryStartMatch()
    {
        // Only the server can spawn the ball
        if (!IsServer)
        {
            return;
        }

        // Wait until both players are connected
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2)
        {
            return;
        }

        // Prevent spawning multiple balls
        if (soccerBall != null)
        {
            return;
        }

        // Create the ball at its spawn point
        NetworkObject ball = Instantiate(
            ballPrefab,
            ballSpawnPoint.position,
            ballSpawnPoint.rotation
        );

        // Spawn the ball across the network
        ball.Spawn();

        // Save a reference to the spawned ball
        soccerBall = ball.GetComponent<SoccerBall>();

        Debug.Log("Match ball spawned");

        StartCoroutine(StartKickoffCountdown());
    }

    private IEnumerator StartKickoffCountdown()
    {
        // Lock both players
        canMove.Value = false;

        // Display 3
        countdownSeconds.Value = 3;
        Debug.Log("Kickoff in 3...");

        yield return new WaitForSeconds(1f);

        // Display 2
        countdownSeconds.Value = 2;
        Debug.Log("Kickoff in 2...");

        yield return new WaitForSeconds(1f);

        // Display 1
        countdownSeconds.Value = 1;
        Debug.Log("Kickoff in 1...");

        yield return new WaitForSeconds(1f);

        // Hide the countdown
        countdownSeconds.Value = 0;

        // Allow players to move
        canMove.Value = true;

        Debug.Log("KICKOFF!");

        // Allow another goal to be processed
        goalBeingProcessed = false;
    }
}
