using UnityEngine;

/// <summary>
/// Interface for injectable shooting strategies
/// </summary>
public interface IShootingStrategy
{
    /// <summary>
    /// Execute the shooting pattern
    /// </summary>
    /// <param name="origin">Position to shoot from</param>
    /// <param name="direction">Base direction to shoot</param>
    /// <param name="damage">Damage per bullet</param>
    /// <param name="bulletPool">Reference to bullet pool</param>
    void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool);
    
    /// <summary>
    /// Get the name of this strategy
    /// </summary>
    string GetStrategyName();
    
    /// <summary>
    /// Get cooldown modifier for this strategy (1.0 = normal, 2.0 = double cooldown)
    /// </summary>
    float GetCooldownModifier();
}
