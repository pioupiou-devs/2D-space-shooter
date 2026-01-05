using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the pause menu during gameplay.
/// Extends BaseMenuController for shared functionality.
/// Handles Time.timeScale, panel navigation, and game state.
/// </summary>
public class PauseMenuManager : BaseMenuController
{
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject overlayPanel;
    
    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Scene References")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    
    [Header("Settings")]
    [SerializeField] private bool dimHUDWhenPaused = true;
    [SerializeField] private float hudDimAlpha = 0.3f;
    
    private bool isPaused;
    private CanvasGroup hudCanvasGroup;
    
    public bool IsPaused => isPaused;
    public static PauseMenuManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Find HUD canvas group for dimming
        var hudPanel = transform.parent?.Find("HUD Panel");
        if (hudPanel != null)
        {
            hudCanvasGroup = hudPanel.GetComponent<CanvasGroup>();
            if (hudCanvasGroup == null)
                hudCanvasGroup = hudPanel.gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        WirePauseMenuButtons();
        SubscribeToPauseInput();
        
        // Start with pause menu hidden
        HidePauseMenu();
    }
    
    protected override void OnDestroy()
    {
        UnwirePauseMenuButtons();
        UnsubscribeFromPauseInput();
        
        // Ensure time scale is restored
        Time.timeScale = 1f;
        
        if (Instance == this)
            Instance = null;
        
        base.OnDestroy();
    }
    
    private void WirePauseMenuButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }
    
    private void UnwirePauseMenuButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }
    
    private void SubscribeToPauseInput()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnPausePressed += HandlePausePressed;
    }
    
    private void UnsubscribeFromPauseInput()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnPausePressed -= HandlePausePressed;
    }
    
    #region Abstract Implementations
    
    protected override void HandleCancelPressed()
    {
        if (IsConfirmDialogVisible())
        {
            HideConfirmDialog();
            ShowMainPanel();
        }
        else if (optionsPanel != null && optionsPanel.activeSelf)
        {
            OnOptionsBackClicked();
        }
        else if (isPaused)
        {
            Resume();
        }
    }
    
    protected override void ShowMainPanel()
    {
        HidePanel(optionsPanel);
        HideConfirmDialog();
        ShowPanel(pauseMenuPanel);
        currentPanel = pauseMenuPanel;
    }
    
    #endregion
    
    #region Pause Control
    
    private void HandlePausePressed()
    {
        TogglePause();
    }
    
    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }
    
    public void Pause()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        ShowPauseMenu();
        
        // Switch to UI input
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.EnableUI();
        
        // Dim HUD
        if (dimHUDWhenPaused && hudCanvasGroup != null)
            hudCanvasGroup.alpha = hudDimAlpha;
        
        Debug.Log("Game Paused");
    }
    
    public void Resume()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        HidePauseMenu();
        
        // Switch back to gameplay input
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.EnableGameplay();
        
        // Restore HUD
        if (hudCanvasGroup != null)
            hudCanvasGroup.alpha = 1f;
        
        Debug.Log("Game Resumed");
    }
    
    #endregion
    
    #region Panel Management
    
    private void ShowPauseMenu()
    {
        ShowPanel(overlayPanel);
        ShowPanel(pauseMenuPanel);
        HidePanel(optionsPanel);
        HideConfirmDialog();
        currentPanel = pauseMenuPanel;
    }
    
    private void HidePauseMenu()
    {
        HidePanel(overlayPanel);
        HidePanel(pauseMenuPanel);
        HidePanel(optionsPanel);
        HideConfirmDialog();
        currentPanel = null;
    }
    
    #endregion
    
    #region Button Handlers
    
    private void OnResumeClicked()
    {
        Debug.Log("Resume clicked");
        Resume();
    }
    
    private void OnMainMenuClicked()
    {
        Debug.Log("Main menu clicked");
        HidePanel(pauseMenuPanel);
        
        ShowConfirmDialog(
            "RETURN TO MENU?",
            "Your progress will not be saved.",
            OnMainMenuConfirmed,
            () => {
                HideConfirmDialog();
                ShowMainPanel();
            }
        );
    }
    
    private void OnMainMenuConfirmed()
    {
        Debug.Log("Returning to main menu...");
        
        // Restore time scale before loading
        Time.timeScale = 1f;
        isPaused = false;
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    #endregion
}
