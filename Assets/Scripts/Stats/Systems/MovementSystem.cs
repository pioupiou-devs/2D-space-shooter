using UnityEngine;

/// <summary>
/// Manages movement stats: speed, limits
/// </summary>
public class MovementSystem : BaseStatCategory<MovementStatsConfig>
{
    // Default values when config is not assigned
    private const float DefaultMoveSpeed = 5f;
    private const float DefaultMinMoveSpeed = 1f;
    private const float DefaultMaxMoveSpeed = 20f;

    // Properties with modifier support and null checks
    public float MoveSpeed => config != null 
        ? Mathf.Clamp(CalculateStat(config.moveSpeed, "moveSpeed"), config.minMoveSpeed, config.maxMoveSpeed)
        : DefaultMoveSpeed;
    
    public float MinMoveSpeed => config != null ? config.minMoveSpeed : DefaultMinMoveSpeed;
    public float MaxMoveSpeed => config != null ? config.maxMoveSpeed : DefaultMaxMoveSpeed;
    
    /// <summary>
    /// Update movement controller speed if it exists
    /// </summary>
    protected override void OnStatModified(string statName)
    {
        base.OnStatModified(statName);
        
        if (statName == "moveSpeed")
        {
            // Try to update BoundedMovement2D component
            MonoBehaviour movementController = GetComponent("BoundedMovement2D") as MonoBehaviour;
            if (movementController != null)
            {
                var field = movementController.GetType().GetField("moveSpeed");
                if (field != null)
                {
                    field.SetValue(movementController, MoveSpeed);
                }
            }
        }
    }
    
    protected override float GetStatValue(string statName)
    {
        return statName switch
        {
            "moveSpeed" => MoveSpeed,
            _ => 0f
        };
    }
}
