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

    public NetworkVariable<int> connectedPlayerCount =
    new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> selectedGameMode =
    new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public int goalsToWin = 5;
    // Player spawn locations
    public Transform playerSpawn1;
    public Transform playerSpawn2;

    // Balls starting location
    public Transform ballSpawnPoint;

    // The ball prefab used to create the match ball
    public NetworkObject ballPrefab;

    // Reference to the currently spawned ball
    private SoccerBall soccerBall;

    // Prevents the same goal from awarding multiple points
    private bool goalBeingProcessed = false;

    public AudioManager audioManager;
    public GoalCelebration goalCelebration;

    private void UpdateCursor(bool unlocked)
    {
        Cursor.lockState = unlocked
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = unlocked;
    }

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

        //Debug.Log(
        //    "Score: Player 1 = " + player1Score.Value +
        //    " | Player 2 = " + player2Score.Value
        //);

        // Check whether someone has won
        if (player1Score.Value >= goalsToWin ||
            player2Score.Value >= goalsToWin)
        {
            matchOver.Value = true;
            canMove.Value = false;
            UpdateCursor(true);

            // Stop background music
            if (audioManager != null)
            {
                audioManager.StopMusic();
            }

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

    // Returns both players and the ball to their starting positions after a goal or replay
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

        //Debug.Log("Match ball spawned");

        StartCoroutine(StartKickoffCountdown());
    }

    public void RequestReplay()
    {
        //Debug.Log("Replay button clicked");

        if (IsServer)
        {
            //Debug.Log("This player is the host");
            ReplayMatch();
        }
        else
        {
            //Debug.Log("Sending replay request to host");
            ReplayMatchRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void ReplayMatchRpc()
    {
        //Debug.Log("Host received replay request");
        ReplayMatch();
    }

    private void ReplayMatch()
    {
        //Debug.Log("ReplayMatch started");

        if (!IsServer || !matchOver.Value)
        {
            Debug.LogWarning("Replay blocked. IsServer: " + IsServer +
                             " MatchOver: " + matchOver.Value);
            return;
        }

        player1Score.Value = 0;
        player2Score.Value = 0;

        matchOver.Value = false;
        UpdateCursor(false);
        goalBeingProcessed = false;


        ResetPositions();

        StartCoroutine(StartKickoffCountdown());

        //Debug.Log("Replaying match");
    }

    // Handles the kickoff countdown keeping players locked until the match is ready to resume
    private IEnumerator StartKickoffCountdown()
    {
        // Lock both players
        canMove.Value = false;

        // Display 3
        countdownSeconds.Value = 3;
        //Debug.Log("Kickoff in 3...");

        yield return new WaitForSeconds(1f);

        // Display 2
        countdownSeconds.Value = 2;
        //Debug.Log("Kickoff in 2...");

        yield return new WaitForSeconds(1f);

        // Display 1
        countdownSeconds.Value = 1;
        //Debug.Log("Kickoff in 1...");

        yield return new WaitForSeconds(1f);

        // Hide the countdown
        countdownSeconds.Value = 0;

        // Allow players to move
        canMove.Value = true;

        // Start background music
        if (audioManager != null)
        {
            audioManager.StartMusic();
        }


        //Debug.Log("Kickoff");

        // Allow another goal to be processed
        goalBeingProcessed = false;
    }

    public override void OnNetworkSpawn()
    {
        // Listen for goals scored by either player
        player1Score.OnValueChanged += HandlePlayer1ScoreChanged;
        player2Score.OnValueChanged += HandlePlayer2ScoreChanged;

        // Only the server tracks connected players
        if (!IsServer)
        {
            return;
        }

        // Listen for players joining and leaving
        NetworkManager.Singleton.OnClientConnectedCallback += UpdatePlayerCount;
        NetworkManager.Singleton.OnClientDisconnectCallback += UpdatePlayerCount;

        // Set the initial count
        connectedPlayerCount.Value =
            NetworkManager.Singleton.ConnectedClientsIds.Count;

        //Debug.Log("Initial player count: " +
            //connectedPlayerCount.Value);
    }

    private void UpdatePlayerCount(ulong clientId)
    {
        if (!IsServer)
        {
            return;
        }

        connectedPlayerCount.Value =
            NetworkManager.Singleton.ConnectedClientsIds.Count;

        //Debug.Log("Connected players: " + connectedPlayerCount.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= UpdatePlayerCount;
            NetworkManager.Singleton.OnClientDisconnectCallback -= UpdatePlayerCount;
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestHostCloseRoomRpc()
    {
        if (!IsServer) return;

        Debug.Log("A player left closing the room for everyone");

        OnlineSessionManager sessionManager =
            FindFirstObjectByType<OnlineSessionManager>();

        if (sessionManager != null)
        {
            sessionManager.LeaveRoom();
        }
    }

    private void HandlePlayer1ScoreChanged(int oldScore, int newScore)
    {
        if (newScore > oldScore && goalCelebration != null)
        {
            goalCelebration.ShowGoal(1);
        }
    }

    private void HandlePlayer2ScoreChanged(int oldScore, int newScore)
    {
        if (newScore > oldScore && goalCelebration != null)
        {
            goalCelebration.ShowGoal(2);
        }
    }
}
