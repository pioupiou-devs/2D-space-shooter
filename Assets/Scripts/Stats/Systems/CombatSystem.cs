using UnityEngine;

/// <summary>
/// Manages combat stats: damage, fire rate, critical hits
/// </summary>
public class CombatSystem : BaseStatCategory<CombatStatsConfig>
{
    // Properties with modifier support
    public float Damage => CalculateStat(config.damage, "damage");
    public float FireRate => CalculateStat(config.fireRate, "fireRate");
    public float CriticalChance => Mathf.Clamp01(CalculateStat(config.criticalChance, "criticalChance"));
    public float CriticalMultiplier => CalculateStat(config.criticalMultiplier, "criticalMultiplier");
    
    /// <summary>
    /// Calculate final damage output with potential critical hit
    /// </summary>
    public float CalculateDamageOutput()
    {
        float finalDamage = Damage;
        
        // Check for critical hit
        if (Random.value <= CriticalChance)
        {
            finalDamage *= CriticalMultiplier;
            Debug.Log("Critical Hit!");
        }
        
        return finalDamage;
    }
    
    /// <summary>
    /// Get time between shots based on fire rate
    /// </summary>
    public float GetTimeBetweenShots()
    {
        return 1f / FireRate;
    }
    
    protected override float GetStatValue(string statName)
    {
        return statName switch
        {
            "damage" => Damage,
            "fireRate" => FireRate,
            "criticalChance" => CriticalChance,
            "criticalMultiplier" => CriticalMultiplier,
            _ => 0f
        };
    }
}
