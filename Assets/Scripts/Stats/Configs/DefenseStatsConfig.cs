using UnityEngine;

/// <summary>
/// ScriptableObject configuration for defense-related stats
/// </summary>
[CreateAssetMenu(fileName = "DefenseStats", menuName = "Stats/Defense Config")]
public class DefenseStatsConfig : ScriptableObject
{
    [Header("Armor")]
    [Tooltip("Flat damage reduction (subtracted from incoming damage)")]
    public float armor = 0f;
    
    [Header("Damage Reduction")]
    [Tooltip("Percentage damage reduction (0-1)")]
    [Range(0f, 1f)]
    public float damageReduction = 0f;
    
    [Tooltip("Maximum allowed damage reduction")]
    [Range(0f, 1f)]
    public float maxDamageReduction = 0.75f;
}
