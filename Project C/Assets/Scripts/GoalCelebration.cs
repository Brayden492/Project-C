using TMPro;
using UnityEngine;
using System.Collections;

public class GoalCelebration : MonoBehaviour
{
    public GameObject celebrationPanel;
    public TextMeshProUGUI celebrationText;

    public ParticleSystem player1GoalEffect;
    public ParticleSystem player2GoalEffect;

    private Coroutine hideCoroutine;

    private void Start()
    {
        // Hide the celebration when the game starts
        celebrationPanel.SetActive(false);
    }

    // Displays the goal celebration and plays the effect at the correct goal and starts a timer to hide the message
    public void ShowGoal(int playerWhoScored)
    {
        // Display which player scored
        celebrationText.text = "GOAL: PLAYER " +
            playerWhoScored + " SCORED!";

        // Show the celebration
        celebrationPanel.SetActive(true);

        // Play the explosion at the correct goal
        if (playerWhoScored == 1)
        {
            player1GoalEffect.Play();
        }
        else if (playerWhoScored == 2)
        {
            player2GoalEffect.Play();
        }

        // Restart the hide timer
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        // Keep the message visible for 2 seconds
        yield return new WaitForSeconds(2f);

        celebrationPanel.SetActive(false);
        hideCoroutine = null;
    }
}
