using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Manages game options including audio settings and key bindings.
/// Persists settings using PlayerPrefs.
/// </summary>
public class OptionsManager : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Display Options")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    [Header("Key Binding Buttons")]
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button fireButton;
    [SerializeField] private Button pauseButton;

    [Header("Buttons")]
    [SerializeField] private Button resetDefaultsButton;
    [SerializeField] private Button applyButton;

    // PlayerPrefs keys
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string FullscreenKey = "Fullscreen";
    private const string VSyncKey = "VSync";

    // Events for other systems to listen to
    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;

    // Static access to current settings
    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 1f;
    public static float SFXVolume { get; private set; } = 1f;

    // Rebinding state
    private bool isRebinding = false;

    private void Awake()
    {
        LoadSettings();
    }

    private void Start()
    {
        InitializeUI();
        WireUpListeners();
        UpdateKeyBindingLabels();
    }

    private void InitializeUI()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = MasterVolume;
            UpdateSliderLabel(masterVolumeSlider, MasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = MusicVolume;
            UpdateSliderLabel(musicVolumeSlider, MusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = SFXVolume;
            UpdateSliderLabel(sfxVolumeSlider, SFXVolume);
        }

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        if (vsyncToggle != null)
            vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
    }

    private void WireUpListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(OnVSyncToggled);

        // Key binding buttons
        if (moveUpButton != null)
            moveUpButton.onClick.AddListener(() => StartRebindMoveComposite("up", moveUpButton));

        if (moveDownButton != null)
            moveDownButton.onClick.AddListener(() => StartRebindMoveComposite("down", moveDownButton));

        if (moveLeftButton != null)
            moveLeftButton.onClick.AddListener(() => StartRebindMoveComposite("left", moveLeftButton));

        if (moveRightButton != null)
            moveRightButton.onClick.AddListener(() => StartRebindMoveComposite("right", moveRightButton));

        if (fireButton != null)
            fireButton.onClick.AddListener(() => StartRebindAction(GameInputManager.Instance?.FireAction, 0, fireButton));

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => StartRebindAction(GameInputManager.Instance?.PauseAction, 0, pauseButton));

        if (resetDefaultsButton != null)
            resetDefaultsButton.onClick.AddListener(ResetToDefaults);

        if (applyButton != null)
            applyButton.onClick.AddListener(SaveSettings);
    }

    private void OnMasterVolumeSliderChanged(float value)
    {
        MasterVolume = value;
        UpdateSliderLabel(masterVolumeSlider, value);
        OnMasterVolumeChanged?.Invoke(value);
        AudioListener.volume = value;
    }

    private void OnMusicVolumeSliderChanged(float value)
    {
        MusicVolume = value;
        UpdateSliderLabel(musicVolumeSlider, value);
        OnMusicVolumeChanged?.Invoke(value);
    }

    private void OnSFXVolumeSliderChanged(float value)
    {
        SFXVolume = value;
        UpdateSliderLabel(sfxVolumeSlider, value);
        OnSFXVolumeChanged?.Invoke(value);
    }

    private void UpdateSliderLabel(Slider slider, float value)
    {
        if (slider == null || slider.transform.parent == null)
            return;

        Transform valueTransform = slider.transform.parent.Find("Value");
        if (valueTransform == null)
            return;

        string percentText = $"{Mathf.RoundToInt(value * 100)}%";

        // Try TMP first (via reflection to avoid compile issues)
        var tmpComponent = valueTransform.GetComponent("TextMeshProUGUI");
        if (tmpComponent != null)
        {
            var textProperty = tmpComponent.GetType().GetProperty("text");
            if (textProperty != null)
            {
                textProperty.SetValue(tmpComponent, percentText);
                return;
            }
        }

        // Fallback to standard UI Text
        var textComponent = valueTransform.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = percentText;
        }
    }

    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void OnVSyncToggled(bool vsyncEnabled)
    {
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
    }

    #region Key Rebinding

    private void UpdateKeyBindingLabels()
    {
        var input = GameInputManager.Instance;
        if (input == null) return;

        var moveBindings = input.GetMoveBindingDisplayNames();
        UpdateButtonLabel(moveUpButton, moveBindings.up);
        UpdateButtonLabel(moveDownButton, moveBindings.down);
        UpdateButtonLabel(moveLeftButton, moveBindings.left);
        UpdateButtonLabel(moveRightButton, moveBindings.right);
        UpdateButtonLabel(fireButton, input.GetFireBindingDisplayName());
        UpdateButtonLabel(pauseButton, input.GetPauseBindingDisplayName());
    }

    private void UpdateButtonLabel(Button button, string text)
    {
        if (button == null) return;

        // Try TMP via reflection
        var tmpTexts = button.GetComponentsInChildren<Component>();
        foreach (var comp in tmpTexts)
        {
            if (comp.GetType().Name == "TextMeshProUGUI" || comp.GetType().Name == "TMP_Text")
            {
                var textProperty = comp.GetType().GetProperty("text");
                if (textProperty != null)
                {
                    textProperty.SetValue(comp, text);
                    return;
                }
            }
        }

        // Fallback to Text
        var uiText = button.GetComponentInChildren<Text>();
        if (uiText != null)
        {
            uiText.text = text;
        }
    }

    private void StartRebindMoveComposite(string compositePart, Button button)
    {
        if (isRebinding) return;
        
        var moveAction = GameInputManager.Instance?.MoveAction;
        if (moveAction == null) return;

        // Find the binding index for this composite part
        int bindingIndex = -1;
        for (int i = 0; i < moveAction.bindings.Count; i++)
        {
            var binding = moveAction.bindings[i];
            if (binding.isPartOfComposite && binding.name.ToLower() == compositePart.ToLower())
            {
                bindingIndex = i;
                break;
            }
        }

        if (bindingIndex >= 0)
        {
            StartRebindAction(moveAction, bindingIndex, button);
        }
    }

    private void StartRebindAction(InputAction action, int bindingIndex, Button button)
    {
        if (isRebinding || action == null) return;

        isRebinding = true;
        UpdateButtonLabel(button, "Press key...");

        GameInputManager.Instance?.StartRebinding(
            action,
            bindingIndex,
            onComplete: (newBinding) =>
            {
                isRebinding = false;
                UpdateKeyBindingLabels();
            },
            onCancel: () =>
            {
                isRebinding = false;
                UpdateKeyBindingLabels();
            }
        );
    }

    #endregion

    public void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        AudioListener.volume = MasterVolume;

        Debug.Log("Settings loaded");
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt(VSyncKey, QualitySettings.vSyncCount);
        PlayerPrefs.Save();
        
        // Also save input bindings
        GameInputManager.Instance?.SaveBindings();
        
        Debug.Log("Settings saved");
    }

    public void ResetToDefaults()
    {
        MasterVolume = 1f;
        MusicVolume = 1f;
        SFXVolume = 1f;

        InitializeUI();
        AudioListener.volume = MasterVolume;
        
        // Reset input bindings
        GameInputManager.Instance?.ResetBindings();
        UpdateKeyBindingLabels();
        
        Debug.Log("Settings reset to defaults");
    }

    private void OnDestroy()
    {
        SaveSettings();
    }
}
