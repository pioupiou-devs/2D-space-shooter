using UnityEngine;

/// <summary>
/// Manages defense stats: armor, damage reduction
/// </summary>
public class DefenseSystem : BaseStatCategory<DefenseStatsConfig>
{
    // Default values when config is not assigned
    private const float DefaultArmor = 0f;
    private const float DefaultDamageReduction = 0f;
    private const float DefaultMaxDamageReduction = 0.75f;

    // Properties with modifier support and null checks
    public float Armor => config != null ? CalculateStat(config.armor, "armor") : DefaultArmor;
    public float DamageReduction => config != null 
        ? Mathf.Clamp(CalculateStat(config.damageReduction, "damageReduction"), 0f, config.maxDamageReduction) 
        : DefaultDamageReduction;
    
    /// <summary>
    /// Apply defense calculations to incoming damage
    /// </summary>
    public float ApplyDefense(float incomingDamage)
    {
        // Apply percentage reduction first
        float reducedDamage = incomingDamage * (1f - DamageReduction);
        
        // Then subtract flat armor
        reducedDamage = Mathf.Max(0, reducedDamage - Armor);
        
        return reducedDamage;
    }
    
    protected override float GetStatValue(string statName)
    {
        return statName switch
        {
            "armor" => Armor,
            "damageReduction" => DamageReduction,
            _ => 0f
        };
    }
}
