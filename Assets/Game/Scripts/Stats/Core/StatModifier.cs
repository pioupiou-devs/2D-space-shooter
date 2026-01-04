using UnityEngine;
using System;

/// <summary>
/// Base class for all stat modifiers (buffs, debuffs, equipment bonuses)
/// </summary>
[Serializable]
public class StatModifier
{
    public enum ModifierType
    {
        Flat,           // Adds/subtracts a flat value (e.g., +10 damage)
        PercentAdd,     // Adds percentage (e.g., +20% damage, stacks additively)
        PercentMult     // Multiplies percentage (e.g., x1.5 damage, stacks multiplicatively)
    }
    
    [SerializeField] private float value;
    [SerializeField] private ModifierType type;
    [SerializeField] private int order; // Lower order applies first
    private object source; // What caused this modifier (item, buff, etc.)
    
    public float Value => value;
    public ModifierType Type => type;
    public int Order => order;
    public object Source => source;
    
    public StatModifier(float value, ModifierType type, int order = 0, object source = null)
    {
        this.value = value;
        this.type = type;
        this.order = order;
        this.source = source;
    }
    
    /// <summary>
    /// Creates a flat modifier (e.g., +10 damage)
    /// </summary>
    public static StatModifier Flat(float value, object source = null)
    {
        return new StatModifier(value, ModifierType.Flat, 1, source);
    }
    
    /// <summary>
    /// Creates an additive percentage modifier (e.g., +20% = 0.2)
    /// </summary>
    public static StatModifier PercentAdd(float value, object source = null)
    {
        return new StatModifier(value, ModifierType.PercentAdd, 2, source);
    }
    
    /// <summary>
    /// Creates a multiplicative percentage modifier (e.g., x1.5 = 1.5)
    /// </summary>
    public static StatModifier PercentMult(float value, object source = null)
    {
        return new StatModifier(value, ModifierType.PercentMult, 3, source);
    }
}
