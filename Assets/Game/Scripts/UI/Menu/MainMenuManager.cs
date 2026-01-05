using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu functionality including navigation between panels
/// and scene transitions. Extends BaseMenuController for shared functionality.
/// </summary>
public class MainMenuManager : BaseMenuController
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Scene References")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;

    protected override void Start()
    {
        base.Start();

        WireMainMenuButtons();

        // Enable UI input mode
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.EnableUI();

        ShowMainPanel();
    }

    protected override void OnDestroy()
    {
        UnwireMainMenuButtons();
        base.OnDestroy();
    }

    private void WireMainMenuButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
    }

    private void UnwireMainMenuButtons()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
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
        else
        {
            OnQuitClicked();
        }
    }

    protected override void ShowMainPanel()
    {
        HidePanel(optionsPanel);
        HideConfirmDialog();
        ShowPanel(mainMenuPanel);
        currentPanel = mainMenuPanel;
    }

    #endregion

    #region Main Menu Specific Handlers

    public void OnPlayClicked()
    {
        Debug.Log("Starting game...");

        // Unsubscribe and switch to gameplay input before loading scene
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnCancelPressed -= HandleCancelPressed;
            GameInputManager.Instance.EnableGameplay();
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    #endregion
}
