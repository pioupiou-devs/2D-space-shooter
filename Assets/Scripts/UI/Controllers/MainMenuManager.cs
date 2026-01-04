using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu functionality including navigation between panels
/// and scene transitions.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("Scene References")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsBackButton;
    [SerializeField] private Button quitConfirmButton;
    [SerializeField] private Button quitCancelButton;

    private void Start()
    {
        ShowMainMenu();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (optionsBackButton != null)
            optionsBackButton.onClick.AddListener(OnOptionsBackClicked);

        if (quitConfirmButton != null)
            quitConfirmButton.onClick.AddListener(OnQuitConfirmed);

        if (quitCancelButton != null)
            quitCancelButton.onClick.AddListener(OnQuitCancelled);

        // Subscribe to input events
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnCancelPressed += HandleCancelPressed;
            GameInputManager.Instance.EnableUI();
        }
    }

    private void HandleCancelPressed()
    {
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            OnOptionsBackClicked();
        }
        else if (quitConfirmPanel != null && quitConfirmPanel.activeSelf)
        {
            OnQuitCancelled();
        }
        else
        {
            OnQuitClicked();
        }
    }

    #region Button Handlers

    public void OnPlayClicked()
    {
        Debug.Log("Starting game...");

        // Enable gameplay input before loading scene
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnCancelPressed -= HandleCancelPressed;
            GameInputManager.Instance.EnableGameplay();
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnOptionsClicked()
    {
        Debug.Log("Opening options...");
        ShowOptions();
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit requested...");
        ShowQuitConfirm();
    }

    public void OnOptionsBackClicked()
    {
        Debug.Log("Closing options...");
        ShowMainMenu();
    }

    public void OnQuitConfirmed()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnQuitCancelled()
    {
        Debug.Log("Quit cancelled...");
        ShowMainMenu();
    }

    #endregion

    #region Panel Management

    private void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(quitConfirmPanel, false);
    }

    private void ShowOptions()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(optionsPanel, true);
        SetPanelActive(quitConfirmPanel, false);
    }

    private void ShowQuitConfirm()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(quitConfirmPanel, true);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    #endregion

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OnOptionsClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);

        if (optionsBackButton != null)
            optionsBackButton.onClick.RemoveListener(OnOptionsBackClicked);

        if (quitConfirmButton != null)
            quitConfirmButton.onClick.RemoveListener(OnQuitConfirmed);

        if (quitCancelButton != null)
            quitCancelButton.onClick.RemoveListener(OnQuitCancelled);

        // Unsubscribe from input events
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnCancelPressed -= HandleCancelPressed;
        }
    }
}
