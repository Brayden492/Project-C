using Unity.Netcode;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
    // Stamina settings
    public float maxStamina = 100f;
    public float kickCost = 20f;
    public float runCostPerSecond = 15f;
    public float regenerationPerSecond = 15f;
    private bool isRunning;
    public float regenerationDelay = 1.5f;

    private float regenerationTimer;

    // The server controls stamina, but everyone can read it
    public NetworkVariable<float> currentStamina =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        // Give the player full stamina when they spawn
        if (IsServer)
        {
            currentStamina.Value = maxStamina;
        }
    }

    private void Update()
    {
        // Only the server manages stamina
        if (!IsServer)
        {
            return;
        }

        // Count down the regeneration delay
        if (regenerationTimer > 0f)
        {
            regenerationTimer -= Time.deltaTime;
            return;
        }

        // Only regenerate stamina when not running
        if (!isRunning && currentStamina.Value < maxStamina)
        {
            currentStamina.Value +=
                regenerationPerSecond * Time.deltaTime;

            currentStamina.Value = Mathf.Min(
                currentStamina.Value,
                maxStamina
            );
        }
    }

    // Try to spend a specific amount of stamina
    public bool TryUseStamina(float amount)
    {
        if (!IsServer)
        {
            return false;
        }

        // Check whether enough stamina is available
        if (currentStamina.Value < amount)
        {
            return false;
        }

        // Spend the stamina
        currentStamina.Value -= amount;

        return true;
    }

    public void SetRunning(bool running)
    {
        if (!IsServer)
        {
            return;
        }

        isRunning = running;
    }

    // Drain a specific amount of stamina
    public void DrainStamina(float amount)
    {
        if (!IsServer)
        {
            return;
        }

        currentStamina.Value = Mathf.Max(
            0f,
            currentStamina.Value - amount
        );

        // Start regeneration delay when stamina runs out
        if (currentStamina.Value <= 0f)
        {
            regenerationTimer = regenerationDelay;
        }
    }
}
