using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls full-screen effects like damage flash and low health vignette
/// Uses UI Image with materials for shader-based effects
/// </summary>
public class ScreenEffectsController : MonoBehaviour, IVisualFeedback
{
    [Header("Visual Type")]
    [SerializeField] private VisualType visualType = VisualType.Shader;
    
    [Header("Damage Flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.5f); // Red semi-transparent
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Low Health Vignette")]
    [SerializeField] private Image vignetteImage;
    [SerializeField] private Color vignetteColor = new Color(1f, 0f, 0f, 0.3f); // Red vignette
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float pulsSpeed = 2f;
    [SerializeField] private float minVignetteAlpha = 0.1f;
    [SerializeField] private float maxVignetteAlpha = 0.4f;
    
    [Header("Shield Break Flash")]
    [SerializeField] private bool flashOnShieldBreak = true;
    [SerializeField] private Color shieldBreakColor = new Color(0.5f, 0.5f, 1f, 0.3f); // Blue flash
    
    private PlayerStatsManager statsManager;
    private HealthSystem healthSystem;
    private float currentHealthPercent = 1f;
    private bool isFlashing = false;
    private bool showVignette = false;
    
    private void Awake()
    {
        // Initialize images as transparent
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
        
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }
    }
    
    public void Initialize(PlayerStatsManager stats)
    {
        statsManager = stats;
        healthSystem = stats.Health;
        
        // Subscribe to events
        healthSystem.OnHealthChanged += HandleHealthChanged;
        healthSystem.OnDamageTaken += HandleDamageTaken;
        healthSystem.OnShieldBroken += HandleShieldBroken;
        
        // Initialize
        UpdateVisual(healthSystem.HealthPercentage);
    }
    
    public void Show()
    {
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
        }
        
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
        }
    }
    
    public void Hide()
    {
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(false);
        }
        
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
        }
    }
    
    public void UpdateVisual(float normalizedValue)
    {
        currentHealthPercent = normalizedValue;
        
        // Show vignette if health is low
        showVignette = currentHealthPercent < lowHealthThreshold && currentHealthPercent > 0;
    }
    
    private void Update()
    {
        UpdateVignette();
    }
    
    private void UpdateVignette()
    {
        if (vignetteImage == null) return;
        
        if (showVignette)
        {
            // Pulse vignette
            float pulse = Mathf.Sin(Time.time * pulsSpeed) * 0.5f + 0.5f; // 0 to 1
            float alpha = Mathf.Lerp(minVignetteAlpha, maxVignetteAlpha, pulse);
            
            // Increase intensity as health gets lower
            float intensity = 1f - (currentHealthPercent / lowHealthThreshold);
            alpha *= intensity;
            
            Color c = vignetteColor;
            c.a = alpha;
            vignetteImage.color = c;
        }
        else
        {
            // Fade out vignette
            Color c = vignetteImage.color;
            c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 5f);
            vignetteImage.color = c;
        }
    }
    
    private void HandleHealthChanged(float current, float max)
    {
        float percent = max > 0 ? current / max : 0f;
        UpdateVisual(percent);
    }
    
    private void HandleDamageTaken(float amount)
    {
        FlashScreen(flashColor);
    }
    
    private void HandleShieldBroken()
    {
        if (flashOnShieldBreak)
        {
            FlashScreen(shieldBreakColor);
        }
    }
    
    public void FlashScreen(Color color)
    {
        if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine(color));
        }
    }
    
    private System.Collections.IEnumerator FlashCoroutine(Color color)
    {
        if (flashImage == null) yield break;
        
        isFlashing = true;
        float elapsed = 0f;
        
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            float alpha = flashCurve.Evaluate(t);
            
            Color c = color;
            c.a *= alpha;
            flashImage.color = c;
            
            yield return null;
        }
        
        // Ensure fully transparent at end
        Color finalColor = flashImage.color;
        finalColor.a = 0f;
        flashImage.color = finalColor;
        
        isFlashing = false;
    }
    
    public void Cleanup()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= HandleHealthChanged;
            healthSystem.OnDamageTaken -= HandleDamageTaken;
            healthSystem.OnShieldBroken -= HandleShieldBroken;
        }
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
    
    #region Debug
    
    [ContextMenu("Test Damage Flash")]
    private void DebugDamageFlash()
    {
        FlashScreen(flashColor);
    }
    
    [ContextMenu("Test Shield Break Flash")]
    private void DebugShieldBreakFlash()
    {
        FlashScreen(shieldBreakColor);
    }
    
    [ContextMenu("Toggle Low Health Vignette")]
    private void DebugToggleVignette()
    {
        showVignette = !showVignette;
        currentHealthPercent = showVignette ? 0.2f : 1f;
    }
    
    #endregion
}
