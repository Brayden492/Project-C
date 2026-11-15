using TMPro;
using UnityEngine;


// Displays the winner when the match ends and manages the victory screens visibility
public class VictoryUI : MonoBehaviour
{
    public MatchManager matchManager;
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;
    private bool hasLeftMatch = false;

    private void Start()
    {
        // Hide the victory screen initially
        victoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (matchManager == null)
        {
            return;
        }

        // If the match is not over hide the screen and prepare for the next match.
        if (!matchManager.matchOver.Value)
        {
            hasLeftMatch = false;
            victoryPanel.SetActive(false);
            return;
        }

        // Dont show the victory screen if we left the match.
        if (hasLeftMatch)
        {
            victoryPanel.SetActive(false);
            return;
        }

        // Show the victory screen.
        victoryPanel.SetActive(true);

        if (matchManager.player1Score.Value >= matchManager.goalsToWin)
        {
            victoryText.text = "PLAYER 1 WINS!";
        }
        else
        {
            victoryText.text = "PLAYER 2 WINS!";
        }
    }

    public void HideVictoryScreen()
    {
        hasLeftMatch = true;
        victoryPanel.SetActive(false);
    }

    public void ResetVictoryScreen()
    {
        hasLeftMatch = false;
        victoryPanel.SetActive(false);
    }
}

