using TMPro;
using UnityEngine;

public class ScoreboardUI : MonoBehaviour
{
    public MatchManager matchManager;
    public TextMeshProUGUI scoreText;

    private void Update()
    {
        if (matchManager == null)
        {
            return;
        }

        // Get both players' scores
        int player1 = matchManager.player1Score.Value;
        int player2 = matchManager.player2Score.Value;

        // Update the scoreboard
        scoreText.text = player1 + " - " + player2;
    }
}
