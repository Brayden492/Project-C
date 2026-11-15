using TMPro;
using UnityEngine;

// Displays the currently selected game mode to both players
public class GameModeUI : MonoBehaviour
{
    public MatchManager matchManager;
    public TMP_Text gameModeText;

    private void Update()
    {
        if (matchManager == null)
        {
            return;
        }

        if (matchManager.selectedGameMode.Value == 0)
        {
            gameModeText.text = "Game Mode: Normal";
        }
        else if (matchManager.selectedGameMode.Value == 1)
        {
            gameModeText.text = "Game Mode: Chaos";
        }
    }
}
