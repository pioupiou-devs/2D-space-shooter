using UnityEngine;
using System;

/// <summary>
/// Manages progression: experience, leveling
/// </summary>
public class ProgressionSystem : BaseStatCategory<ProgressionStatsConfig>
{
    // Default values when config is not assigned
    private const int DefaultStartingLevel = 1;
    private const float DefaultXPPerLevel = 100f;
    private const float DefaultHealthPerLevel = 10f;
    private const float DefaultDamagePerLevel = 2f;
    private const float DefaultSpeedPerLevel = 0.1f;
    private const bool DefaultRestoreOnLevelUp = true;

    private int currentLevel;
    private float currentXP;
    
    // Events
    public event Action<int> OnLevelUp;
    public event Action<float, float> OnXPChanged; // current, required
    
    // Properties with null checks
    public int CurrentLevel => currentLevel;
    public float CurrentXP => currentXP;
    public float XPForNextLevel => (config != null ? config.xpPerLevel : DefaultXPPerLevel) * currentLevel;
    public float XPProgress => XPForNextLevel > 0 ? currentXP / XPForNextLevel : 0f;

    // Config accessors with defaults
    private float HealthPerLevel => config != null ? config.healthPerLevel : DefaultHealthPerLevel;
    private float DamagePerLevel => config != null ? config.damagePerLevel : DefaultDamagePerLevel;
    private float SpeedPerLevel => config != null ? config.speedPerLevel : DefaultSpeedPerLevel;
    private bool RestoreOnLevelUp => config != null ? config.restoreOnLevelUp : DefaultRestoreOnLevelUp;
    
    protected override void Awake()
    {
        base.Awake();
        currentLevel = config != null ? config.startingLevel : DefaultStartingLevel;
        currentXP = 0f;
    }
    
    private void Start()
    {
        OnXPChanged?.Invoke(currentXP, XPForNextLevel);
    }
    
    public void AddXP(float xp)
    {
        currentXP += xp;
        OnXPChanged?.Invoke(currentXP, XPForNextLevel);
        
        // Check for level up
        while (currentXP >= XPForNextLevel)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0f;
        
        Debug.Log($"Level Up! Now level {currentLevel}");
        
        // Apply level up bonuses to other systems
        ApplyLevelUpBonuses();
        
        OnLevelUp?.Invoke(currentLevel);
        OnXPChanged?.Invoke(currentXP, XPForNextLevel);
    }
    
    private void ApplyLevelUpBonuses()
    {
        // Get other systems
        var healthSystem = GetComponent<HealthSystem>();
        var combatSystem = GetComponent<CombatSystem>();
        var movementSystem = GetComponent<MovementSystem>();
        
        // Add permanent stat bonuses
        if (healthSystem != null && HealthPerLevel > 0)
        {
            var modifier = StatModifier.Flat(HealthPerLevel, this);
            healthSystem.AddModifier("maxHealth", modifier);
            
            if (RestoreOnLevelUp)
            {
                healthSystem.RestoreFullHealth();
            }
        }
        
        if (combatSystem != null && DamagePerLevel > 0)
        {
            var modifier = StatModifier.Flat(DamagePerLevel, this);
            combatSystem.AddModifier("damage", modifier);
        }
        
        if (movementSystem != null && SpeedPerLevel > 0)
        {
            var modifier = StatModifier.Flat(SpeedPerLevel, this);
            movementSystem.AddModifier("moveSpeed", modifier);
        }
    }
    
    protected override float GetStatValue(string statName)
    {
        return statName switch
        {
            "level" => currentLevel,
            "xp" => currentXP,
            _ => 0f
        };
    }
}
