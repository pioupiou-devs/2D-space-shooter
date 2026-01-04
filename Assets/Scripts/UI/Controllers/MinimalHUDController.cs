using TMPro;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal HUD controller showing only essential info: Lives, Score, Level
/// Uses standard Unity UI Text
/// NOTE: For better text quality, use TextMeshPro and replace Text with TextMeshProUGUI
/// </summary>
public class MinimalHUDController : MonoBehaviour, IVisualFeedback
{
    [Header("Visual Type")]
    [SerializeField] private VisualType visualType = VisualType.UI;
    
    [Header("Text References (Unity UI Text)")]
    [Tooltip("Use TextMeshPro for better quality if package is installed")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText; // Optional XP display
    
    [Header("Formatting")]
    [SerializeField] private string livesFormat = "♥ {0}";
    [SerializeField] private string scoreFormat = "{0:N0}";
    [SerializeField] private string levelFormat = "Lv {0}";
    [SerializeField] private string xpFormat = "[{0:F0}%]";
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowLivesColor = Color.red;
    [SerializeField] private int lowLivesThreshold = 1;
    
    [Header("Animations")]
    [SerializeField] private bool animateOnChange = true;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.2f;
    
    private PlayerStatsManager statsManager;
    private HealthSystem healthSystem;
    private ProgressionSystem progressionSystem;
    
    private int currentScore = 0;
    private int currentLives = 0;
    private int currentLevel = 1;
    private float currentXPPercent = 0f;
    
    public void Initialize(PlayerStatsManager stats)
    {
        statsManager = stats;
        healthSystem = stats.Health;
        progressionSystem = stats.Progression;
        
        // Subscribe to events
        healthSystem.OnLivesChanged += HandleLivesChanged;
        progressionSystem.OnLevelUp += HandleLevelUp;
        progressionSystem.OnXPChanged += HandleXPChanged;
        
        // Initialize display
        UpdateLivesDisplay(healthSystem.CurrentLives);
        UpdateLevelDisplay(progressionSystem.CurrentLevel);
        UpdateXPDisplay(progressionSystem.CurrentXP, progressionSystem.XPForNextLevel);
        UpdateScoreDisplay(currentScore);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void UpdateVisual(float normalizedValue)
    {
        // Not used for HUD, but required by interface
    }
    
    #region Lives Display
    
    private void HandleLivesChanged(int lives)
    {
        UpdateLivesDisplay(lives);
    }
    
    private void UpdateLivesDisplay(int lives)
    {
        currentLives = lives;
        
        if (livesText != null)
        {
            livesText.text = string.Format(livesFormat, lives);
            
            // Change color if low
            if (lives <= lowLivesThreshold)
            {
                livesText.color = lowLivesColor;
            }
            else
            {
                livesText.color = normalColor;
            }
            
            if (animateOnChange)
            {
                AnimateText(livesText);
            }
        }
    }
    
    #endregion
    
    #region Score Display
    
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay(currentScore);
    }
    
    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreDisplay(currentScore);
    }
    
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
            
            if (animateOnChange)
            {
                AnimateText(scoreText);
            }
        }
    }
    
    #endregion
    
    #region Level Display
    
    private void HandleLevelUp(int newLevel)
    {
        UpdateLevelDisplay(newLevel);
    }
    
    private void UpdateLevelDisplay(int level)
    {
        currentLevel = level;
        
        if (levelText != null)
        {
            levelText.text = string.Format(levelFormat, level);
            
            if (animateOnChange)
            {
                AnimateText(levelText);
            }
        }
    }
    
    #endregion
    
    #region XP Display
    
    private void HandleXPChanged(float current, float needed)
    {
        UpdateXPDisplay(current, needed);
    }
    
    private void UpdateXPDisplay(float current, float needed)
    {
        if (xpText != null && needed > 0)
        {
            float percent = (current / needed) * 100f;
            currentXPPercent = percent;
            xpText.text = string.Format(xpFormat, percent);
        }
    }
    
    #endregion
    
    #region Animation
    
    private void AnimateText(TextMeshProUGUI text)
    {
        if (text == null) return;
        
        StartCoroutine(PopAnimation(text));
    }
    
    private System.Collections.IEnumerator PopAnimation(TextMeshProUGUI text)
    {
        Vector3 originalScale = text.transform.localScale;
        Vector3 popScaleVec = originalScale * popScale;
        
        float elapsed = 0f;
        float halfDuration = popDuration / 2f;
        
        // Scale up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            text.transform.localScale = Vector3.Lerp(originalScale, popScaleVec, t);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale back down
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            text.transform.localScale = Vector3.Lerp(popScaleVec, originalScale, t);
            yield return null;
        }
        
        text.transform.localScale = originalScale;
    }
    
    #endregion
    
    public void Cleanup()
    {
        if (healthSystem != null)
        {
            healthSystem.OnLivesChanged -= HandleLivesChanged;
        }
        
        if (progressionSystem != null)
        {
            progressionSystem.OnLevelUp -= HandleLevelUp;
            progressionSystem.OnXPChanged -= HandleXPChanged;
        }
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
    
    #region Debug
    
    [ContextMenu("Add 100 Score")]
    private void DebugAddScore()
    {
        AddScore(100);
    }
    
    [ContextMenu("Lose a Life")]
    private void DebugLoseLife()
    {
        if (currentLives > 0)
        {
            UpdateLivesDisplay(currentLives - 1);
        }
    }
    
    [ContextMenu("Level Up")]
    private void DebugLevelUp()
    {
        UpdateLevelDisplay(currentLevel + 1);
    }
    
    #endregion
}
