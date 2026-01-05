using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Abstract base class for menu controllers (MainMenu, PauseMenu).
/// Provides shared panel management, button wiring, and navigation logic.
/// </summary>
public abstract class BaseMenuController : MonoBehaviour
{
    [Header("Shared Panels")]
    [SerializeField] protected GameObject optionsPanel;
    
    [Header("Shared Components")]
    [SerializeField] protected ConfirmDialog confirmDialog;
    
    [Header("Shared Buttons")]
    [SerializeField] protected Button optionsButton;
    [SerializeField] protected Button optionsBackButton;
    [SerializeField] protected Button quitButton;
    
    protected GameObject currentPanel;
    protected GameObject previousPanel;
    
    protected virtual void Start()
    {
        WireSharedButtons();
        SubscribeToInput();
    }
    
    protected virtual void OnDestroy()
    {
        UnwireSharedButtons();
        UnsubscribeFromInput();
    }
    
    protected virtual void SubscribeToInput()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnCancelPressed += HandleCancelPressed;
    }
    
    protected virtual void UnsubscribeFromInput()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnCancelPressed -= HandleCancelPressed;
    }
    
    protected virtual void WireSharedButtons()
    {
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);
        
        if (optionsBackButton != null)
            optionsBackButton.onClick.AddListener(OnOptionsBackClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    protected virtual void UnwireSharedButtons()
    {
        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OnOptionsClicked);
        
        if (optionsBackButton != null)
            optionsBackButton.onClick.RemoveListener(OnOptionsBackClicked);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
    
    protected void ShowPanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(true);
    }
    
    protected void HidePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(false);
    }
    
    protected void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
    
    protected void ShowConfirmDialog(string title, string message, Action onConfirm, Action onCancel = null)
    {
        if (confirmDialog != null)
        {
            confirmDialog.Show(title, message, onConfirm, onCancel ?? (() => {
                HideConfirmDialog();
                ShowMainPanel();
            }));
        }
    }
    
    protected void HideConfirmDialog()
    {
        if (confirmDialog != null)
            confirmDialog.Hide();
    }
    
    protected bool IsConfirmDialogVisible()
    {
        return confirmDialog != null && confirmDialog.gameObject.activeSelf;
    }
    
    protected virtual void OnOptionsClicked()
    {
        Debug.Log("Opening options...");
        previousPanel = currentPanel;
        HidePanel(currentPanel);
        ShowPanel(optionsPanel);
        currentPanel = optionsPanel;
    }
    
    protected virtual void OnOptionsBackClicked()
    {
        Debug.Log("Closing options...");
        HidePanel(optionsPanel);
        ShowPanel(previousPanel);
        currentPanel = previousPanel;
    }
    
    protected virtual void OnQuitClicked()
    {
        Debug.Log("Quit requested...");
        HidePanel(currentPanel);
        ShowConfirmDialog(
            "QUIT GAME?",
            "Are you sure you want to quit?",
            OnQuitConfirmed,
            () => {
                HideConfirmDialog();
                ShowMainPanel();
            }
        );
    }
    
    protected virtual void OnQuitConfirmed()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    protected abstract void HandleCancelPressed();
    protected abstract void ShowMainPanel();
}
