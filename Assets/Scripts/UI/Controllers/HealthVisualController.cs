using UnityEngine;

/// <summary>
/// Controls health visual feedback using particle effects
/// Shows smoke, fire, and damage effects based on health percentage
/// </summary>
public class HealthVisualController : MonoBehaviour, IVisualFeedback
{
    [Header("Visual Type")]
    [SerializeField] private VisualType visualType = VisualType.Prefab;
    
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private ParticleSystem sparksParticles;
    
    [Header("Health Thresholds")]
    [SerializeField] private float smokeThreshold = 0.75f;  // Below 75% health
    [SerializeField] private float sparksThreshold = 0.5f;   // Below 50% health
    [SerializeField] private float fireThreshold = 0.25f;    // Below 25% health
    
    [Header("Sprite Damage States (Optional)")]
    [SerializeField] private SpriteRenderer shipSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Sprite criticalSprite;
    
    [Header("Behavior")]
    [SerializeField] private bool useParticleIntensity = true;
    [SerializeField] private float maxSmokeEmission = 20f;
    [SerializeField] private float maxFireEmission = 15f;
    
    private PlayerStatsManager statsManager;
    private HealthSystem healthSystem;
    private float currentHealthPercent = 1f;
    
    public void Initialize(PlayerStatsManager stats)
    {
        statsManager = stats;
        healthSystem = stats.Health;
        
        // Subscribe to health events
        healthSystem.OnHealthChanged += HandleHealthChanged;
        healthSystem.OnDamageTaken += HandleDamageTaken;
        healthSystem.OnHealed += HandleHealed;
        healthSystem.OnDeath += HandleDeath;
        healthSystem.OnRespawn += HandleRespawn;
        
        // Initialize visual
        UpdateVisual(healthSystem.HealthPercentage);
    }
    
    public void Show()
    {
        // Show is handled by UpdateVisual based on health
    }
    
    public void Hide()
    {
        StopAllParticles();
    }
    
    public void UpdateVisual(float normalizedValue)
    {
        currentHealthPercent = normalizedValue;
        
        UpdateParticleEffects(normalizedValue);
        UpdateSpriteState(normalizedValue);
    }
    
    private void UpdateParticleEffects(float healthPercent)
    {
        // Smoke appears below smokeThreshold
        if (smokeParticles != null)
        {
            if (healthPercent < smokeThreshold && healthPercent > 0)
            {
                if (!smokeParticles.isPlaying)
                {
                    smokeParticles.Play();
                }
                
                if (useParticleIntensity)
                {
                    // Increase smoke as health decreases
                    float smokeIntensity = 1f - (healthPercent / smokeThreshold);
                    var emission = smokeParticles.emission;
                    emission.rateOverTime = smokeIntensity * maxSmokeEmission;
                }
            }
            else
            {
                if (smokeParticles.isPlaying)
                {
                    smokeParticles.Stop();
                }
            }
        }
        
        // Sparks appear below sparksThreshold
        if (sparksParticles != null)
        {
            if (healthPercent < sparksThreshold && healthPercent > 0)
            {
                if (!sparksParticles.isPlaying)
                {
                    sparksParticles.Play();
                }
            }
            else
            {
                if (sparksParticles.isPlaying)
                {
                    sparksParticles.Stop();
                }
            }
        }
        
        // Fire appears below fireThreshold (critical!)
        if (fireParticles != null)
        {
            if (healthPercent < fireThreshold && healthPercent > 0)
            {
                if (!fireParticles.isPlaying)
                {
                    fireParticles.Play();
                }
                
                if (useParticleIntensity)
                {
                    float fireIntensity = 1f - (healthPercent / fireThreshold);
                    var emission = fireParticles.emission;
                    emission.rateOverTime = fireIntensity * maxFireEmission;
                }
            }
            else
            {
                if (fireParticles.isPlaying)
                {
                    fireParticles.Stop();
                }
            }
        }
    }
    
    private void UpdateSpriteState(float healthPercent)
    {
        if (shipSprite == null) return;
        
        if (healthPercent > sparksThreshold && normalSprite != null)
        {
            shipSprite.sprite = normalSprite;
        }
        else if (healthPercent > fireThreshold && damagedSprite != null)
        {
            shipSprite.sprite = damagedSprite;
        }
        else if (healthPercent > 0 && criticalSprite != null)
        {
            shipSprite.sprite = criticalSprite;
        }
    }
    
    private void HandleHealthChanged(float current, float max)
    {
        float percent = max > 0 ? current / max : 0f;
        UpdateVisual(percent);
    }
    
    private void HandleDamageTaken(float amount)
    {
        // Could trigger damage flash or shake here
        StartCoroutine(DamageFlashEffect());
    }
    
    private void HandleHealed(float amount)
    {
        // Could show heal particles here
    }
    
    private void HandleDeath()
    {
        StopAllParticles();
        
        // Could play explosion effect here
        if (fireParticles != null)
        {
            fireParticles.Play();
        }
    }
    
    private void HandleRespawn()
    {
        StopAllParticles();
        UpdateVisual(1f);
    }
    
    private void StopAllParticles()
    {
        if (smokeParticles != null && smokeParticles.isPlaying)
        {
            smokeParticles.Stop();
        }
        
        if (fireParticles != null && fireParticles.isPlaying)
        {
            fireParticles.Stop();
        }
        
        if (sparksParticles != null && sparksParticles.isPlaying)
        {
            sparksParticles.Stop();
        }
    }
    
    private System.Collections.IEnumerator DamageFlashEffect()
    {
        if (shipSprite == null) yield break;
        
        Color originalColor = shipSprite.color;
        shipSprite.color = Color.red;
        
        yield return new WaitForSeconds(0.1f);
        
        shipSprite.color = originalColor;
    }
    
    public void Cleanup()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= HandleHealthChanged;
            healthSystem.OnDamageTaken -= HandleDamageTaken;
            healthSystem.OnHealed -= HandleHealed;
            healthSystem.OnDeath -= HandleDeath;
            healthSystem.OnRespawn -= HandleRespawn;
        }
        
        StopAllParticles();
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
    
    #region Debug
    
    [ContextMenu("Test 75% Health")]
    private void DebugHealth75()
    {
        UpdateVisual(0.75f);
    }
    
    [ContextMenu("Test 50% Health")]
    private void DebugHealth50()
    {
        UpdateVisual(0.5f);
    }
    
    [ContextMenu("Test 25% Health")]
    private void DebugHealth25()
    {
        UpdateVisual(0.25f);
    }
    
    [ContextMenu("Test Damage Flash")]
    private void DebugDamageFlash()
    {
        HandleDamageTaken(10f);
    }
    
    #endregion
}
