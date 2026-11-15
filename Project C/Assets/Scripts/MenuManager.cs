using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject gameModePanel;
    public GameObject multiplayerPanel;
    public GameObject lobbyPanel;
    public GameObject settingsPanel;
    public MatchManager matchManager;
    public GameObject creditsPanel;
    public int selectedGameMode = 0;

    private void Start()
    {
        ShowMainMenu();
    }

    // Hides all menu panels so only the selected menu is visible
    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        gameModePanel.SetActive(false);
        multiplayerPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowGameMode()
    {
        HideAllPanels();
        gameModePanel.SetActive(true);
    }

    public void ShowMultiplayer()
    {
        HideAllPanels();
        multiplayerPanel.SetActive(true);
    }

    public void ShowLobby()
    {
        HideAllPanels();
        lobbyPanel.SetActive(true);
    }

    public void ShowSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        HideAllPanels();
        creditsPanel.SetActive(true);
    }

    public void SelectNormalMode()
    {
        selectedGameMode = 0;

        //Debug.Log("Selected game mode: Normal");

        ShowMultiplayer();
    }

    public void SelectChaosMode()
    {
        selectedGameMode = 1;

        //Debug.Log("Selected game mode: Chaos");

        ShowMultiplayer();
    }

    // Hide the menus while the kickoff countdown is active and gameplay is starting
    private void Update()
    {
        if (matchManager != null &&
            matchManager.countdownSeconds.Value > 0)
        {
            HideAllPanels();
        }
    }
}
