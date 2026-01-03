using UnityEngine;

/// <summary>
/// Manages movement stats: speed, limits
/// </summary>
public class MovementSystem : BaseStatCategory<MovementStatsConfig>
{
    // Properties with modifier support
    public float MoveSpeed => Mathf.Clamp(
        CalculateStat(config.moveSpeed, "moveSpeed"),
        config.minMoveSpeed,
        config.maxMoveSpeed
    );
    
    public float MinMoveSpeed => config.minMoveSpeed;
    public float MaxMoveSpeed => config.maxMoveSpeed;
    
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
