using UnityEngine;

/// <summary>
/// ScriptableObject configuration for progression-related stats
/// </summary>
[CreateAssetMenu(fileName = "ProgressionStats", menuName = "Stats/Progression Config")]
public class ProgressionStatsConfig : ScriptableObject
{
    [Header("Leveling")]
    [Tooltip("Starting level")]
    public int startingLevel = 1;
    
    [Tooltip("Experience points required per level")]
    public float xpPerLevel = 100f;
    
    [Header("Level Up Bonuses")]
    [Tooltip("Health gained per level")]
    public float healthPerLevel = 10f;
    
    [Tooltip("Damage gained per level")]
    public float damagePerLevel = 2f;
    
    [Tooltip("Speed gained per level")]
    public float speedPerLevel = 0.2f;
    
    [Header("Options")]
    [Tooltip("Fully restore health and shield on level up")]
    public bool restoreOnLevelUp = true;
}
