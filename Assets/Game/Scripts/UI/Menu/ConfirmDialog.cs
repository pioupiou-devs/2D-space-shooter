using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Reusable confirmation dialog component
/// Used by both MainMenu and PauseMenu for quit/return confirmations
/// </summary>
public class ConfirmDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Component titleText;   // TextMeshProUGUI via reflection
    [SerializeField] private Component messageText; // TextMeshProUGUI via reflection
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Button Text (Optional)")]
    [SerializeField] private Component confirmButtonText; // TextMeshProUGUI via reflection
    [SerializeField] private Component cancelButtonText;  // TextMeshProUGUI via reflection
    
    private Action onConfirmCallback;
    private Action onCancelCallback;
    
    private void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Start hidden
        gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancelClicked);
    }
    
    /// <summary>
    /// Show the dialog with custom content
    /// </summary>
    public void Show(string title, string message, Action onConfirm, Action onCancel = null)
    {
        SetContent(title, message);
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Show with custom button labels
    /// </summary>
    public void Show(string title, string message, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel = null)
    {
        Show(title, message, onConfirm, onCancel);
        SetButtonLabels(confirmLabel, cancelLabel);
    }
    
    /// <summary>
    /// Hide the dialog
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        onConfirmCallback = null;
        onCancelCallback = null;
    }
    
    /// <summary>
    /// Set dialog content
    /// </summary>
    public void SetContent(string title, string message)
    {
        SetTextProperty(titleText, title);
        SetTextProperty(messageText, message);
    }
    
    /// <summary>
    /// Set button labels
    /// </summary>
    public void SetButtonLabels(string confirmLabel, string cancelLabel)
    {
        SetTextProperty(confirmButtonText, confirmLabel);
        SetTextProperty(cancelButtonText, cancelLabel);
    }
    
    private void SetTextProperty(Component textComponent, string value)
    {
        if (textComponent == null) return;
        
        var textProperty = textComponent.GetType().GetProperty("text");
        if (textProperty != null)
        {
            textProperty.SetValue(textComponent, value);
        }
    }
    
    private void OnConfirmClicked()
    {
        var callback = onConfirmCallback;
        Hide();
        callback?.Invoke();
    }
    
    private void OnCancelClicked()
    {
        var callback = onCancelCallback;
        Hide();
        callback?.Invoke();
    }
}
