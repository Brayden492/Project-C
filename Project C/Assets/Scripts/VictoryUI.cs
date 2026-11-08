using TMPro;
using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    public MatchManager matchManager;
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;

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

        // Check whether the match has ended
        if (matchManager.matchOver.Value)
        {
            victoryPanel.SetActive(true);

            // Display the winner
            if (matchManager.player1Score.Value >= matchManager.goalsToWin)
            {
                victoryText.text = "PLAYER 1 WINS!";
            }
            else
            {
                victoryText.text = "PLAYER 2 WINS!";
            }
        }
    }
}
