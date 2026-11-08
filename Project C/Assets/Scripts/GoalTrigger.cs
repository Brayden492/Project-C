using Unity.Netcode;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public int playerWhoScores;
    public MatchManager matchManager;

    private void OnTriggerEnter(Collider other)
    {
        // Only the server handles scoring
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        // Check whether the object entering is the soccer ball
        SoccerBall ball = other.GetComponent<SoccerBall>();

        if (ball == null)
        {
            return;
        }

        // Award the point
        matchManager.ScoreGoal(playerWhoScores);
    }
}
