using UnityEngine;

/// <summary>
/// Interface for all visual feedback components
/// Provides a contract for initializing and managing visual effects
/// </summary>
public interface IVisualFeedback
{
    /// <summary>
    /// Initialize the visual feedback component with player stats
    /// Subscribe to relevant events here
    /// </summary>
    /// <param name="statsManager">Player stats manager to observe</param>
    void Initialize(PlayerStatsManager statsManager);
    
    /// <summary>
    /// Show the visual effect
    /// </summary>
    void Show();
    
    /// <summary>
    /// Hide the visual effect
    /// </summary>
    void Hide();
    
    /// <summary>
    /// Update the visual based on a normalized value (0-1)
    /// </summary>
    /// <param name="normalizedValue">Value between 0 and 1</param>
    void UpdateVisual(float normalizedValue);
    
    /// <summary>
    /// Cleanup and unsubscribe from events
    /// </summary>
    void Cleanup();
}
