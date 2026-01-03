using UnityEngine;
using System;

/// <summary>
/// Manages progression: experience, leveling
/// </summary>
public class ProgressionSystem : BaseStatCategory<ProgressionStatsConfig>
{
    private int currentLevel;
    private float currentXP;
    
    // Events
    public event Action<int> OnLevelUp;
    public event Action<float, float> OnXPChanged; // current, required
    
    // Properties
    public int CurrentLevel => currentLevel;
    public float CurrentXP => currentXP;
    public float XPForNextLevel => config.xpPerLevel * currentLevel;
    public float XPProgress => currentXP / XPForNextLevel;
    
    protected override void Awake()
    {
        base.Awake();
        currentLevel = config.startingLevel;
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
        if (healthSystem != null && config.healthPerLevel > 0)
        {
            var modifier = StatModifier.Flat(config.healthPerLevel, this);
            healthSystem.AddModifier("maxHealth", modifier);
            
            if (config.restoreOnLevelUp)
            {
                healthSystem.RestoreFullHealth();
            }
        }
        
        if (combatSystem != null && config.damagePerLevel > 0)
        {
            var modifier = StatModifier.Flat(config.damagePerLevel, this);
            combatSystem.AddModifier("damage", modifier);
        }
        
        if (movementSystem != null && config.speedPerLevel > 0)
        {
            var modifier = StatModifier.Flat(config.speedPerLevel, this);
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
