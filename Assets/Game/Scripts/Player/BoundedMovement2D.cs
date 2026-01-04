using UnityEngine;

/// <summary>
/// Handles player movement within game screen bounds using GameInputManager.
/// </summary>
public class BoundedMovement2D : MonoBehaviour
{
    public GameScreenManager gameScreenManager;
    public float moveSpeed = 5f;

    private Vector3 velocity;
    private Vector3 lastValidPosition;

    private void Start()
    {
        lastValidPosition = transform.position;
        
        // Subscribe to input events
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.EnableGameplay();
        }
    }

    private void Update()
    {
        if (gameScreenManager == null)
        {
            return;
        }

        // Get movement input from GameInputManager
        Vector2 moveInput = Vector2.zero;
        if (GameInputManager.Instance != null)
        {
            moveInput = GameInputManager.Instance.MoveInput;
        }

        // Calculate desired velocity
        velocity = new Vector3(moveInput.x, moveInput.y, 0f).normalized * moveSpeed;

        // Calculate new position
        Vector3 newPosition = transform.position + velocity * Time.deltaTime;

        // Temporarily move to new position
        transform.position = newPosition;

        // Check if out of bounds
        gameScreenManager.CheckBounds(gameObject);

        // If we're within bounds, update last valid position
        if (gameScreenManager.gameScreenBounds.Contains(new Vector2(transform.position.x, transform.position.y)))
        {
            lastValidPosition = transform.position;
        }
        else
        {
            // Revert to last valid position if out of bounds
            transform.position = lastValidPosition;
        }
    }
}
