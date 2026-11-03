using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class PlayerCameraController : NetworkBehaviour
{
    public Transform cameraPivot;
    public float mouseSensitivity = 200f;

    private float verticalRotation;

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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        // Unlock mouse with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Lock mouse again when clicking
        if (Input.GetMouseButtonDown(0) &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Read mouse movement
        float mouseX = Input.GetAxis("Mouse X") *
                       mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") *
                       mouseSensitivity * Time.deltaTime;

        // Rotate the player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -45f, 60f);

        cameraPivot.localRotation =
            Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
