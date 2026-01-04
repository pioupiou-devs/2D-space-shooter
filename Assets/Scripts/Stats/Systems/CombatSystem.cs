using UnityEngine;

/// <summary>
/// Manages combat stats: damage, fire rate, critical hits
/// </summary>
public class CombatSystem : BaseStatCategory<CombatStatsConfig>
{
    // Default values when config is not assigned
    private const float DefaultDamage = 10f;
    private const float DefaultFireRate = 1f;
    private const float DefaultCriticalChance = 0.05f;
    private const float DefaultCriticalMultiplier = 2f;

    // Properties with modifier support and null checks
    public float Damage => config != null ? CalculateStat(config.damage, "damage") : DefaultDamage;
    public float FireRate => config != null ? CalculateStat(config.fireRate, "fireRate") : DefaultFireRate;
    public float CriticalChance => config != null ? Mathf.Clamp01(CalculateStat(config.criticalChance, "criticalChance")) : DefaultCriticalChance;
    public float CriticalMultiplier => config != null ? CalculateStat(config.criticalMultiplier, "criticalMultiplier") : DefaultCriticalMultiplier;
    
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
