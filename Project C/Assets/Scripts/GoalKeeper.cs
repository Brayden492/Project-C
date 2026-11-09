using UnityEngine;
using Unity.Netcode;

public class GoalKeeper : NetworkBehaviour
{
    public float movementDistance = 3f;
    public float movementSpeed = 1.5f;

    private Vector3 startingPosition;
    private Vector3 movementDirection;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        // Remember the starting position
        startingPosition = transform.position;

        // Move along the object's right direction
        movementDirection = transform.right;
    }

    private void Update()
    {
        // Only the server controls movement
        if (!IsServer)
        {
            return;
        }

        // Calculate smooth back-and-forth movement
        float movement = Mathf.Sin(Time.time * movementSpeed)
                         * movementDistance;

        // Move the goalkeeper
        transform.position = startingPosition +
                             movementDirection * movement;
    }
}
