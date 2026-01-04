using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Base class for all stat category systems with built-in modifier support
/// </summary>
public abstract class BaseStatCategory<TConfig> : MonoBehaviour where TConfig : ScriptableObject
{
    [SerializeField] protected TConfig config;
    
    // Dictionary of stat modifiers: statName -> list of modifiers
    protected Dictionary<string, List<StatModifier>> modifiers = new Dictionary<string, List<StatModifier>>();
    
    // Events for stat changes
    public event Action<string, float> OnStatChanged;
    
    public TConfig Config => config;
    
    protected virtual void Awake()
    {
        if (config == null)
        {
            Debug.LogWarning($"{GetType().Name}: Config is not assigned!");
        }
    }
    
    /// <summary>
    /// Calculates the final stat value with all modifiers applied
    /// Order: Base -> Flat -> PercentAdd -> PercentMult
    /// </summary>
    protected float CalculateStat(float baseStat, string statName)
    {
        if (!modifiers.ContainsKey(statName) || modifiers[statName].Count == 0)
        {
            return baseStat;
        }
        
        float finalValue = baseStat;
        float percentAddSum = 0f;
        
        // Sort modifiers by order
        var sortedModifiers = modifiers[statName].OrderBy(m => m.Order).ToList();
        
        foreach (var modifier in sortedModifiers)
        {
            switch (modifier.Type)
            {
                case StatModifier.ModifierType.Flat:
                    finalValue += modifier.Value;
                    break;
                    
                case StatModifier.ModifierType.PercentAdd:
                    percentAddSum += modifier.Value;
                    break;
                    
                case StatModifier.ModifierType.PercentMult:
                    finalValue *= modifier.Value;
                    break;
            }
        }
        
        // Apply all additive percentages at once
        if (percentAddSum != 0f)
        {
            finalValue *= (1f + percentAddSum);
        }
        
        return finalValue;
    }
    
    /// <summary>
    /// Adds a modifier to a specific stat
    /// </summary>
    public void AddModifier(string statName, StatModifier modifier)
    {
        if (!modifiers.ContainsKey(statName))
        {
            modifiers[statName] = new List<StatModifier>();
        }
        
        modifiers[statName].Add(modifier);
        OnStatModified(statName);
    }
    
    /// <summary>
    /// Removes all modifiers from a specific source
    /// </summary>
    public void RemoveModifier(string statName, object source)
    {
        if (!modifiers.ContainsKey(statName))
        {
            return;
        }
        
        modifiers[statName].RemoveAll(m => m.Source == source);
        OnStatModified(statName);
    }
    
    /// <summary>
    /// Removes all modifiers for a specific stat
    /// </summary>
    public void ClearModifiers(string statName)
    {
        if (modifiers.ContainsKey(statName))
        {
            modifiers[statName].Clear();
            OnStatModified(statName);
        }
    }
    
    /// <summary>
    /// Removes all modifiers from all stats
    /// </summary>
    public void ClearAllModifiers()
    {
        foreach (var statName in modifiers.Keys.ToList())
        {
            ClearModifiers(statName);
        }
    }
    
    /// <summary>
    /// Called when a stat is modified - can be overridden for custom behavior
    /// </summary>
    protected virtual void OnStatModified(string statName)
    {
        OnStatChanged?.Invoke(statName, GetStatValue(statName));
    }
    
    /// <summary>
    /// Override this to return the current value of a specific stat
    /// </summary>
    protected abstract float GetStatValue(string statName);
}
