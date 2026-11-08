using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    public MatchManager matchManager;
    public TextMeshProUGUI countdownText;

    private void Start()
    {
        // Hide the countdown when the game starts
        countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (matchManager == null)
        {
            return;
        }

        int seconds = matchManager.countdownSeconds.Value;

        // Hide the text when countdown is finished
        if (seconds <= 0)
        {
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            // Show the countdown number
            countdownText.gameObject.SetActive(true);
            countdownText.text = seconds.ToString();
        }
    }
}
