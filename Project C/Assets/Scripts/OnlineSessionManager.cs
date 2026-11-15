using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

public class OnlineSessionManager : MonoBehaviour
{
    public TMP_InputField joinCodeInput;
    public TMP_Text roomCodeText;
    public MenuManager menuManager;
    public MatchManager matchManager;
    public VictoryUI victoryUI;

    private ISession currentSession;


    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback +=
            HandleClientDisconnect;
    }

    // Handles what happens when a client loses connection to the host
    private void HandleClientDisconnect(ulong clientId)
    {
        // Only handle this on a client, not the host
        if (NetworkManager.Singleton.IsServer)
        {
            return;
        }

        // Make sure it is our own connection that was disconnected
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        Debug.Log("Disconnected from the host");

        currentSession = null;
        roomCodeText.text = "";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        victoryUI.HideVictoryScreen();
        menuManager.ShowMultiplayer();
    }

    // Creates a two player online session using Unity Relay and generates a room code
    public async void CreateRoom()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("Player is not signed in yet");
            return;
        }

        try
        {
            SessionOptions options = new SessionOptions
            {
                MaxPlayers = 2
            }.WithRelayNetwork();

            currentSession =
                await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log("Room created. Code: " + currentSession.Code);

            // Host saves the selected game mode to the network
            matchManager.selectedGameMode.Value =
                menuManager.selectedGameMode;

            Debug.Log("Network game mode set to: " +
                matchManager.selectedGameMode.Value);

            roomCodeText.text = currentSession.Code;
            menuManager.ShowLobby();
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Room creation failed: " + exception.Message);
        }
    }

    // Joins an existing online session using the room code entered by the player
    public async void JoinRoom()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("Player is not signed in yet");
            return;
        }

        try
        {
            string code = joinCodeInput.text.Trim();

            currentSession =
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            Debug.Log("Joined room: " + currentSession.Code);

            roomCodeText.text = currentSession.Code;
            menuManager.ShowLobby();
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Joining room failed: " + exception.Message);
        }
    }

    public async void LeaveRoom()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No active session to leave");
            return;
        }

        // If this player is the client ask the host to close the room.
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Requesting the host to close the room");

            matchManager.RequestHostCloseRoomRpc();
            return;
        }

        // The host leaves closing the session for everyone
        try
        {
            // Reset match data before closing the room
            matchManager.player1Score.Value = 0;
            matchManager.player2Score.Value = 0;
            matchManager.matchOver.Value = false;
            matchManager.canMove.Value = false;
            matchManager.countdownSeconds.Value = 0;

            await currentSession.LeaveAsync();

            Debug.Log("Host left the room. Everyone is being disconnected.");

            currentSession = null;
            roomCodeText.text = "";
            victoryUI.HideVictoryScreen();
            menuManager.ShowMultiplayer();
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Failed to leave room: " + exception.Message);
        }
    }

    public void CopyRoomCode()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No room code to copy");
            return;
        }

        GUIUtility.systemCopyBuffer = currentSession.Code;

        //Debug.Log("Room code copied");
    }
}
