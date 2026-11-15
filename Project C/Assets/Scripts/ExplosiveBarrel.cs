using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class ExplosiveBarrel : NetworkBehaviour
{
    public ParticleSystem explosionEffect;

    public float explosionForce = 25f;
    public float upwardForce = 10f;

    public float explosionCooldown = 2f;

    [Header("Chaos Mode Settings")]
    public float chaosExplosionMultiplier = 3f;

    // Whether the barrel can currently explode
    private NetworkVariable<bool> isAvailable =
        new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Collider[] barrelColliders;
    private Renderer[] barrelRenderers;

    private void Awake()
    {
        barrelColliders = GetComponentsInChildren<Collider>();
        barrelRenderers = GetComponentsInChildren<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        isAvailable.OnValueChanged += OnAvailabilityChanged;

        UpdateBarrelState(isAvailable.Value);
    }

    public override void OnNetworkDespawn()
    {
        isAvailable.OnValueChanged -= OnAvailabilityChanged;
    }

    private void OnAvailabilityChanged(bool oldValue, bool newValue)
    {
        UpdateBarrelState(newValue);
    }

    private void UpdateBarrelState(bool available)
    {
        // Enable or disable the barrel's collision
        foreach (Collider col in barrelColliders)
        {
            col.enabled = available;
        }

        // Show or hide the barrel
        foreach (Renderer rend in barrelRenderers)
        {
            // Don't hide the particle effect during cooldown
            if (rend is ParticleSystemRenderer)
            {
                continue;
            }

            rend.enabled = available;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || !isAvailable.Value)
        {
            return;
        }

        SoccerBall ball = collision.rigidbody != null
            ? collision.rigidbody.GetComponent<SoccerBall>()
            : null;

        if (ball == null)
        {
            return;
        }

        // Determine the explosion multiplier
        float multiplier = 1f;

        MatchManager matchManager =
            FindFirstObjectByType<MatchManager>();

        if (matchManager != null &&
            matchManager.selectedGameMode.Value == 1)
        {
            multiplier = chaosExplosionMultiplier;
        }

        // Explode the ball
        ball.ExplodeFrom(
            transform.position,
            explosionForce * multiplier,
            upwardForce * multiplier
        );

        // Play the visual explosion for everyone
        PlayExplosionEffectRpc();

        Debug.Log("EXPLOSION!");

        // Start the cooldown
        StartCoroutine(BarrelCooldown());
    }

    private IEnumerator BarrelCooldown()
    {
        // Hide and disable the barrel
        isAvailable.Value = false;

        Debug.Log("Barrel recharging...");

        yield return new WaitForSeconds(explosionCooldown);

        // Bring the barrel back
        isAvailable.Value = true;

        Debug.Log("Barrel ready!");
    }

    [Rpc(SendTo.Everyone)]
    private void PlayExplosionEffectRpc()
    {
        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }
    }
}
