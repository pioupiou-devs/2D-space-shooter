using UnityEngine;
using System;

/// <summary>
/// Lightweight coordinator that manages all player stat systems
/// Implements IDamageable for compatibility with existing damage system
/// </summary>
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(DefenseSystem))]
[RequireComponent(typeof(MovementSystem))]
[RequireComponent(typeof(ProgressionSystem))]
public class PlayerStatsManager : MonoBehaviour, IDamageable
{
    // Category Systems
    private HealthSystem healthSystem;
    private CombatSystem combatSystem;
    private DefenseSystem defenseSystem;
    private MovementSystem movementSystem;
    private ProgressionSystem progressionSystem;
    
    // Public accessors for each system
    public HealthSystem Health => healthSystem;
    public CombatSystem Combat => combatSystem;
    public DefenseSystem Defense => defenseSystem;
    public MovementSystem Movement => movementSystem;
    public ProgressionSystem Progression => progressionSystem;
    
    // Legacy properties for backward compatibility with existing code
    public float CurrentHealth => healthSystem.CurrentHealth;
    public float MaxHealth => healthSystem.MaxHealth;
    public float HealthPercentage => healthSystem.HealthPercentage;
    public float MoveSpeed => movementSystem.MoveSpeed;
    public float FireRate => combatSystem.FireRate;
    public float Damage => combatSystem.Damage;
    public float CurrentShield => healthSystem.CurrentShield;
    public float MaxShield => healthSystem.MaxShield;
    public bool IsAlive => healthSystem.IsAlive;
    public int Lives => healthSystem.CurrentLives;
    public int Level => progressionSystem.CurrentLevel;
    
    private void Awake()
    {
        // Get all category systems
        healthSystem = GetComponent<HealthSystem>();
        combatSystem = GetComponent<CombatSystem>();
        defenseSystem = GetComponent<DefenseSystem>();
        movementSystem = GetComponent<MovementSystem>();
        progressionSystem = GetComponent<ProgressionSystem>();
    }
    
    #region IDamageable Implementation
    
    /// <summary>
    /// Takes damage with defense calculations applied
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (!healthSystem.IsAlive)
        {
            return;
        }
        
        // Apply defense calculations
        float reducedDamage = defenseSystem.ApplyDefense(damageAmount);
        
        // Apply to health
        healthSystem.TakeDamage(reducedDamage);
    }
    
    #endregion
    
    #region Helper Methods for Legacy Code
    
    public float CalculateDamageOutput()
    {
        return combatSystem.CalculateDamageOutput();
    }
    
    public float GetTimeBetweenShots()
    {
        return combatSystem.GetTimeBetweenShots();
    }
    
    public void Heal(float healAmount)
    {
        healthSystem.Heal(healAmount);
    }
    
    public void AddExperience(float xp)
    {
        progressionSystem.AddXP(xp);
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Take 20 Damage")]
    private void DebugTakeDamage()
    {
        TakeDamage(20f);
    }
    
    [ContextMenu("Heal 30")]
    private void DebugHeal()
    {
        Heal(30f);
    }
    
    [ContextMenu("Add 100 XP")]
    private void DebugAddXP()
    {
        AddExperience(100f);
    }
    
    [ContextMenu("Add Damage Buff (+50%)")]
    private void DebugAddDamageBuff()
    {
        var modifier = StatModifier.PercentAdd(0.5f, this);
        combatSystem.AddModifier("damage", modifier);
        Debug.Log($"Added +50% damage buff. New damage: {combatSystem.Damage}");
    }
    
    [ContextMenu("Add Speed Buff (+2)")]
    private void DebugAddSpeedBuff()
    {
        var modifier = StatModifier.Flat(2f, this);
        movementSystem.AddModifier("moveSpeed", modifier);
        Debug.Log($"Added +2 speed buff. New speed: {movementSystem.MoveSpeed}");
    }
    
    [ContextMenu("Clear All Modifiers")]
    private void DebugClearAllModifiers()
    {
        healthSystem.ClearAllModifiers();
        combatSystem.ClearAllModifiers();
        defenseSystem.ClearAllModifiers();
        movementSystem.ClearAllModifiers();
        progressionSystem.ClearAllModifiers();
        Debug.Log("Cleared all modifiers from all systems");
    }
    
    #endregion
}
