using UnityEngine;

/// <summary>
/// Manages defense stats: armor, damage reduction
/// </summary>
public class DefenseSystem : BaseStatCategory<DefenseStatsConfig>
{
    // Properties with modifier support
    public float Armor => CalculateStat(config.armor, "armor");
    public float DamageReduction => Mathf.Clamp(
        CalculateStat(config.damageReduction, "damageReduction"),
        0f,
        config.maxDamageReduction
    );
    
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
