using UnityEngine;

public class OutOfBoundsHandler : MonoBehaviour
{
    public GameScreenManager gameScreenManager;

    private void Start()
    {
        if (gameScreenManager != null)
        {
            gameScreenManager.OnObjectOutOfBounds += HandleOutOfBounds;
        }
    }

    private void HandleOutOfBounds(GameObject obj)
    {
        Debug.Log($"{obj.name} is blocked at bounds!");

        // Clamp the object's position to stay within bounds
        Vector3 clampedPosition = obj.transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, 
            gameScreenManager.gameScreenBounds.xMin, 
            gameScreenManager.gameScreenBounds.xMax);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, 
            gameScreenManager.gameScreenBounds.yMin, 
            gameScreenManager.gameScreenBounds.yMax);
        
        obj.transform.position = clampedPosition;
    }

    private void OnDestroy()
    {
        if (gameScreenManager != null)
        {
            gameScreenManager.OnObjectOutOfBounds -= HandleOutOfBounds;
        }
    }
}