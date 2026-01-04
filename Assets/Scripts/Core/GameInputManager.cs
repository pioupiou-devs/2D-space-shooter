using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Singleton manager that handles all game input using the new Input System.
/// Provides easy access to input actions and supports rebinding.
/// </summary>
public class GameInputManager : MonoBehaviour
{
    private static GameInputManager _instance;
    public static GameInputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameInputManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameInputManager");
                    _instance = go.AddComponent<GameInputManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private InputActionAsset _inputActions;
    
    // Action Maps
    private InputActionMap _gameplayMap;
    private InputActionMap _uiMap;

    // Gameplay Actions
    public InputAction MoveAction { get; private set; }
    public InputAction FireAction { get; private set; }
    public InputAction PauseAction { get; private set; }

    // UI Actions
    public InputAction NavigateAction { get; private set; }
    public InputAction SubmitAction { get; private set; }
    public InputAction CancelAction { get; private set; }

    // Events
    public event Action<Vector2> OnMove;
    public event Action OnFirePressed;
    public event Action OnFireReleased;
    public event Action OnPausePressed;
    public event Action OnCancelPressed;

    // State
    public Vector2 MoveInput { get; private set; }
    public bool IsFireHeld { get; private set; }

    private const string BindingsKey = "InputBindings";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInputActions();
        SetupActions();
        LoadBindings();
        EnableGameplay();
    }

    private void LoadInputActions()
    {
        _inputActions = Resources.Load<InputActionAsset>("GameInputActions");
        
        if (_inputActions == null)
        {
            // Try loading from Settings folder
            _inputActions = UnityEngine.Resources.Load<InputActionAsset>("GameInputActions");
        }

        if (_inputActions == null)
        {
            Debug.LogWarning("GameInputActions asset not found. Please run 'Tools > 2D Space Shooter > Generate Input Actions' and move the asset to Resources folder.");
            CreateDefaultActions();
        }
    }

    private void CreateDefaultActions()
    {
        // Create minimal input actions programmatically as fallback
        _inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
        
        // Gameplay map
        _gameplayMap = _inputActions.AddActionMap("Gameplay");
        
        var moveAction = _gameplayMap.AddAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        var fireAction = _gameplayMap.AddAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Keyboard>/space");
        fireAction.AddBinding("<Mouse>/leftButton");

        var pauseAction = _gameplayMap.AddAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");

        // UI map
        _uiMap = _inputActions.AddActionMap("UI");
        
        var navigateAction = _uiMap.AddAction("Navigate", InputActionType.PassThrough);
        navigateAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        var submitAction = _uiMap.AddAction("Submit", InputActionType.Button);
        submitAction.AddBinding("<Keyboard>/enter");

        var cancelAction = _uiMap.AddAction("Cancel", InputActionType.Button);
        cancelAction.AddBinding("<Keyboard>/escape");
    }

    private void SetupActions()
    {
        if (_inputActions == null) return;

        _gameplayMap = _inputActions.FindActionMap("Gameplay");
        _uiMap = _inputActions.FindActionMap("UI");

        if (_gameplayMap != null)
        {
            MoveAction = _gameplayMap.FindAction("Move");
            FireAction = _gameplayMap.FindAction("Fire");
            PauseAction = _gameplayMap.FindAction("Pause");
        }

        if (_uiMap != null)
        {
            NavigateAction = _uiMap.FindAction("Navigate");
            SubmitAction = _uiMap.FindAction("Submit");
            CancelAction = _uiMap.FindAction("Cancel");
        }

        // Subscribe to events
        if (MoveAction != null)
        {
            MoveAction.performed += ctx => 
            {
                MoveInput = ctx.ReadValue<Vector2>();
                OnMove?.Invoke(MoveInput);
            };
            MoveAction.canceled += ctx => 
            {
                MoveInput = Vector2.zero;
                OnMove?.Invoke(MoveInput);
            };
        }

        if (FireAction != null)
        {
            FireAction.performed += ctx => 
            {
                IsFireHeld = true;
                OnFirePressed?.Invoke();
            };
            FireAction.canceled += ctx => 
            {
                IsFireHeld = false;
                OnFireReleased?.Invoke();
            };
        }

        if (PauseAction != null)
        {
            PauseAction.performed += ctx => OnPausePressed?.Invoke();
        }

        if (CancelAction != null)
        {
            CancelAction.performed += ctx => OnCancelPressed?.Invoke();
        }
    }

    public void EnableGameplay()
    {
        _uiMap?.Disable();
        _gameplayMap?.Enable();
    }

    public void EnableUI()
    {
        _gameplayMap?.Disable();
        _uiMap?.Enable();
    }

    public void EnableAll()
    {
        _gameplayMap?.Enable();
        _uiMap?.Enable();
    }

    public void DisableAll()
    {
        _gameplayMap?.Disable();
        _uiMap?.Disable();
    }

    #region Rebinding

    /// <summary>
    /// Get the display name for a binding on an action
    /// </summary>
    public string GetBindingDisplayName(InputAction action, int bindingIndex = 0)
    {
        if (action == null || bindingIndex >= action.bindings.Count)
            return "None";

        return InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    /// <summary>
    /// Start interactive rebinding for an action
    /// </summary>
    public void StartRebinding(InputAction action, int bindingIndex, Action<string> onComplete, Action onCancel = null)
    {
        if (action == null) return;

        action.Disable();

        var rebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                action.Enable();
                string newBinding = GetBindingDisplayName(action, bindingIndex);
                SaveBindings();
                onComplete?.Invoke(newBinding);
                operation.Dispose();
            })
            .OnCancel(operation =>
            {
                action.Enable();
                onCancel?.Invoke();
                operation.Dispose();
            });

        rebind.Start();
    }

    /// <summary>
    /// Get composite binding display names (for WASD-style bindings)
    /// </summary>
    public (string up, string down, string left, string right) GetMoveBindingDisplayNames()
    {
        if (MoveAction == null)
            return ("W", "S", "A", "D");

        string up = "W", down = "S", left = "A", right = "D";

        for (int i = 0; i < MoveAction.bindings.Count; i++)
        {
            var binding = MoveAction.bindings[i];
            if (binding.isPartOfComposite)
            {
                string displayName = GetBindingDisplayName(MoveAction, i);
                switch (binding.name.ToLower())
                {
                    case "up": up = displayName; break;
                    case "down": down = displayName; break;
                    case "left": left = displayName; break;
                    case "right": right = displayName; break;
                }
            }
        }

        return (up, down, left, right);
    }

    public string GetFireBindingDisplayName()
    {
        return GetBindingDisplayName(FireAction, 0);
    }

    public string GetPauseBindingDisplayName()
    {
        return GetBindingDisplayName(PauseAction, 0);
    }

    #endregion

    #region Persistence

    public void SaveBindings()
    {
        if (_inputActions == null) return;
        
        string json = _inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(BindingsKey, json);
        PlayerPrefs.Save();
    }

    public void LoadBindings()
    {
        if (_inputActions == null) return;

        string json = PlayerPrefs.GetString(BindingsKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            _inputActions.LoadBindingOverridesFromJson(json);
        }
    }

    public void ResetBindings()
    {
        if (_inputActions == null) return;

        foreach (var map in _inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        
        PlayerPrefs.DeleteKey(BindingsKey);
        PlayerPrefs.Save();
    }

    #endregion

    private void OnDestroy()
    {
        if (_instance == this)
        {
            DisableAll();
            _instance = null;
        }
    }
}
