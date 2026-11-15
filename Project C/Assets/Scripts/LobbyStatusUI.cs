using TMPro;
using UnityEngine;

public class LobbyStatusUI : MonoBehaviour
{
    public MatchManager matchManager;

    public TMP_Text player1StatusText;
    public TMP_Text player2StatusText;

    private int previousPlayerCount = -1;

    private void Update()
    {
        if (matchManager == null)
        {
            return;
        }

        int playerCount = matchManager.connectedPlayerCount.Value;

        // Only update the labels when the count changes
        if (playerCount == previousPlayerCount)
        {
            return;
        }

        previousPlayerCount = playerCount;

        // Update Player 1's status
        player1StatusText.text = playerCount >= 1
            ? "Player 1: Connected"
            : "Player 1: Waiting";

        // Update Player 2's status
        player2StatusText.text = playerCount >= 2
            ? "Player 2: Connected"
            : "Player 2: Waiting for player";
    }
}
