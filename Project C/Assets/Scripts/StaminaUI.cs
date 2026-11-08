using UnityEngine;
using UnityEngine.UI;


public class StaminaUI : MonoBehaviour
{
    public Slider staminaSlider;

    private PlayerStamina playerStamina;

    private void Update()
    {
        // Find the local player's stamina
        if (playerStamina == null)
        {
            PlayerStamina[] players =
                FindObjectsByType<PlayerStamina>(
                    FindObjectsSortMode.None
                );

            foreach (PlayerStamina player in players)
            {
                if (player.IsOwner)
                {
                    playerStamina = player;
                    break;
                }
            }

            return;
        }

        // Update the stamina bar
        staminaSlider.maxValue = playerStamina.maxStamina;
        staminaSlider.value = playerStamina.currentStamina.Value;
    }
}
