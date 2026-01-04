using UnityEngine;
using System;

/// <summary>
/// Manages health, shield, lives, and damage reception
/// </summary>
public class HealthSystem : BaseStatCategory<HealthStatsConfig>
{
    // Runtime state
    private float currentHealth;
    private float currentShield;
    private int currentLives;
    private bool isInvulnerable;
    private float timeSinceLastDamage;
    
    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float> OnDamageTaken; // damage amount
    public event Action<float> OnHealed; // heal amount
    public event Action<float, float> OnShieldChanged; // current, max
    public event Action<float> OnShieldHit; // damage absorbed by shield
    public event Action OnShieldBroken; // shield depleted
    public event Action OnShieldRestored; // shield fully recharged
    public event Action<int> OnLivesChanged;
    public event Action OnDeath;
    public event Action OnRespawn;
    
    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => CalculateStat(config.maxHealth, "maxHealth");
    public float HealthPercentage => currentHealth / MaxHealth;
    public float CurrentShield => currentShield;
    public float MaxShield => CalculateStat(config.maxShield, "maxShield");
    public float ShieldPercentage => currentShield / MaxShield;
    public int CurrentLives => currentLives;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvulnerable => isInvulnerable;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Initialize
        currentHealth = MaxHealth;
        currentShield = MaxShield;
        currentLives = config.startingLives;
        isInvulnerable = config.startInvulnerable;
    }
    
    private void Start()
    {
        // Notify initial values
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        OnShieldChanged?.Invoke(currentShield, MaxShield);
        OnLivesChanged?.Invoke(currentLives);
    }
    
    private void Update()
    {
        // Handle shield regeneration
        if (currentShield < MaxShield)
        {
            timeSinceLastDamage += Time.deltaTime;
            
            if (timeSinceLastDamage >= config.shieldRegenDelay)
            {
                float regenAmount = config.shieldRegenRate * Time.deltaTime;
                RegenerateShield(regenAmount);
            }
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable || !IsAlive)
        {
            return;
        }
        
        timeSinceLastDamage = 0f;
        float remainingDamage = damageAmount;
        bool shieldWasBroken = false;
        
        // Shield absorbs damage first
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldDamage;
            remainingDamage -= shieldDamage;
            
            OnShieldHit?.Invoke(shieldDamage);
            OnShieldChanged?.Invoke(currentShield, MaxShield);
            
            if (currentShield <= 0)
            {
                shieldWasBroken = true;
                OnShieldBroken?.Invoke();
            }
        }
        
        // Apply remaining damage to health
        if (remainingDamage > 0)
        {
            currentHealth -= remainingDamage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            OnDamageTaken?.Invoke(remainingDamage);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            
            Debug.Log($"Took {remainingDamage} damage. Health: {currentHealth}/{MaxHealth}");
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        if (!IsAlive)
        {
            return;
        }
        
        currentHealth = Mathf.Min(currentHealth + healAmount, MaxHealth);
        
        OnHealed?.Invoke(healAmount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        
        Debug.Log($"Healed {healAmount}. Health: {currentHealth}/{MaxHealth}");
    }
    
    public void RegenerateShield(float amount)
    {
        float oldShield = currentShield;
        currentShield = Mathf.Min(currentShield + amount, MaxShield);
        
        OnShieldChanged?.Invoke(currentShield, MaxShield);
        
        // Fire restored event when shield becomes full
        if (oldShield < MaxShield && currentShield >= MaxShield)
        {
            OnShieldRestored?.Invoke();
        }
    }
    
    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;
        currentShield = MaxShield;
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        OnShieldChanged?.Invoke(currentShield, MaxShield);
    }
    
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
        Debug.Log($"Invulnerability: {isInvulnerable}");
    }
    
    private void Die()
    {
        Debug.Log("Died!");
        OnDeath?.Invoke();
        
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);
        
        if (currentLives > 0)
        {
            Invoke(nameof(Respawn), config.respawnDelay);
        }
        else
        {
            Debug.Log("Game Over!");
        }
    }
    
    private void Respawn()
    {
        currentHealth = MaxHealth;
        currentShield = MaxShield;
        timeSinceLastDamage = 0f;
        
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        OnShieldChanged?.Invoke(currentShield, MaxShield);
        OnRespawn?.Invoke();
        
        Debug.Log("Respawned!");
    }
    
    protected override float GetStatValue(string statName)
    {
        return statName switch
        {
            "maxHealth" => MaxHealth,
            "maxShield" => MaxShield,
            _ => 0f
        };
    }
}
