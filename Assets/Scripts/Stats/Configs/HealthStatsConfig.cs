using UnityEngine;

/// <summary>
/// ScriptableObject configuration for health-related stats
/// </summary>
[CreateAssetMenu(fileName = "HealthStats", menuName = "Stats/Health Config")]
public class HealthStatsConfig : ScriptableObject
{
    [Header("Health")]
    [Tooltip("Maximum health points")]
    public float maxHealth = 100f;
    
    [Header("Shield")]
    [Tooltip("Maximum shield points")]
    public float maxShield = 50f;
    
    [Tooltip("Shield regeneration rate per second")]
    public float shieldRegenRate = 5f;
    
    [Tooltip("Delay before shield starts regenerating after taking damage")]
    public float shieldRegenDelay = 3f;
    
    [Header("Lives & Respawn")]
    [Tooltip("Number of starting lives")]
    public int startingLives = 3;
    
    [Tooltip("Delay before respawning after death")]
    public float respawnDelay = 2f;
    
    [Tooltip("Is player invulnerable at start")]
    public bool startInvulnerable = false;
}
