using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class PlayerCameraController : NetworkBehaviour
{
    public Transform cameraPivot;
    public float mouseSensitivity = 200f;

    private float verticalRotation;
    private float horizontalRotation;

    private float sendRate = 1f / 30f;
    private float sendTimer;

    public override void OnNetworkSpawn()
    {
        Camera playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            playerCamera.enabled = IsOwner;

            AudioListener listener =
                playerCamera.GetComponent<AudioListener>();

            if (listener != null)
            {
                listener.enabled = IsOwner;
            }
        }

        if (IsOwner)
        {
            horizontalRotation = transform.eulerAngles.y;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis("Mouse X") *
                       mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") *
                       mouseSensitivity * Time.deltaTime;

        // Update horizontal rotation
        horizontalRotation += mouseX;

        // Update vertical camera rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -45f, 60f);

        cameraPivot.localRotation =
            Quaternion.Euler(verticalRotation, 0f, 0f);

        // Send horizontal rotation to the server
        sendTimer += Time.deltaTime;

        if (sendTimer >= sendRate)
        {
            SetPlayerRotationServerRpc(horizontalRotation);
            sendTimer = 0f;
        }
    }

    [ServerRpc]
    private void SetPlayerRotationServerRpc(float yaw)
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
