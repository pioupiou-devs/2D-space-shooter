using UnityEngine;

/// <summary>
/// ScriptableObject configuration for combat-related stats
/// </summary>
[CreateAssetMenu(fileName = "CombatStats", menuName = "Stats/Combat Config")]
public class CombatStatsConfig : ScriptableObject
{
    [Header("Damage")]
    [Tooltip("Base damage dealt per attack")]
    public float damage = 10f;
    
    [Header("Fire Rate")]
    [Tooltip("Number of attacks per second")]
    public float fireRate = 1f;
    
    [Header("Critical Hits")]
    [Tooltip("Chance to deal critical damage (0-1)")]
    [Range(0f, 1f)]
    public float criticalChance = 0.1f;
    
    [Tooltip("Damage multiplier for critical hits")]
    public float criticalMultiplier = 2f;
}
