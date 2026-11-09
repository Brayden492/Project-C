using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SoccerBall : NetworkBehaviour
{
    private Rigidbody rb;

    public float playerPushForce = 4f;

    // Controls the strength of an intentional kick
    public float kickForce = 6f;
    public float kickLift = 1.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PushBall(Vector3 direction, float playerSpeed)
    {
        // Only the server controls ball physics
        if (!IsServer)
        {
            return;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude == 0f)
        {
            return;
        }

        direction.Normalize();

        // Apply a push based on the player's movement speed
        rb.AddForce(
            direction * playerPushForce * playerSpeed,
            ForceMode.Impulse
        );
    }

    public void KickBall(Vector3 direction)
    {
        // Only the server controls ball physics
        if (!IsServer)
        {
            return;
        }

        // Keep the kick direction horizontal
        direction.y = 0f;
        direction.Normalize();

        // Apply forward force and a little upward force
        Vector3 kickDirection =
            direction * kickForce + Vector3.up * kickLift;

        rb.AddForce(kickDirection, ForceMode.Impulse);
    }

    public void ExplodeFrom(
    Vector3 explosionOrigin,
    float explosionForce,
    float upwardForce)
    {
        // Only the server applies the explosion
        if (!IsServer)
        {
            return;
        }

        // Calculate the direction away from the explosion
        Vector3 direction = rb.worldCenterOfMass - explosionOrigin;

        // Keep the horizontal direction
        direction.y = 0f;

        direction.Normalize();

        // Apply the explosion force
        Vector3 explosion = direction * explosionForce
                            + Vector3.up * upwardForce;

        rb.AddForce(explosion, ForceMode.Impulse);
    }
}