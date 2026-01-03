using UnityEngine;
using UnityEngine.InputSystem;

public class BoundedMovement2D : MonoBehaviour
{
    public GameScreenManager gameScreenManager;
    public float moveSpeed = 5f;

    private Vector3 velocity;
    private Vector3 lastValidPosition;
    private Vector2 moveInput;

    private void Start()
    {
        // Store initial position as last valid position
        lastValidPosition = transform.position;
    }

    private void Update()
    {
        if (gameScreenManager == null)
        {
            return;
        }

        // Get input using New Input System
        var keyboard = Keyboard.current;
        
        moveInput = Vector2.zero;
        
        if (keyboard != null)
        {
            // Horizontal input
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                moveInput.x = -1f;
            }
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                moveInput.x = 1f;
            }
            
            // Vertical input
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                moveInput.y = -1f;
            }
            else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                moveInput.y = 1f;
            }
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
