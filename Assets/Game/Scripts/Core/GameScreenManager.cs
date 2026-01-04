using UnityEngine;
using System;

public class GameScreenManager : MonoBehaviour
{
    public Rect gameScreenBounds; // Define the rectangle area for the game screen
    public event Action<GameObject> OnObjectOutOfBounds; // Event triggered when an object goes out of bounds
    
    public Camera mainCamera; // Reference to the main camera
    public bool followCameraSize = true; // Toggle to enable/disable camera following
    public float boundsPadding = 0f; // Optional padding from camera edges

    private void Start()
    {
        // Get main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Initialize the game screen bounds
        UpdateBoundsFromCamera();
    }

    private void Update()
    {
        if (followCameraSize && mainCamera != null)
        {
            UpdateBoundsFromCamera();
        }
    }

    private void UpdateBoundsFromCamera()
    {
        if (mainCamera == null)
        {
            return;
        }

        // Calculate camera bounds in world space
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector3 cameraCenter = mainCamera.transform.position;

        // Apply padding
        float width = cameraWidth - (boundsPadding * 2f);
        float height = cameraHeight - (boundsPadding * 2f);

        // Update bounds
        gameScreenBounds = new Rect(
            cameraCenter.x - width / 2f,
            cameraCenter.y - height / 2f,
            width,
            height
        );
    }

    public void CheckBounds(GameObject obj)
    {
        Vector3 position = obj.transform.position;

        // Check if the object is outside the bounds
        if (!gameScreenBounds.Contains(new Vector2(position.x, position.y)))
        {
        // Trigger the event
            OnObjectOutOfBounds?.Invoke(obj);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the game screen bounds in the scene view for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gameScreenBounds.center, new Vector3(gameScreenBounds.width, gameScreenBounds.height, 0));
    }
}
